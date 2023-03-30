using System;
using Qwerty.ECS.Runtime.Chunks;

#if UNITY_EDITOR
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
#endif

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Jobs
{
#if UNITY_EDITOR
    [Unity.Jobs.LowLevel.Unsafe.JobProducerType(typeof(UnityJobsWorker.ParallelForJobChunk<>))]
#endif
    public interface IParallelForJobChunk
    {
        void Execute(EcsChunkAccessor chunk);
    }

#if UNITY_EDITOR
    internal static partial class UnityJobsWorker
    {
        public static unsafe JobHandle Execute<T>(this T jobData, int arrayLength, int innerLoopBatchCount,
            IntPtr chunks) where T : struct, IParallelForJobChunk
        {
            JobsExtension.JobChunkWrapper<T> wrapper = new JobsExtension.JobChunkWrapper<T>()
            {
                jobData = jobData,
                chunks = chunks,
                length = arrayLength
            };

            JobsUtility.JobScheduleParameters scheduleParams = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref wrapper), ParallelForJobChunk<T>.Initialize(), default,
                ScheduleMode.Parallel);

            return JobsUtility.ScheduleParallelFor(ref scheduleParams, arrayLength, innerLoopBatchCount);
        }

        internal struct ParallelForJobChunk<T> where T : struct, IParallelForJobChunk
        {
            private static IntPtr m_jobReflectionData;

            public static IntPtr Initialize()
            {
                if (m_jobReflectionData == IntPtr.Zero)
                {
                    m_jobReflectionData = JobsUtility.CreateJobReflectionData(typeof(JobsExtension.JobChunkWrapper<T>),
                        typeof(T), (ExecuteJobFunction)Execute);
                }

                return m_jobReflectionData;
            }

            private delegate void ExecuteJobFunction(ref JobsExtension.JobChunkWrapper<T> jobWrapper,
                IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);

            private static unsafe void Execute(ref JobsExtension.JobChunkWrapper<T> jobWrapper, IntPtr additionalPtr,
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
    }
#endif
}