using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime.Archetypes
{
    internal unsafe struct EcsChunk
    {
        internal EcsChunk* prior;
        internal EcsChunkHeader* header;
        internal IntPtr body;
        internal int* count;

        public void Alloc(int bodySizeInBytes, EcsChunkHeader* header)
        {
            body = MemoryUtil.Alloc(bodySizeInBytes);
            count = (int*)MemoryUtil.Alloc<int>();
            this.header = header;
        }
        
        internal EcsEntity ReadEntity(int index)
        {
            return Unsafe.Read<EcsEntity>((void*)(body + header->rowSizeInBytes * index + header->entityOffset));
        }
        
        internal void WriteEntity(int index, EcsEntity entity)
        {
            Unsafe.Write((void*)(body + header->rowSizeInBytes * index + header->entityOffset), entity);
        }
        
        internal T ReadComponent<T>(int index, int typeIndex) where T : struct, IEcsComponent
        {
            return Unsafe.Read<T>((void*)(body + header->rowSizeInBytes * index + header->ReadOffsetByType((short)typeIndex)));
        }

        internal void WriteComponent<T>(int index, int typeIndex, T component) where T : struct, IEcsComponent
        {
            Unsafe.Write((void*)(body + header->rowSizeInBytes * index + header->ReadOffsetByType((short)typeIndex)), component);
        }

        internal void Dispose()
        {
            MemoryUtil.Free(body);
            MemoryUtil.Free((IntPtr)count);
        }
    }
}