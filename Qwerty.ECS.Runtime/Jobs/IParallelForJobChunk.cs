using Qwerty.ECS.Runtime.Chunks;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Jobs
{
#if UNITY_EDITOR
    [Unity.Jobs.LowLevel.Unsafe.JobProducerType(typeof(JobExtensions.ParallelForJobChunk<>))]
#endif
    public interface IParallelForJobChunk
    {
        void Execute(EcsChunkAccessor chunk);
    }
}