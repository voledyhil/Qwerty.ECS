using Qwerty.ECS.Runtime.Archetypes;

namespace Qwerty.ECS.Runtime.Chunks
{
    public unsafe ref struct EcsChunkEnumerator
    {
        private int m_archetypeIndex;
        private EcsChunk* m_chunk;
        private readonly UnsafeArray* m_archetypes;

        public EcsChunkEnumerator(UnsafeArray* archetypes)
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
        
        public EcsChunkAccessor Current => new EcsChunkAccessor(m_chunk);
        
        public void Reset()
        {
            m_archetypeIndex = -1;
            m_chunk = null;
        }
    }
}