using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime.Archetypes
{
    public unsafe readonly struct EcsChunkAccessor
    {
        public int count => m_count;
        private readonly byte* m_bodyPtr;
        private readonly int m_count;
        private readonly int m_rowCapacityInBytes;
        private readonly UnsafeArray* m_offsets;
        private readonly IntMap* m_map;

        internal EcsChunkAccessor(byte* bodyPtr, int count, int rowCapacityInBytes, IntMap* map, UnsafeArray* offsets)
        {
            m_bodyPtr = bodyPtr;
            m_count = count;
            m_offsets = offsets;
            m_map = map;
            m_rowCapacityInBytes = rowCapacityInBytes;
        }

        public EcsChunkEntityAccessor GetEntityAccessor()
        {
            return new EcsChunkEntityAccessor(m_bodyPtr, m_rowCapacityInBytes);
        }
        
        public EcsChunkComponentAccessor<T> GetComponentAccessor<T>() where T : struct, IEcsComponent
        {
            int offset = m_offsets->Read<int>(m_map->Get(EcsComponentType<T>.index));
            return new EcsChunkComponentAccessor<T>(m_bodyPtr, m_rowCapacityInBytes, offset);
        }
    }
}