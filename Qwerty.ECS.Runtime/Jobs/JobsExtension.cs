// ReSharper disable once RedundantUsingDirective
using System;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Chunks;

#if UNITY_EDITOR
using Unity.Jobs;
#endif

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Jobs
{
    public static class JobsExtension
    {
        public static void RunAsParallel<T>(this T jobData, EcsArchetypeGroup archetypeGroup) where T : struct, IParallelForJobChunk
        {
            int chunksCount = archetypeGroup.CalculateChunksCount();
            IntPtr chunks = GetChunks(archetypeGroup.archetypesChunks, archetypeGroup.archetypesCount, chunksCount);
#if UNITY_EDITOR
            JobHandle handle = jobData.Execute(chunksCount, 1, chunks);
            handle.Complete();
#else
            ThreadPoolWorker worker = new ThreadPoolWorker(chunksCount);
            worker.Execute(jobData, chunks);
#endif
            MemoryUtil.Free(chunks);
        }

        public static void RunAsSingle<T>(this T jobData, EcsArchetypeGroup archetypeGroup) where T : struct, IJobChunk
        {
            int chunksCount = archetypeGroup.CalculateChunksCount();
            IntPtr chunks = GetChunks(archetypeGroup.archetypesChunks, archetypeGroup.archetypesCount, chunksCount);
#if UNITY_EDITOR
            JobHandle handle = jobData.Execute(chunksCount, chunks);
            handle.Complete();
#else
            throw new NotImplementedException(nameof(RunAsSingle));
#endif
            MemoryUtil.Free(chunks);
        }
        
        internal struct JobChunkWrapper<T> where T : struct
        {
            public T jobData;
            public int length;
#if UNITY_EDITOR
            [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction] public IntPtr chunks;
#endif
        }
        
        private static unsafe IntPtr GetChunks(IntPtr archetypes, int archetypesCount, int chunksCount)
        {
            int index = 0;
            int archetypeIndex = 0;
            IntPtr chunks = MemoryUtil.Alloc<IntPtr>(chunksCount);
            while (archetypeIndex < archetypesCount)
            {
                IntPtr intPtr = MemoryUtil.ReadElement<IntPtr>(archetypes, archetypeIndex++);
                EcsChunk* chunk = ((EcsArchetype.Chunks*)intPtr)->last;
                while (chunk != null)
                {
                    MemoryUtil.WriteElement(chunks, index++, (IntPtr)chunk);
                    chunk = chunk->prior;
                }
            }
            return chunks;
        }
    }
}



