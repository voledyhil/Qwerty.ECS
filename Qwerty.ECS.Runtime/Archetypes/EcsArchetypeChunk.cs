using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime.Archetypes
{
    internal unsafe struct EcsArchetypeChunk
    {
        public int sizeInBytes => m_sizeInBytes;
        public int rowCapacityInBytes => m_rowCapacityInBytes;
        public EcsArchetypeChunk* prior;
        public byte* body;
        public int* start;
        public int* count;

        private int m_sizeInBytes;
        private int m_rowCapacityInBytes;
        public EcsArchetypeComponentsMap* map;
        public UnsafeArray* offsets;

        public void Alloc(int sizeInBytes, int rowCapacityInBytes, EcsArchetypeComponentsMap* map, UnsafeArray* offsets)
        {
            body = (byte*)MemoryUtilities.Alloc(sizeInBytes, true);
            start = (int*)MemoryUtilities.Alloc<int>(1);
            count = (int*)MemoryUtilities.Alloc<int>(1);
            
            m_sizeInBytes = sizeInBytes;
            m_rowCapacityInBytes = rowCapacityInBytes;
            this.map = map;
            this.offsets = offsets;
        }

        public EcsEntity ReadEntity(int index)
        { 
            int offset = m_rowCapacityInBytes - Unsafe.SizeOf<EcsEntity>();
            return Unsafe.Read<EcsEntity>((void*)((IntPtr)body + m_rowCapacityInBytes * index + offset));
        }
        
        public void WriteEntity(int index, EcsEntity entity)
        {
            int offset = m_rowCapacityInBytes - Unsafe.SizeOf<EcsEntity>();
            Unsafe.Write((void*)((IntPtr)body + m_rowCapacityInBytes * index + offset), entity);
        }
        
        public T ReadComponent<T>(int index, int offset) where T : struct, IEcsComponent
        {
            return Unsafe.Read<T>((void*)((IntPtr)body + m_rowCapacityInBytes * index + offset));
        }

        public void WriteComponent<T>(int index, int offset, T component) where T : struct, IEcsComponent
        {
            Unsafe.Write((void*)((IntPtr)body + m_rowCapacityInBytes * index + offset), component);
        }

        internal void Dispose()
        {
            MemoryUtilities.Free((IntPtr)body);
            MemoryUtilities.Free((IntPtr)start);
            MemoryUtilities.Free((IntPtr)count);
            prior = null;
        }
    }
}