
// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    internal unsafe class EcsArchetypeManager
    {
        private readonly EcsWorldSetting m_setting;
        internal int archetypeCounter => m_indexCounter;
        internal EcsArchetype empty => m_emptyArchetype;
        internal EcsArchetype this[int index] => m_archetypes[index];
        
        private int m_indexCounter;
        private readonly EcsArchetype m_emptyArchetype;
        private readonly EcsArchetype[] m_archetypes;
        private readonly byte[] m_indicesBuffer;
        
        private readonly PrimeStorage* m_primeStorage;

        public EcsArchetypeManager(EcsWorldSetting setting)
        {
            m_primeStorage = (PrimeStorage*)MemoryUtilities.Alloc<PrimeStorage>(1);
            m_primeStorage->Alloc();
            
            m_setting = setting;
            m_indicesBuffer = new byte[EcsTypeManager.typeCount];
            m_archetypes = new EcsArchetype[setting.archetypeCapacity];
            m_emptyArchetype = new EcsArchetype(m_indexCounter, new byte[] { }, setting.archetypeChunkSizeInByte, m_primeStorage);
            m_archetypes[m_indexCounter++] = m_emptyArchetype;
        }
        
        internal EcsArchetype FindOrCreateArchetype(byte[] typeIndicesBuffer, int typeIndicesLen)
        {
            EcsArchetype current = m_emptyArchetype;
            for (int i = 0; i < typeIndicesLen; i++)
            {
                byte index = typeIndicesBuffer[i];
                if (!current.next.TryGetValue(index, out EcsArchetype next))
                {
                    byte[] archetypeIndices = new byte[i + 1];
                    for (int j = 0; j < archetypeIndices.Length; j++)
                    {
                        archetypeIndices[j] = typeIndicesBuffer[j];
                    }
                    next = new EcsArchetype(m_indexCounter, archetypeIndices, m_setting.archetypeChunkSizeInByte, m_primeStorage);
                    next.prior[index] = current;
                    current.next[index] = next;
                    m_archetypes[m_indexCounter++] = next;
                }
                current = next;
            }
            return current;
        }

        internal EcsArchetype FindOrCreateNextArchetype(EcsArchetype archetype, byte nextIndex)
        {
            if (archetype.next.TryGetValue(nextIndex, out EcsArchetype next))
            {
                return next;
            }

            bool added = false;
            int length = 0;
            
            byte[] typeIndices = archetype.typeIndices;
            foreach (byte typeIndex in typeIndices)
            {
                if (nextIndex < typeIndex && !added)
                {
                    m_indicesBuffer[length++] = nextIndex;
                    added = true;
                }

                m_indicesBuffer[length++] = typeIndex;
            }

            if (!added)
            {
                m_indicesBuffer[length++] = nextIndex;
            }

            return FindOrCreateArchetype(m_indicesBuffer, length);
        }
        
        internal EcsArchetype FindOrCreatePriorArchetype(EcsArchetype archetype, byte priorIndex)
        {
            if (archetype.next.TryGetValue(priorIndex, out EcsArchetype prior))
            {
                return prior;
            }
            int length = 0;
            byte[] typeIndices = archetype.typeIndices;
            for (int index = 0; index < typeIndices.Length; index++)
            {
                byte typeIndex = typeIndices[index];
                if (typeIndex != priorIndex)
                {
                    m_indicesBuffer[length++] = typeIndex;
                }
            }
            return FindOrCreateArchetype(m_indicesBuffer, length);
        }

        internal void Dispose()
        {
            m_primeStorage->Dispose();
            MemoryUtilities.Free((IntPtr)m_primeStorage);
        }
    }
}