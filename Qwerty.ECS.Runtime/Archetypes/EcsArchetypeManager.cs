
// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    internal unsafe class EcsArchetypeManager
    {
        private readonly EcsWorldSetting m_setting;
        internal int archetypeCount => m_archetypes.Count;
        internal EcsArchetype empty => m_emptyArchetype;
        internal EcsArchetype this[int index] => m_archetypes[index];
        
        private readonly EcsArchetype m_emptyArchetype;
        private readonly List<EcsArchetype> m_archetypes = new List<EcsArchetype>();
        private readonly byte[] m_indicesBuffer;
        
        private readonly PrimeStorage* m_primeStorage;

        public EcsArchetypeManager(EcsWorldSetting setting)
        {
            m_primeStorage = (PrimeStorage*)MemoryUtilities.Alloc<PrimeStorage>(1);
            m_primeStorage->Alloc();
            
            m_setting = setting;
            m_indicesBuffer = new byte[EcsTypeManager.typeCount];
            m_emptyArchetype = new EcsArchetype(m_archetypes.Count, Array.Empty<byte>(), setting.archetypeChunkSizeInByte, m_primeStorage);
            m_archetypes.Add(m_emptyArchetype);
        }
        
        internal EcsArchetype FindOrCreateArchetype(byte[] indicesBuffer, int indicesLen)
        {
            EcsArchetype current = m_emptyArchetype;
            for (int i = 0; i < indicesLen; i++)
            {
                byte index = indicesBuffer[i];
                if (!current.next.TryGetValue(index, out int nextId))
                {
                    byte[] newIndices = new byte[i + 1];
                    Array.Copy(indicesBuffer, newIndices, i + 1);
                    EcsArchetype next = new EcsArchetype(m_archetypes.Count, newIndices, m_setting.archetypeChunkSizeInByte, m_primeStorage);
                    m_archetypes.Add(next);
                    next.prior[index] = current.index;
                    current.next[index] = next.index;
                    nextId = next.index;
                }
                current = m_archetypes[nextId];
            }
            return current;
        }

        internal EcsArchetype FindOrCreateNextArchetype(EcsArchetype archetype, byte nextIndex)
        {
            if (archetype.next.TryGetValue(nextIndex, out int next))
            {
                return m_archetypes[next];
            }

            bool added = false;
            int len = 0;
            
            byte[] typeIndices = archetype.indices;
            foreach (byte typeIndex in typeIndices)
            {
                if (nextIndex < typeIndex && !added)
                {
                    m_indicesBuffer[len++] = nextIndex;
                    added = true;
                }
                m_indicesBuffer[len++] = typeIndex;
            }
            if (!added)
            {
                m_indicesBuffer[len++] = nextIndex;
            }

            return FindOrCreateArchetype(m_indicesBuffer, len);
        }
        
        internal EcsArchetype FindOrCreatePriorArchetype(EcsArchetype archetype, byte priorIndex)
        {
            if (archetype.prior.TryGetValue(priorIndex, out int prior))
            {
                return m_archetypes[prior];
            }
            
            int len = 0;
            byte[] typeIndices = archetype.indices;
            foreach (byte typeIndex in typeIndices)
            {
                if (typeIndex != priorIndex)
                {
                    m_indicesBuffer[len++] = typeIndex;
                }
            }
            return FindOrCreateArchetype(m_indicesBuffer, len);
        }

        internal void Dispose()
        {
            m_primeStorage->Dispose();
            MemoryUtilities.Free((IntPtr)m_primeStorage);
        }
    }
}