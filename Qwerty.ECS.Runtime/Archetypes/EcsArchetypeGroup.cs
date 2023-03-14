
// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    public class EcsArchetypeGroup : IDisposable
    {
        public int Version { get; private set; }

        internal readonly List<EcsArchetype> archetypes = new List<EcsArchetype>();
        private EcsArchetypeAccessor m_archetypeAccessor;
        
        internal void ChangeVersion(int newVersion)
        {
            if (Version > 0)
            {
                m_archetypeAccessor.Dispose();
            }
            
            m_archetypeAccessor = new EcsArchetypeAccessor(archetypes);
            Version = newVersion;
        }
        
        public int CalculateEntitiesCount()
        {
            unsafe
            {
                int count = 0;
                foreach (EcsArchetype archetype in archetypes)
                {
                    EcsArchetypeChunk* chunk = archetype.chunks->last;
                    if (chunk != null)
                    {
                        count += *chunk->start + *chunk->count;
                    }
                }
                return count;
            }
        }

        public ref EcsArchetypeAccessor GetEntityAccessor()
        {
            return ref m_archetypeAccessor;
        }
        
        public unsafe EcsArchetypeAccessor* GetEntityAccessorPtr()
        {
            fixed (EcsArchetypeAccessor* accessor = &m_archetypeAccessor)
            {
                return accessor;
            }
        }

        public void Dispose()
        {
            m_archetypeAccessor.Dispose();
        }
    }
}