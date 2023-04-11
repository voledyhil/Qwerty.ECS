using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    internal class EcsArchetypeManager : IDisposable
    {
        internal int count => m_archetypes.Count;
        internal EcsArchetype empty => m_emptyArchetype;
        internal EcsArchetype this[int index] => m_archetypes[index];
        
        private readonly EcsArchetype m_emptyArchetype;
        private readonly List<EcsArchetype> m_archetypes = new List<EcsArchetype>();
        private readonly short[] m_indicesBuffer;
        private readonly EcsWorldSetting m_setting;

        public EcsArchetypeManager(EcsWorldSetting setting)
        {
            m_setting = setting;
            m_indicesBuffer = new short[EcsTypeManager.typeCount];
            m_emptyArchetype = new EcsArchetype(m_archetypes.Count, Array.Empty<short>(), m_setting);
            m_archetypes.Add(m_emptyArchetype);
        }
        
        internal EcsArchetype FindOrCreateArchetype(short[] indicesBuffer, int indicesLen)
        {
            EcsArchetype current = m_emptyArchetype;
            for (int i = 0; i < indicesLen; i++)
            {
                short index = indicesBuffer[i];
                if (!current.next.TryGetValue(index, out int nextId))
                {
                    short[] newIndices = new short[i + 1];
                    Array.Copy(indicesBuffer, newIndices, i + 1);
                    EcsArchetype next = new EcsArchetype(m_archetypes.Count, newIndices, m_setting);
                    m_archetypes.Add(next);
                    next.prior[index] = current.index;
                    current.next[index] = next.index;
                    nextId = next.index;
                }
                current = m_archetypes[nextId];
            }
            return current;
        }

        internal EcsArchetype FindOrCreateNextArchetype(EcsArchetype archetype, short nextIndex)
        {
            if (archetype.next.TryGetValue(nextIndex, out int next))
            {
                return m_archetypes[next];
            }

            bool added = false;
            int len = 0;
            
            short[] typeIndices = archetype.indices;
            foreach (short typeIndex in typeIndices)
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
        
        internal EcsArchetype FindOrCreatePriorArchetype(EcsArchetype archetype, short priorIndex)
        {
            if (archetype.prior.TryGetValue(priorIndex, out int prior))
            {
                return m_archetypes[prior];
            }
            
            int len = 0;
            short[] typeIndices = archetype.indices;
            foreach (short typeIndex in typeIndices)
            {
                if (typeIndex != priorIndex)
                {
                    m_indicesBuffer[len++] = typeIndex;
                }
            }
            return FindOrCreateArchetype(m_indicesBuffer, len);
        }

        public void Dispose()
        {
            foreach (EcsArchetype archetype in m_archetypes)
            {
                archetype.Dispose();
            }
        }
    }
}