
// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    public class EcsArchetypeGroup : IDisposable
    {
        public int Version { get; private set; }

        internal readonly List<EcsArchetype> archetypes = new List<EcsArchetype>();

        private unsafe UnsafeArray* m_archetypesRoot;
        internal unsafe void ChangeVersion(int newVersion)
        {
            if (Version > 0)
            {
                m_archetypesRoot->Dispose();
            }
            
            m_archetypesRoot = (UnsafeArray*)MemoryUtil.Alloc<UnsafeArray>();
            m_archetypesRoot->Alloc<IntPtr>(archetypes.Count);
            for (int i = 0; i < archetypes.Count; i++)
            {
                m_archetypesRoot->Write(i, (IntPtr)archetypes[i].chunks);
            }
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
                    while (chunk != null)
                    {
                        count += *chunk->count;
                        chunk = chunk->prior;
                    }
                }
                return count;
            }
        }

        public unsafe EcsArchetypeGroupAccessor GetAccessor()
        {
            return new EcsArchetypeGroupAccessor(m_archetypesRoot);
        }
        
        public unsafe void Dispose()
        {
            m_archetypesRoot->Dispose();
        }
    }
}