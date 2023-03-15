using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime.Archetypes
{
    internal unsafe struct EcsChunk
    {
        public int rowCapacityInBytes => m_rowCapacityInBytes;
        public EcsChunk* prior;
        public byte* body;
        public int* start;
        public int* count;
        
        private int m_rowCapacityInBytes;
        public IntMap* offsetMap;

        public void Alloc(int sizeInBytes, int rowCapacityInBytes, IntMap* offsetMap)
        {
            body = (byte*)MemoryUtil.Alloc(sizeInBytes);
            start = (int*)MemoryUtil.Alloc<int>(1);
            count = (int*)MemoryUtil.Alloc<int>(1);
            m_rowCapacityInBytes = rowCapacityInBytes;
            
            this.offsetMap = offsetMap;
        }

        internal EcsEntity ReadEntity(int index, int offset)
        {
            return Unsafe.Read<EcsEntity>((void*)((IntPtr)body + m_rowCapacityInBytes * index + offset));
        }
        
        internal void WriteEntity(int index, int offset, EcsEntity entity)
        {
            Unsafe.Write((void*)((IntPtr)body + m_rowCapacityInBytes * index + offset), entity);
        }
        
        internal T ReadComponent<T>(int index) where T : struct, IEcsComponent
        {
            int offset = offsetMap->Get(EcsComponentType<T>.index);
            return Unsafe.Read<T>((void*)((IntPtr)body + m_rowCapacityInBytes * index + offset));
        }

        internal void WriteComponent<T>(int index, T component) where T : struct, IEcsComponent
        {
            int offset = offsetMap->Get(EcsComponentType<T>.index);
            Unsafe.Write((void*)((IntPtr)body + m_rowCapacityInBytes * index + offset), component);
        }

        internal void Dispose()
        {
            MemoryUtil.Free((IntPtr)body);
            MemoryUtil.Free((IntPtr)start);
            MemoryUtil.Free((IntPtr)count);
            prior = null;
        }
    }
}