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

        private IntPtr m_archetypes;
        private readonly int m_sizeOfIntPtr = MemoryUtil.SizeOf<IntPtr>();
        private int m_archetypesCount;

        internal unsafe void ChangeVersion(int newVersion)
        {
            if (Version > 0)
            {
                MemoryUtil.Free(m_archetypes);
            }
            
            m_archetypes = MemoryUtil.Alloc((uint)(m_sizeOfIntPtr * archetypes.Count));
            m_archetypesCount = archetypes.Count;
            for (int i = 0; i < m_archetypesCount; i++)
            {
                MemoryUtil.Write(m_archetypes, m_sizeOfIntPtr * i, (IntPtr)archetypes[i].chunks);
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

        public EcsArchetypeGroupAccessor GetAccessor()
        {
            return new EcsArchetypeGroupAccessor(m_archetypes, m_archetypesCount);
        }
        
        public void Dispose()
        {
            MemoryUtil.Free(m_archetypes);
        }
    }
}