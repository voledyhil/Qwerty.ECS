using System;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Chunks
{
    internal unsafe struct EcsChunkHeader
    {
        private IntPtr m_body;

        public int rowSizeInBytes;
        public int chunkCapacity;
        public short entityOffset;
        public int typesCount;
        
        private const int MaxComponentCount = 20;
        
        private const ushort Length = 47;
        private const int SizeOfShort = sizeof(short);
        private const int SizeOfInt = sizeof(int);
        private const int TypeToOffset = Length * SizeOfInt;
        private const int TypeToIndex = Length * SizeOfShort;
        private const int IndexToOffset = Length * SizeOfShort;
        private const int HeaderSize = TypeToOffset + TypeToIndex + IndexToOffset; // 376 bytes
        
        public void Alloc(int bodySizeInBytes, short[] indices)
        {
            m_body = MemoryUtil.Alloc(HeaderSize);
            typesCount = indices.Length;
            
            short sizeInBytes = 0;
            for (short index = 0; index < indices.Length; index++)
            {
                short typeIndex = indices[index];
                short tIndex = typeIndex;

                tIndex++;
                int hash = tIndex % Length;
                int curKey = *(int*)(m_body + SizeOfInt * hash) >> 16;
                while (curKey > 0)
                {
                    hash = (hash + 1) % Length;
                    curKey = *(int*)(m_body + SizeOfInt * hash) >> 16;
                }

                *(int*)(m_body + SizeOfInt * hash) = (tIndex << 16) | (sizeInBytes & 0xFFFF);
                *(short*)(m_body + TypeToOffset + SizeOfShort * hash) = index;
                *(short*)(m_body + TypeToOffset + TypeToIndex + SizeOfShort * index) = sizeInBytes;

                sizeInBytes += (short)EcsTypeManager.Sizes[typeIndex];
            }

            entityOffset = sizeInBytes;
            rowSizeInBytes = entityOffset + MemoryUtil.SizeOf<EcsEntity>();
            chunkCapacity = bodySizeInBytes / rowSizeInBytes;
        }

        private int GetHash(int typeIndex)
        {
            typeIndex++;
            int hash = typeIndex % Length;
            int t = *(int*)(m_body + SizeOfInt * hash) >> 16;
            while (t > 0)
            {
                if (t == typeIndex) return hash;
                hash = (hash + 1) % Length;
                t = *(int*)(m_body + SizeOfInt * hash) >> 16;
            }
            return -1;
        }

        public bool ContainType(short typeIndex)
        {
            return GetHash(typeIndex) > -1;
        }

        public short ReadIndex(short typeIndex)
        {
            return *(short*)(m_body + TypeToOffset + SizeOfShort * GetHash(typeIndex));
        }

        public int ReadOffsetByType(short typeIndex)
        {
            return *(int*)(m_body + SizeOfInt * GetHash(typeIndex)) & 0xFFFF;
        }

        public short ReadOffsetByIndex(int index)
        {
            return *(short*)(m_body + TypeToOffset + TypeToIndex + SizeOfShort * index);
        }

        internal void Dispose()
        {
            MemoryUtil.Free(m_body);
        }
    }
}