using System;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Chunks
{
    internal struct EcsChunk
    {
        internal unsafe EcsChunk* prior;
        internal unsafe EcsChunkHeader* header;
        internal IntPtr body;
        internal unsafe int* count;
        internal int index;

        public unsafe void Alloc(int index, uint bodySizeInBytes, EcsChunkHeader* chunkHeader)
        {
            this.index = index;
            body = MemoryUtil.Alloc(bodySizeInBytes);
            count = (int*)MemoryUtil.Alloc<int>();
            header = chunkHeader;
        }
        
        internal unsafe EcsEntity ReadEntity(int index)
        {
            return MemoryUtil.Read<EcsEntity>(body, header->rowSizeInBytes * index + header->entityOffset);
        }
        
        internal unsafe void WriteEntity(int index, EcsEntity entity)
        {
            MemoryUtil.Write(body, header->rowSizeInBytes * index + header->entityOffset, entity);
        }
        
        internal unsafe T ReadComponent<T>(int index, int typeIndex) where T : struct, IEcsComponent
        {
            return MemoryUtil.Read<T>(body, header->rowSizeInBytes * index + header->ReadOffsetByType(typeIndex));
        }

        internal unsafe void WriteComponent<T>(int index, int typeIndex, T component) where T : struct, IEcsComponent
        {
            MemoryUtil.Write(body, header->rowSizeInBytes * index + header->ReadOffsetByType(typeIndex), component);
        }

        internal unsafe void Dispose()
        {
            MemoryUtil.Free(body);
            MemoryUtil.Free((IntPtr)count);
        }
    }
}