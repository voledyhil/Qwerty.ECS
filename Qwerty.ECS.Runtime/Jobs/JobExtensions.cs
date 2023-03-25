// ReSharper disable once CheckNamespace
using System;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Chunks;

#if UNITY_EDITOR
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
#endif


// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Jobs
{
    public static class JobExtensions
    {
        public static void Run<T>(this T jobData, EcsArchetypeGroup archetypeGroup, int innerLoopBatchCount)
            where T : struct, IParallelForJobChunk
        {
            int chunksCount = archetypeGroup.CalculateChunksCount();
            IntPtr chunks = GetChunks(archetypeGroup.archetypesChunks, archetypeGroup.archetypesCount, chunksCount);
#if UNITY_EDITOR
            Schedule(jobData, chunksCount, innerLoopBatchCount, chunks).Complete();
#else
            ParallelForWorker worker = new ParallelForWorker();
            worker.Execute(jobData, chunksCount, innerLoopBatchCount, chunks);
#endif
            MemoryUtil.Free(chunks);
        }

        private static unsafe IntPtr GetChunks(IntPtr archetypes, int archetypesCount, int chunksCount)
        {
            int index = 0;
            int archetypeIndex = 0;

            int sizeOfIntPtr = MemoryUtil.SizeOf<IntPtr>();
            IntPtr chunks = MemoryUtil.Alloc((uint)(sizeOfIntPtr * chunksCount));
            while (archetypeIndex < archetypesCount)
            {
                IntPtr intPtr = MemoryUtil.Read<IntPtr>(archetypes, archetypeIndex++ * sizeOfIntPtr);
                EcsChunk* chunk = ((EcsArchetype.Chunks*)intPtr)->last;
                while (chunk != null)
                {
                    MemoryUtil.Write(chunks, sizeOfIntPtr * index++, (IntPtr)chunk);
                    chunk = chunk->prior;
                }
            }

            return chunks;
        }

        private struct JobChunkWrapper<T> where T : struct
        {
            public T jobData;
#if UNITY_EDITOR
            [NativeDisableUnsafePtrRestriction]
#endif
            public IntPtr chunks;
        }

#if UNITY_EDITOR
        private static unsafe JobHandle Schedule<T>(this T jobData, int arrayLength, int innerLoopBatchCount,
            IntPtr chunks) where T : struct, IParallelForJobChunk
        {
            JobChunkWrapper<T> wrapper = new JobChunkWrapper<T>()
            {
                jobData = jobData,
                chunks = chunks
            };

            JobsUtility.JobScheduleParameters scheduleParams = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref wrapper), ParallelForJobChunk<T>.Initialize(), default,
                ScheduleMode.Parallel);

            return JobsUtility.ScheduleParallelFor(ref scheduleParams, arrayLength, innerLoopBatchCount);
        }
#else

        private class ParallelForWorker
        {
            private struct JobState<T> where T : struct, IParallelForJobChunk
            {
                public JobChunkWrapper<T> wrapper;
                public int from;
                public int to;
            }
            
            private int m_threads;
            public void Execute<T>(T jobData, int arrayLength, int innerLoopBatchCount, IntPtr chunks) where T : struct, IParallelForJobChunk
            {
                JobChunkWrapper<T> wrapper = new JobChunkWrapper<T>()
                {
                    jobData = jobData,
                    chunks = chunks
                };

                m_threads = Math.Max(arrayLength / innerLoopBatchCount, 1);

                int threads = m_threads;
                int remainder = arrayLength % threads;
                int slice = arrayLength / threads + (remainder == 0 ? 0 : 1);
                for (int t = 0; t < threads; t++)
                {
                    int from = t * slice;
                    int to = from + slice;
                    if (to > arrayLength)
                        to = arrayLength;

                    if (from != to)
                    {
                        ThreadPool.QueueUserWorkItem(queueOnThread, new JobState<T> { wrapper = wrapper, from = from, to = to, }, true);
                    }
                    else
                    {
                        Interlocked.Decrement(ref m_threads);
                    }
                }
                while (m_threads > 0) { }
            }

            private unsafe void queueOnThread<T>(JobState<T> state) where T : struct, IParallelForJobChunk
            {
                JobChunkWrapper<T> wrapper = state.wrapper;
                IntPtr chunks = wrapper.chunks;
                T jobData = wrapper.jobData;

                for (int index = state.from; index < state.to; index++)
                {
                    EcsChunk* chunk = (EcsChunk*)MemoryUtil.Read<IntPtr>(chunks, index * sizeof(IntPtr));
                    jobData.Execute(new EcsChunkAccessor(chunk));
                }

                Interlocked.Decrement(ref m_threads);
            }
        }
#endif


#if UNITY_EDITOR
        internal struct ParallelForJobChunk<T> where T : struct, IParallelForJobChunk
        {
            private static IntPtr m_jobReflectionData;

            public static IntPtr Initialize()
            {
                if (m_jobReflectionData == IntPtr.Zero)
                {
                    m_jobReflectionData = JobsUtility.CreateJobReflectionData(typeof(JobChunkWrapper<T>), typeof(T),
                        (ExecuteJobFunction)Execute);
                }

                return m_jobReflectionData;
            }

            private delegate void ExecuteJobFunction(ref JobChunkWrapper<T> jobWrapper, IntPtr additionalPtr,
                IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);

            private static unsafe void Execute(ref JobChunkWrapper<T> jobWrapper, IntPtr additionalPtr,
                IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
            {
                T jobData = jobWrapper.jobData;

                while (true)
                {
                    if (!JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out int begin, out int end))
                    {
                        break;
                    }

                    JobsUtility.PatchBufferMinMaxRanges(bufferRangePatchData, UnsafeUtility.AddressOf(ref jobWrapper),
                        begin, end - begin);

                    for (int i = begin; i < end; i++)
                    {
                        EcsChunk* chunk = (EcsChunk*)MemoryUtil.Read<IntPtr>(jobWrapper.chunks, i * sizeof(IntPtr));
                        jobData.Execute(new EcsChunkAccessor(chunk));
                    }
                }
            }
        }
#endif
    }
}



