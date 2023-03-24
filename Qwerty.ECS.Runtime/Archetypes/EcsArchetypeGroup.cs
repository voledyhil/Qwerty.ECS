using System;
using System.Collections.Generic;
using Qwerty.ECS.Runtime.Chunks;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    public class EcsArchetypeGroup : IDisposable
    {
        public int Version { get; private set; }

        internal readonly List<EcsArchetype> archetypes = new List<EcsArchetype>();
        internal IntPtr archetypesChunks;
        internal int archetypesCount;
        
        private readonly int m_sizeOfIntPtr = MemoryUtil.SizeOf<IntPtr>();

        internal EcsArchetypeGroup()
        {
            
        }

        internal unsafe void ChangeVersion(int newVersion)
        {
            if (Version > 0)
            {
                MemoryUtil.Free(archetypesChunks);
            }
            
            archetypesChunks = MemoryUtil.Alloc((uint)(m_sizeOfIntPtr * archetypes.Count));
            archetypesCount = archetypes.Count;
            for (int i = 0; i < archetypesCount; i++)
            {
                MemoryUtil.Write(archetypesChunks, m_sizeOfIntPtr * i, (IntPtr)archetypes[i].chunks);
            }
            Version = newVersion;
        }

        public unsafe int CalculateEntitiesCount()
        {
            int count = 0;
            foreach (EcsArchetype archetype in archetypes)
            {
                EcsChunk* chunk = archetype.chunks->last;
                if (chunk != null)
                {
                    count += chunk->index * chunk->header->chunkCapacity + *chunk->count;
                }
            }
            return count;
        }
        
        public unsafe int CalculateChunksCount()
        {
            int count = 0;
            foreach (EcsArchetype archetype in archetypes)
            {
                EcsChunk* chunk = archetype.chunks->last;
                if (chunk != null)
                {
                    count += chunk->index + 1;
                }
            }
            return count;
        }
        
        public EcsChunkEnumerator GetEnumerator()
        {
            return new EcsChunkEnumerator(archetypesChunks, archetypesCount);
        }
        
        public void Dispose()
        {
            MemoryUtil.Free(archetypesChunks);
        }
    }
}