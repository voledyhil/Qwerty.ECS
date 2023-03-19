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
        
        private const ushort Capacity = 47;
        private const int SizeOfShort = sizeof(short);
        private const int SizeOfInt = sizeof(int);
        private const int TypeToOffset = Capacity * SizeOfInt;
        
        private const int TypeToIndex = Capacity * SizeOfShort;
        private const int IndexToOffset = Capacity * SizeOfShort;
        private const int HeaderSize = TypeToOffset + TypeToIndex + IndexToOffset;
        
        public void Alloc(int bodySizeInBytes, byte[] indices)
        {
            m_body = MemoryUtil.Alloc(HeaderSize);
            typesCount = indices.Length;
            
            short sizeInBytes = 0;
            for (short index = 0; index < indices.Length; index++)
            {
                short typeIndex = indices[index];
                short tIndex = typeIndex;

                tIndex++;
                int hash = tIndex % Capacity;
                int curKey = *(int*)(m_body + SizeOfInt * hash) >> 16;
                while (curKey > 0)
                {
                    hash = (hash + 1) % Capacity;
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

        private int GetHash(short typeIndex)
        {
            typeIndex++;
            int hash = typeIndex % Capacity;
            int t = *(int*)(m_body + SizeOfInt * hash) >> 16;
            while (t > 0)
            {
                if (t == typeIndex) return hash;
                hash = (hash + 1) % Capacity;
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