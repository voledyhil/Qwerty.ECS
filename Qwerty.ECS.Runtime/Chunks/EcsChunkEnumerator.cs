using System;
using Qwerty.ECS.Runtime.Archetypes;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Chunks
{
    public unsafe ref struct EcsChunkEnumerator
    {
        private int m_archetypeIndex;
        private EcsChunk* m_chunk;
        private readonly IntPtr m_archetypes;
        private readonly int m_archetypesCount;

        public EcsChunkEnumerator(IntPtr archetypes, int archetypesCount)
        {
            m_archetypes = archetypes;
            m_archetypesCount = archetypesCount;
            m_archetypeIndex = -1;
            m_chunk = null;
        }

        public EcsChunkAccessor Current => new EcsChunkAccessor(m_chunk);
        
        public bool MoveNext()
        {
            while (true)
            {
                if (m_chunk != null && m_chunk->prior != null)
                {
                    m_chunk = m_chunk->prior;
                    return true;
                }

                if (++m_archetypeIndex >= m_archetypesCount)
                {
                    return false;
                }

                IntPtr intPtr = MemoryUtil.ReadElement<IntPtr>(m_archetypes, m_archetypeIndex);
                m_chunk = ((EcsArchetype.Chunks*)intPtr)->last;
                
                if (m_chunk != null)
                {
                    return true;
                }
            }
        }
    }
}