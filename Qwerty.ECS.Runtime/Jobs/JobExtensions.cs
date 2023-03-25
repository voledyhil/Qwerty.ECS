// ReSharper disable once CheckNamespace
using System;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Chunks;
using Unity.Collections.LowLevel.Unsafe;

#if UNITY_EDITOR
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
#if UNITY_EDITOR
            int chunksCount = archetypeGroup.CalculateChunksCount();
            IntPtr chunks = GetChunks(archetypeGroup.archetypesChunks, archetypeGroup.archetypesCount, chunksCount);
            Schedule(jobData, chunksCount, innerLoopBatchCount, chunks).Complete();
            MemoryUtil.Free(chunks);
#endif
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
#endif
    }
}



