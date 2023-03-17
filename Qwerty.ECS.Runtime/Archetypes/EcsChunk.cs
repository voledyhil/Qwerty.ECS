using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime.Archetypes
{
    internal unsafe struct EcsChunk
    {
        internal int rowByteSize => m_rowByteSize;
        internal EcsChunk* prior;
        internal IntMap* offsets;
        
        internal byte* body;
        internal int* start;
        internal int* count;
        
        private int m_rowByteSize;

        public void Alloc(int sizeInBytes, int rowByteSize, IntMap* offsets)
        {
            body = (byte*)MemoryUtil.Alloc(sizeInBytes);
            start = (int*)MemoryUtil.Alloc<int>();
            count = (int*)MemoryUtil.Alloc<int>();
            m_rowByteSize = rowByteSize;
            
            this.offsets = offsets;
        }

        internal EcsEntity ReadEntity(int index, int offset)
        {
            return Unsafe.Read<EcsEntity>((void*)((IntPtr)body + m_rowByteSize * index + offset));
        }
        
        internal void WriteEntity(int index, int offset, EcsEntity entity)
        {
            Unsafe.Write((void*)((IntPtr)body + m_rowByteSize * index + offset), entity);
        }
        
        internal T ReadComponent<T>(int index, int typeIndex) where T : struct, IEcsComponent
        {
            return Unsafe.Read<T>((void*)((IntPtr)body + m_rowByteSize * index + offsets->Get(typeIndex)));
        }

        internal void WriteComponent<T>(int index, int typeIndex, T component) where T : struct, IEcsComponent
        {
            Unsafe.Write((void*)((IntPtr)body + m_rowByteSize * index + offsets->Get(typeIndex)), component);
        }

        internal void Dispose()
        {
            MemoryUtil.Free((IntPtr)body);
            MemoryUtil.Free((IntPtr)start);
            MemoryUtil.Free((IntPtr)count);
        }
    }
}