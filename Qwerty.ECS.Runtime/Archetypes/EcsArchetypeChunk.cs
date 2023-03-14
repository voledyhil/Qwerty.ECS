using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime.Archetypes
{
    internal unsafe struct EcsArchetypeChunk
    {
        public int rowCapacityInBytes => m_rowCapacityInBytes;
        public EcsArchetypeChunk* prior;
        public byte* body;
        public int* start;
        public int* count;
        
        private int m_rowCapacityInBytes;
        public IntMap* map;
        public UnsafeArray* offsets;
        private int m_entityOffset;

        public void Alloc(int sizeInBytes, int rowCapacityInBytes, IntMap* map, UnsafeArray* offsets)
        {
            body = (byte*)MemoryUtilities.Alloc(sizeInBytes, true);
            start = (int*)MemoryUtilities.Alloc<int>(1);
            count = (int*)MemoryUtilities.Alloc<int>(1);
            
            this.map = map;
            this.offsets = offsets;
            
            m_rowCapacityInBytes = rowCapacityInBytes;
            m_entityOffset = offsets->Read<int>(offsets->length - 1);
        }

        public EcsEntity ReadEntity(int index)
        {
            return Unsafe.Read<EcsEntity>((void*)((IntPtr)body + m_rowCapacityInBytes * index + m_entityOffset));
        }
        
        public void WriteEntity(int index, EcsEntity entity)
        {
            Unsafe.Write((void*)((IntPtr)body + m_rowCapacityInBytes * index + m_entityOffset), entity);
        }
        
        public T ReadComponent<T>(int index) where T : struct, IEcsComponent
        {
            int offset = offsets->Read<int>(map->Get(EcsComponentType<T>.index));
            return Unsafe.Read<T>((void*)((IntPtr)body + m_rowCapacityInBytes * index + offset));
        }

        public void WriteComponent<T>(int index, T component) where T : struct, IEcsComponent
        {
            int offset = offsets->Read<int>(map->Get(EcsComponentType<T>.index));
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