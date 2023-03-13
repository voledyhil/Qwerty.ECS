using System.Collections;

namespace Qwerty.ECS.Runtime.Archetypes
{
    public unsafe struct EcsArchetypeChunkEnumerator : IEnumerator<EcsArchetypeChunkAccessor>
    {
        private int m_archetypeIndex;
        private EcsArchetypeChunk* m_chunk;
        private readonly UnsafeArray* m_archetypes;

        public EcsArchetypeChunkEnumerator(UnsafeArray* archetypes)
        {
            m_archetypes = archetypes;
            m_archetypeIndex = -1;
            m_chunk = null;
        }

        public bool MoveNext()
        {
            while (true)
            {
                if (m_chunk != null && m_chunk->prior != null)
                {
                    m_chunk = m_chunk->prior;
                    return true;
                }

                if (++m_archetypeIndex >= m_archetypes->length)
                {
                    return false;
                }
                
                m_chunk = ((EcsArchetype.Chunks*)m_archetypes->Get<IntPtr>(m_archetypeIndex))->last;
                
                if (m_chunk != null)
                {
                    return true;
                }
            }
        }

        public void Reset()
        {
            m_archetypeIndex = -1;
            m_chunk = null;
        }

        public EcsArchetypeChunkAccessor Current => new EcsArchetypeChunkAccessor(m_chunk->body, *m_chunk->count, m_chunk->rowCapacityInBytes, m_chunk->map, m_chunk->offsets);
        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}