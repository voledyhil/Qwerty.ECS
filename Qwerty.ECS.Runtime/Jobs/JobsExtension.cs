// ReSharper disable once CheckNamespace
using System;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Chunks;
using Unity.Jobs;


// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Jobs
{
    public static class JobsExtension
    {
        public static void Run<T>(this T jobData, EcsArchetypeGroup archetypeGroup, int innerLoopBatchCount)
            where T : struct, IParallelForJobChunk
        {
            int chunksCount = archetypeGroup.CalculateChunksCount();
            IntPtr chunks = GetChunks(archetypeGroup.archetypesChunks, archetypeGroup.archetypesCount, chunksCount);
#if UNITY_EDITOR
             JobHandle handle = jobData.Execute(chunksCount, innerLoopBatchCount, chunks);
             handle.Complete();
#else
            ThreadPoolWorker worker = new ThreadPoolWorker();
            worker.Execute(jobData, chunksCount, innerLoopBatchCount, chunks);
            worker.WaitComplete();
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



