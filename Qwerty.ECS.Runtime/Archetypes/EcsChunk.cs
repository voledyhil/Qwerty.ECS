using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime.Archetypes
{
    internal unsafe struct EcsChunk
    {
        internal int rowByteSize => m_rowByteSize;
        internal EcsChunk* prior;
        
        internal IntPtr body;
        internal int* start;
        internal int* count;
        
        private int m_rowByteSize;

        public const int HeaderSize = 400;

        public void Alloc(int sizeInBytes, int rowByteSize)
        {
            body = MemoryUtil.Alloc(HeaderSize + sizeInBytes);
            start = (int*)MemoryUtil.Alloc<int>();
            count = (int*)MemoryUtil.Alloc<int>();
            m_rowByteSize = rowByteSize;
        }
        
        private const int MaxComponentCount = 20;
        private const ushort Length = 47;
        public void FillHeader(byte[] indices)
        {
            short rowCapacityInBytes = 0;
            foreach (short typeIndex in indices)
            {
                WhiteOffset(typeIndex, rowCapacityInBytes);
                rowCapacityInBytes += (short)EcsTypeManager.Sizes[typeIndex];
            }
        }

        private const int SizeOfInt = sizeof(int);

        private void WhiteOffset(short typeIndex, short value)
        {
            typeIndex++;
            int hash = typeIndex % Length;
            int curKey = *(int*)(body + SizeOfInt * hash) >> 16;
            while (curKey > 0)
            {
                hash = (hash + 1) % Length;
                curKey = *(int*)(body + SizeOfInt * hash) >> 16;
            }
            *(int*)(body + SizeOfInt * hash) = (typeIndex << 16) | (value & 0xFFFF);
        }
        
        public int ReadOffset(short typeIndex)
        {
            typeIndex++;
            int hash = typeIndex % Length;
            int amount = *(int*)(body + SizeOfInt * hash);
            int curKey = amount >> 16;
            while (curKey > 0)
            {
                if (curKey == typeIndex)
                {
                    return amount & 0xFFFF;
                }
                hash = (hash + 1) % Length;
                curKey = *(int*)(body + SizeOfInt * hash) >> 16;
            }
            throw new ArgumentException(nameof(ReadOffset));
        }
        
        internal EcsEntity ReadEntity(int index, int offset)
        {
            return Unsafe.Read<EcsEntity>((void*)(body + HeaderSize + m_rowByteSize * index + offset));
        }
        
        internal void WriteEntity(int index, int offset, EcsEntity entity)
        {
            Unsafe.Write((void*)(body + HeaderSize + m_rowByteSize * index + offset), entity);
        }
        
        internal T ReadComponent<T>(int index, int typeIndex) where T : struct, IEcsComponent
        {
            return Unsafe.Read<T>((void*)(body + HeaderSize + m_rowByteSize * index + ReadOffset((short)typeIndex)));
        }

        internal void WriteComponent<T>(int index, int typeIndex, T component) where T : struct, IEcsComponent
        {
            Unsafe.Write((void*)(body + HeaderSize + m_rowByteSize * index + ReadOffset((short)typeIndex)), component);
        }

        internal void Dispose()
        {
            MemoryUtil.Free(body);
            MemoryUtil.Free((IntPtr)start);
            MemoryUtil.Free((IntPtr)count);
        }
    }
}