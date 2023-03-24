using Qwerty.ECS.Runtime.Chunks;
using Unity.Jobs.LowLevel.Unsafe;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Jobs
{
#if UNITY_EDITOR
    [JobProducerType(typeof(JobExtensions.ParallelForJobChunk<>))]
#endif
    public interface IParallelForJobChunk
    {
        void Execute(EcsChunkAccessor chunk);
    }
}