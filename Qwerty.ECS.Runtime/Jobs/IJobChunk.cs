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
    [JobProducerType(typeof(UnityJobsWorker.JobChunk<>))]
#endif
    public interface IJobChunk
    {
        void Execute(EcsChunkAccessor chunk);
    }
    
#if UNITY_EDITOR
    internal static partial class UnityJobsWorker
    {
        public static unsafe JobHandle Execute<T>(this T jobData, int arrayLength, IntPtr chunks) where T : struct, IJobChunk
        {
            JobsExtension.JobChunkWrapper<T> wrapper = new JobsExtension.JobChunkWrapper<T>()
            {
                jobData = jobData,
                chunks = chunks,
                length = arrayLength
            };
            
            JobsUtility.JobScheduleParameters scheduleParams = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref wrapper), JobChunk<T>.Initialize(), default, ScheduleMode.Single);
            return JobsUtility.Schedule(ref scheduleParams);
        }
        
        internal struct JobChunk<T> where T : struct, IJobChunk
        {
            private static IntPtr jobReflectionData;

            public static IntPtr Initialize()
            {
                if (jobReflectionData == IntPtr.Zero)
                {
                    jobReflectionData = JobsUtility.CreateJobReflectionData(typeof(JobsExtension.JobChunkWrapper<T>), typeof(T), (ExecuteJobFunction)Execute);
                }
                return jobReflectionData;
            }

            private delegate void ExecuteJobFunction(ref JobsExtension.JobChunkWrapper<T> jobWrapper, IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);

            private static unsafe void Execute(ref JobsExtension.JobChunkWrapper<T> jobWrapper, IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
            {
                T jobData = jobWrapper.jobData;
                int length = jobWrapper.length;
                IntPtr chunks = jobWrapper.chunks;
                
                for (int i = 0; i < length; i++)
                {
                    EcsChunk* chunk = (EcsChunk*)MemoryUtil.Read<IntPtr>(chunks, i * sizeof(IntPtr));
                    jobData.Execute(new EcsChunkAccessor(chunk));
                }
            }
        }
    }
#endif
}