using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    public class EcsArchetypeGroup : IDisposable
    {
        public int Version { get; private set; }

        internal readonly List<EcsArchetype> archetypes = new List<EcsArchetype>();
        private EcsArchetypesAccessor m_archetypesAccessor;
        
        internal void ChangeVersion(int newVersion)
        {
            if (Version > 0)
            {
                m_archetypesAccessor.Dispose();
            }
            
            m_archetypesAccessor = new EcsArchetypesAccessor(archetypes);
            Version = newVersion;
        }
        
        public int CalculateCount()
        {
            int entitiesCount = 0;
            int archetypesCount = archetypes.Count;
            for (int i = 0; i < archetypesCount; i++)
            {
                entitiesCount += archetypes[i].Entities.count;
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