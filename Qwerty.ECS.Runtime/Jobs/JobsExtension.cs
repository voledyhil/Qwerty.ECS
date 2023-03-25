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
        public static void Run<T>(this T jobData, EcsArchetypeGroup archetypeGroup)
            where T : struct, IParallelForJobChunk
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
    }
}



