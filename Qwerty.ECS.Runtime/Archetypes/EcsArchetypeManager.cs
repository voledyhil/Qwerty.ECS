namespace Qwerty.ECS.Runtime.Archetypes
{
    public class EcsArchetypeManager
    {
        internal int ArchetypeCount => m_archetypesLen;
        internal EcsArchetype Empty => m_emptyArchetype;
        
        private int m_archetypeIndexCounter;
        
        private readonly EcsArchetype m_emptyArchetype;
        private readonly EcsArchetype[] m_archetypes;
        private int m_archetypesLen;
        
        private readonly EcsArchetype[][] m_archetypeTypes;

        private readonly int[] m_archetypeTypesLen;
        private readonly byte[] m_indicesBuffer;

        public EcsArchetypeManager(int archetypeCapacity)
        {
            m_indicesBuffer = new byte[EcsTypeManager.TypeCount];
            m_archetypeTypes = new EcsArchetype[EcsTypeManager.TypeCount][];
            
            m_archetypeTypesLen = new int[archetypeCapacity];
            m_archetypes = new EcsArchetype[archetypeCapacity];
            
            m_emptyArchetype = new EcsArchetype(m_archetypeIndexCounter++, new byte[] { });
            m_archetypes[m_archetypesLen++] = m_emptyArchetype;

            for (int i = 0; i < m_archetypeTypes.Length; i++)
            {
                m_archetypeTypes[i] = new EcsArchetype[archetypeCapacity];
            }
        }

        internal EcsArchetype this[int index] => m_archetypes[index];
        
        internal int GetArchetypes(int startIndex, EcsArchetype[] buffer)
        {
            int len = 0;
            for (int i = startIndex; i < m_archetypesLen; i++)
            {
                buffer[len++] = m_archetypes[i];
            }
            return len;
        }
        
        internal int GetArchetypes(byte typeIndex, int archetypeIndex, EcsArchetype[] buffer)
        {
            int len = 0;
            EcsArchetype[] archetypes = m_archetypeTypes[typeIndex];
            int typeLen = m_archetypeTypesLen[typeIndex];
            for (int i = typeLen - 1; i >= 0; i--)
            {
                EcsArchetype archetype = archetypes[i];
                if (archetype.Index <= archetypeIndex)
                {
                    break;
                }
                buffer[len++] = archetype;
            }
            return len;
        }
        
        internal EcsArchetype FindOrCreateArchetype(byte[] typeIndicesBuffer, int typeIndicesLen)
        {
            Array.Sort(typeIndicesBuffer, 0, typeIndicesLen);
            return InnerFindOrCreateArchetype(typeIndicesBuffer, typeIndicesLen);
        }
        
        private EcsArchetype InnerFindOrCreateArchetype(byte[] typeIndicesBuffer, int typeIndicesLen)
        {
            EcsArchetype curArchetype = m_emptyArchetype;
            for (int i = 0; i < typeIndicesLen; i++)
            {
                byte index = typeIndicesBuffer[i];
                EcsArchetype nextArchetype = curArchetype.Next[index];
                if (nextArchetype == null)
                {
                    byte[] archetypeIndices = new byte[i + 1];
                    for (int j = 0; j < archetypeIndices.Length; j++)
                    {
                        archetypeIndices[j] = typeIndicesBuffer[j];
                    }

                    nextArchetype = new EcsArchetype(m_archetypeIndexCounter++, archetypeIndices);
                    nextArchetype.Prior[index] = curArchetype;
                    
                    byte[] typeIndices = nextArchetype.TypeIndices;
                    foreach (byte typeIndex in typeIndices)
                    {
                        m_archetypeTypes[typeIndex][m_archetypeTypesLen[typeIndex]++] = nextArchetype;
                    }

                    curArchetype.Next[index] = nextArchetype;
                    
                    m_archetypes[m_archetypesLen++] = nextArchetype;
                }

                curArchetype = nextArchetype;
            }
            return curArchetype;
        }

        internal EcsArchetype FindOrCreateNextArchetype(EcsArchetype archetype, byte addIndex)
        {
            EcsArchetype nextArchetype = archetype.Next[addIndex];
            if (nextArchetype != null)
            {
                return nextArchetype;
            }

            bool added = false;
            int length = 0;
            
            byte[] typeIndices = archetype.TypeIndices;
            foreach (byte typeIndex in typeIndices)
            {
                if (addIndex < typeIndex && !added)
                {
                    m_indicesBuffer[length++] = addIndex;
                    added = true;
                }

                m_indicesBuffer[length++] = typeIndex;
            }

            if (!added)
            {
                m_indicesBuffer[length++] = addIndex;
            }

            return InnerFindOrCreateArchetype(m_indicesBuffer, length);
        }
        
        internal EcsArchetype FindOrCreatePriorArchetype(EcsArchetype archetype, byte removeIndex)
        {
            EcsArchetype priorArchetype = archetype.Prior[removeIndex];
            if (priorArchetype != null)
            {
                return priorArchetype;
            }

            int length = 0;
            
            byte[] typeIndices = archetype.TypeIndices;
            foreach (byte typeIndex in typeIndices)
            {
                if (typeIndex != removeIndex)
                {
                    m_indicesBuffer[length++] = typeIndex;
                }
            }

            return InnerFindOrCreateArchetype(m_indicesBuffer, length);
        }
    }
}