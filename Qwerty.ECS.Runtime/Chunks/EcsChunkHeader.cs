using System;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Chunks
{
    internal unsafe struct EcsChunkHeader
    {
        private IntPtr m_body;

        public int rowSizeInBytes;
        public int chunkCapacity;
        public int entityOffset;
        public int typesCount;
        
        private const int MaxComponentCount = 20;
        
        private const ushort Length = 37;
        private const int SizeOfShort = sizeof(short);
        private const int SizeOfLong = sizeof(long);
        private const int TypeToOffset = Length * SizeOfLong;
        private const int TypeToIndex = Length;
        private const int IndexToOffset = Length * SizeOfShort;
        private const int HeaderSize = TypeToOffset + TypeToIndex + IndexToOffset; // 407 bytes
        
        public void Alloc(int bodySizeInBytes, int[] indices)
        {
            m_body = MemoryUtil.Alloc(HeaderSize);
            typesCount = indices.Length;
            
            int sizeInBytes = 0;
            for (int index = 0; index < indices.Length; index++)
            {
                int typeIndex = indices[index];
                int tIndex = typeIndex;

                tIndex++;
                int hash = tIndex % Length;
                int curKey = (int)(*(long*)(m_body + SizeOfLong * hash) >> 32);
                while (curKey > 0)
                {
                    hash = (hash + 1) % Length;
                    curKey = (int)(*(long*)(m_body + SizeOfLong * hash) >> 32);
                }

                *(long*)(m_body + SizeOfLong * hash) = ((long)tIndex << 32) | (sizeInBytes & 0xFFFFFFFF);
                *(byte*)(m_body + TypeToOffset + hash) = (byte)index;
                *(short*)(m_body + TypeToOffset + TypeToIndex + SizeOfShort * index) = (short)sizeInBytes;

                sizeInBytes += EcsTypeManager.Sizes[typeIndex];
            }

            entityOffset = sizeInBytes;
            rowSizeInBytes = entityOffset + MemoryUtil.SizeOf<EcsEntity>();
            chunkCapacity = bodySizeInBytes / rowSizeInBytes;
        }

        private int GetHash(int typeIndex)
        {
            typeIndex++;
            int hash = typeIndex % Length;
            int t = (int)(*(long*)(m_body + SizeOfLong * hash) >> 32);
            while (t > 0)
            {
                if (t == typeIndex) return hash;
                hash = (hash + 1) % Length;
                t = (int)(*(long*)(m_body + SizeOfLong * hash) >> 32);
            }
            return -1;
        }

        public bool ContainType(int typeIndex)
        {
            return GetHash(typeIndex) > -1;
        }

        public int ReadIndex(int typeIndex)
        {
            return *(byte*)(m_body + TypeToOffset + GetHash(typeIndex));
        }

        public int ReadOffsetByType(int typeIndex)
        {
            return (int)(*(long*)(m_body + SizeOfLong * GetHash(typeIndex)) & 0xFFFFFFFF);
        }

        public int ReadOffsetByIndex(int index)
        {
            return *(short*)(m_body + TypeToOffset + TypeToIndex + SizeOfShort * index);
        }

        internal void Dispose()
        {
            MemoryUtil.Free(m_body);
        }
    }
}