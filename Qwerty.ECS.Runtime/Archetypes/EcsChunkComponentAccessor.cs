using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime.Archetypes
{
    public unsafe readonly struct EcsChunkComponentAccessor<T> where T : struct, IEcsComponent
    {
        private readonly byte* m_bodyPtr;
        private readonly int m_rowCapacityInBytes;
        private readonly int m_offset;

        public EcsChunkComponentAccessor(byte* bodyPtr, int rowCapacityInBytes, int offset)
        {
            m_bodyPtr = bodyPtr;
            m_rowCapacityInBytes = rowCapacityInBytes;
            m_offset = offset;
        }

        public T this[int index]
        {
            get => Unsafe.Read<T>((void*)((IntPtr)m_bodyPtr + m_rowCapacityInBytes * index + m_offset));
            set => Unsafe.Write((void*)((IntPtr)m_bodyPtr + m_rowCapacityInBytes * index + m_offset), value);
        }
    }
}