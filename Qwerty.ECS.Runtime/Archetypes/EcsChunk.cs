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

        public void Alloc(int bodySizeInBytes, int rowByteSize, byte[] indices)
        {
            body = MemoryUtil.Alloc(HeaderSize + bodySizeInBytes);
            start = (int*)MemoryUtil.Alloc<int>();
            count = (int*)MemoryUtil.Alloc<int>();
            m_rowByteSize = rowByteSize;
            
            short sizeInBytes = 0;
            for (short index = 0; index < indices.Length; index++)
            {
                short typeIndex = indices[index];
                short tIndex = typeIndex;
                
                tIndex++;
                int hash = tIndex % Length;
                int curKey = *(int*)(body + SizeOfInt * hash) >> 16;
                while (curKey > 0)
                {
                    hash = (hash + 1) % Length;
                    curKey = *(int*)(body + SizeOfInt * hash) >> 16;
                }
                *(int*)(body + SizeOfInt * hash) = (tIndex << 16) | (sizeInBytes & 0xFFFF);
                *(short*)(body + TypeOffsets + SizeOfShort * hash) = index;
                *(short*)(body + TypeOffsets + TypeIndices + SizeOfShort * index) = sizeInBytes;

                sizeInBytes += (short)EcsTypeManager.Sizes[typeIndex];
            }
        }
        
        private const int MaxComponentCount = 20;
        private const ushort Length = 47;
        
        private const int SizeOfShort = sizeof(short);
        private const int SizeOfInt = sizeof(int);
        private const int TypeOffsets = Length * SizeOfInt;
        private const int TypeIndices = Length * SizeOfShort;
        
        private int GetHash(short typeIndex)
        {
            typeIndex++;
            int hash = typeIndex % Length;
            int t = *(int*)(body + SizeOfInt * hash) >> 16;
            while (t > 0)
            {
                if (t == typeIndex) return hash;
                hash = (hash + 1) % Length;
                t = *(int*)(body + SizeOfInt * hash) >> 16;
            }
            return -1;
        }

        public bool ContainType(short typeIndex)
        {
            return GetHash(typeIndex) > -1;
        }
        
        public short ReadIndex(short typeIndex)
        {
            return *(short*)(body + TypeOffsets + SizeOfShort * GetHash(typeIndex));
        }
        
        public int ReadOffsetByType(short typeIndex)
        {
            return *(int*)(body + SizeOfInt * GetHash(typeIndex)) & 0xFFFF;
        }

        public short ReadOffsetByIndex(short index)
        {
            return *(short*)(body + TypeOffsets + TypeIndices + SizeOfShort * index);
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
            return Unsafe.Read<T>((void*)(body + HeaderSize + m_rowByteSize * index + ReadOffsetByType((short)typeIndex)));
        }

        internal void WriteComponent<T>(int index, int typeIndex, T component) where T : struct, IEcsComponent
        {
            Unsafe.Write((void*)(body + HeaderSize + m_rowByteSize * index + ReadOffsetByType((short)typeIndex)), component);
        }

        internal void Dispose()
        {
            MemoryUtil.Free(body);
            MemoryUtil.Free((IntPtr)start);
            MemoryUtil.Free((IntPtr)count);
        }
    }
}