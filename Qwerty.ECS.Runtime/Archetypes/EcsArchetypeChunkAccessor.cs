using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime.Archetypes
{
    public unsafe readonly struct EcsArchetypeChunkAccessor
    {
        public int count => m_count;
        private readonly byte* m_bodyPtr;
        private readonly int m_count;
        private readonly int m_rowCapacityInBytes;
        private readonly UnsafeArray* m_offsets;
        private readonly IntMap* m_map;

        internal EcsArchetypeChunkAccessor(byte* bodyPtr, int count, int rowCapacityInBytes, IntMap* map, UnsafeArray* offsets)
        {
            m_bodyPtr = bodyPtr;
            m_count = count;
            m_offsets = offsets;
            m_map = map;
            m_rowCapacityInBytes = rowCapacityInBytes;
        }

        public EcsArchetypeEntityAccessor GetEntityAccessor()
        {
            return new EcsArchetypeEntityAccessor(m_bodyPtr, m_rowCapacityInBytes);
        }
        
        public EcsArchetypeComponentAccessor<T> GetComponentAccessor<T>() where T : struct, IEcsComponent
        {
            int offset = m_offsets->Read<int>(m_map->Get(EcsComponentType<T>.index));
            return new EcsArchetypeComponentAccessor<T>(m_bodyPtr, m_rowCapacityInBytes, offset);
        }
    }
}