using System;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    public class EcsArchetypeManager
    {
        internal int archetypeCount => m_archetypesLen;
        internal EcsArchetype empty => m_emptyArchetype;
        
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
            EcsArchetype current = m_emptyArchetype;
            for (int i = 0; i < typeIndicesLen; i++)
            {
                byte index = typeIndicesBuffer[i];
                EcsArchetype next = current.Next[index];
                if (next == null)
                {
                    byte[] archetypeIndices = new byte[i + 1];
                    for (int j = 0; j < archetypeIndices.Length; j++)
                    {
                        archetypeIndices[j] = typeIndicesBuffer[j];
                    }

                    next = new EcsArchetype(m_archetypeIndexCounter++, archetypeIndices);
                    next.Prior[index] = current;
                    
                    byte[] typeIndices = next.TypeIndices;
                    foreach (byte typeIndex in typeIndices)
                    {
                        m_archetypeTypes[typeIndex][m_archetypeTypesLen[typeIndex]++] = next;
                    }

                    current.Next[index] = next;
                    
                    m_archetypes[m_archetypesLen++] = next;
                }
                current = next;
            }
            return current;
        }

        internal EcsArchetype FindOrCreateNextArchetype(EcsArchetype archetype, byte nextIndex)
        {
            EcsArchetype next = archetype.Next[nextIndex];
            if (next != null)
            {
                return next;
            }

            bool added = false;
            int length = 0;
            
            byte[] typeIndices = archetype.TypeIndices;
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
            EcsArchetype prior = archetype.Prior[priorIndex];
            if (prior != null)
            {
                return prior;
            }

            int length = 0;
            
            byte[] typeIndices = archetype.TypeIndices;
            foreach (byte typeIndex in typeIndices)
            {
                if (typeIndex != priorIndex)
                {
                    m_indicesBuffer[length++] = typeIndex;
                }
            }

            return FindOrCreateArchetype(m_indicesBuffer, length);
        }
    }
}