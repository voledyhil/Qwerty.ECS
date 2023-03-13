using System.Runtime.CompilerServices;

namespace Qwerty.ECS.Runtime.Archetypes
{
    public unsafe readonly struct EcsArchetypeEntityAccessor
    {
        private readonly byte* m_bodyPtr;
        private readonly int m_rowCapacityInBytes;
        private readonly int m_offset;

        public EcsArchetypeEntityAccessor(byte* bodyPtr, int rowCapacityInBytes)
        {
            m_bodyPtr = bodyPtr;
            m_rowCapacityInBytes = rowCapacityInBytes;
            m_offset = rowCapacityInBytes - Unsafe.SizeOf<EcsEntity>();
        }
        
        public EcsEntity this[int index] => Unsafe.Read<EcsEntity>((void*)((IntPtr)m_bodyPtr + m_rowCapacityInBytes * index + m_offset));
    }
}