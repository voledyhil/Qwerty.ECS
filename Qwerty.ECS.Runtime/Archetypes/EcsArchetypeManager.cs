
// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    public class EcsArchetypeManager
    {
        private readonly EcsWorldSetting m_setting;
        internal int archetypeCount => m_archetypesLen;
        internal EcsArchetype empty => m_emptyArchetype;
        internal EcsArchetype this[int index] => m_archetypes[index];
        
        private int m_archetypeIndexCounter;
        private int m_archetypesLen;
        private readonly EcsArchetype m_emptyArchetype;
        private readonly EcsArchetype[] m_archetypes;
        private readonly byte[] m_indicesBuffer;

        public EcsArchetypeManager(EcsWorldSetting setting)
        {
            m_setting = setting;
            m_indicesBuffer = new byte[EcsTypeManager.typeCount];
            m_archetypes = new EcsArchetype[setting.archetypeCapacity];
            m_emptyArchetype = new EcsArchetype(m_archetypeIndexCounter++, new byte[] { }, setting.archetypeChunkSizeInByte, setting.archetypeMaxComponents);
            m_archetypes[m_archetypesLen++] = m_emptyArchetype;
        }
        
        internal EcsArchetype FindOrCreateArchetype(byte[] typeIndicesBuffer, int typeIndicesLen)
        {
            EcsArchetype current = m_emptyArchetype;
            for (int i = 0; i < typeIndicesLen; i++)
            {
                byte index = typeIndicesBuffer[i];
                EcsArchetype next = current.next[index];
                if (next == null)
                {
                    byte[] archetypeIndices = new byte[i + 1];
                    for (int j = 0; j < archetypeIndices.Length; j++)
                    {
                        archetypeIndices[j] = typeIndicesBuffer[j];
                    }

                    next = new EcsArchetype(m_archetypeIndexCounter++, archetypeIndices, m_setting.archetypeChunkSizeInByte, m_setting.archetypeMaxComponents);
                    next.prior[index] = current;
                    current.next[index] = next;
                    
                    m_archetypes[m_archetypesLen++] = next;
                }
                current = next;
            }
            return current;
        }

        internal EcsArchetype FindOrCreateNextArchetype(EcsArchetype archetype, byte nextIndex)
        {
            EcsArchetype next = archetype.next[nextIndex];
            if (next != null)
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
            EcsArchetype prior = archetype.prior[priorIndex];
            if (prior != null)
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
    }
}