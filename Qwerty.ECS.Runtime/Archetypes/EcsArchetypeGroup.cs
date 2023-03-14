
// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    public class EcsArchetypeGroup : IDisposable
    {
        public int Version { get; private set; }

        internal readonly List<EcsArchetype> archetypes = new List<EcsArchetype>();
        private EcsArchetypeGroupAccessor m_archetypeGroupAccessor;
        
        internal void ChangeVersion(int newVersion)
        {
            if (Version > 0)
            {
                m_archetypeGroupAccessor.Dispose();
            }
            
            m_archetypeGroupAccessor = new EcsArchetypeGroupAccessor(archetypes);
            Version = newVersion;
        }
        
        public int CalculateEntitiesCount()
        {
            unsafe
            {
                int count = 0;
                foreach (EcsArchetype archetype in archetypes)
                {
                    EcsChunk* chunk = archetype.chunks->last;
                    if (chunk != null)
                    {
                        count += *chunk->start + *chunk->count;
                    }
                }
                return count;
            }
        }

        public ref EcsArchetypeGroupAccessor GetEntityAccessor()
        {
            return ref m_archetypeGroupAccessor;
        }
        
        public unsafe EcsArchetypeGroupAccessor* GetEntityAccessorPtr()
        {
            fixed (EcsArchetypeGroupAccessor* accessor = &m_archetypeGroupAccessor)
            {
                return accessor;
            }
        }

        public void Dispose()
        {
            m_archetypeGroupAccessor.Dispose();
        }
    }
}