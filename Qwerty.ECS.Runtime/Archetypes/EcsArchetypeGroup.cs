using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    public class EcsArchetypeGroup : IDisposable
    {
        public int Version { get; private set; }
        
        private readonly List<EcsArchetype> m_archetypes;
        private EcsArchetypesAccessor m_archetypesAccessor;
        
        internal EcsArchetypeGroup(int version, IEnumerable<EcsArchetype> archetypes)
        {
            Version = version;

            m_archetypes = new List<EcsArchetype>(archetypes);
            m_archetypesAccessor = new EcsArchetypesAccessor(m_archetypes);
        }
        
        public void Update(int newVersion, IEnumerable<EcsArchetype> newArchetypes)
        {
            Version = newVersion;
            m_archetypes.AddRange(newArchetypes);
            
            m_archetypesAccessor.Dispose();
            m_archetypesAccessor = new EcsArchetypesAccessor(m_archetypes);
        }
        
        public int CalculateCount()
        {
            int entitiesCount = 0;
            int archetypesCount = m_archetypes.Count;
            for (int i = 0; i < archetypesCount; i++)
            {
                entitiesCount += m_archetypes[i].Entities.count;
            }
            return entitiesCount;
        }

        public ref EcsArchetypesAccessor GetArchetypeAccessor()
        {
            return ref m_archetypesAccessor;
        }
        
        public unsafe EcsArchetypesAccessor* GetArchetypeAccessorPtr()
        {
            fixed (EcsArchetypesAccessor* accessor = &m_archetypesAccessor)
            {
                return accessor;
            }
        }

        public void Dispose()
        {
            m_archetypesAccessor.Dispose();
        }
    }
}