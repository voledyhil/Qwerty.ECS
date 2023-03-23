using System;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Chunks
{
    public readonly struct EcsArchetypeGroupAccessor
    {
        private readonly IntPtr m_archetypes;
        private readonly int m_archetypesCount;
        private readonly int m_sizeOfIntPtr;

        internal EcsArchetypeGroupAccessor(IntPtr archetypes, int archetypesCount)
        {
            m_archetypes = archetypes;
            m_archetypesCount = archetypesCount;
            m_sizeOfIntPtr = MemoryUtil.SizeOf<IntPtr>();
        }

        public EcsChunkEnumerator GetEnumerator() => new EcsChunkEnumerator(m_archetypes, m_archetypesCount);

        public unsafe EcsChunkAccessor GetChunk(int index)
        {
            int archetypeIndex = 0;
            while (archetypeIndex < m_archetypesCount)
            {
                IntPtr intPtr = MemoryUtil.Read<IntPtr>(m_archetypes, archetypeIndex++ * m_sizeOfIntPtr);
                EcsChunk* chunk = ((EcsArchetype.Chunks*)intPtr)->last;
                
                if (chunk == null) continue;

                int count = chunk->index + 1;
                if (index >= count)
                {
                    index -= count;
                    continue;
                }
                
                while (--count > index) chunk = chunk->prior;
                return new EcsChunkAccessor(chunk);
            }
            throw new ArgumentOutOfRangeException(nameof(GetChunk));
        }
    }

    public readonly struct EcsChunkAccessor
    {
        public int count { get; }
        
        private readonly unsafe EcsChunk* m_chunk;
        private readonly int m_entitySizeOf;

        internal unsafe EcsChunkAccessor(EcsChunk* chunk)
        {
            m_chunk = chunk;
            m_entitySizeOf = MemoryUtil.SizeOf<EcsEntity>();
            count = *m_chunk->count;
        }

        public unsafe EcsChunkEntityAccessor GetEntityAccessor()
        {
            int rowCapacityInBytes = m_chunk->header->rowSizeInBytes;
            return new EcsChunkEntityAccessor(m_chunk->body, rowCapacityInBytes, rowCapacityInBytes - m_entitySizeOf);
        }
        
        public unsafe EcsChunkComponentAccessor<T> GetComponentAccessor<T>(EcsComponentTypeHandle<T> typeHandle) where T : struct, IEcsComponent
        {
            EcsChunkHeader* header = m_chunk->header;
            return new EcsChunkComponentAccessor<T>(m_chunk->body, header->rowSizeInBytes, header->ReadOffsetByType(typeHandle.typeIndex));
        }
        
    }
    
    public readonly ref struct EcsChunkComponentAccessor<T> where T : struct, IEcsComponent
    {
        private readonly IntPtr m_bodyPtr;
        private readonly int m_rowCapacityInBytes;
        private readonly int m_offset;
        
        public EcsChunkComponentAccessor(IntPtr bodyPtr, int rowCapacityInBytes, int offset)
        {
            m_bodyPtr = bodyPtr;
            m_rowCapacityInBytes = rowCapacityInBytes;
            m_offset = offset;
        }

        public T this[int index]
        {
            get => MemoryUtil.Read<T>(m_bodyPtr, m_rowCapacityInBytes * index + m_offset);
            set => MemoryUtil.Write(m_bodyPtr, m_rowCapacityInBytes * index + m_offset, value);
        }
    }
    
    public readonly ref struct EcsChunkEntityAccessor
    {
        private readonly IntPtr m_bodyPtr;
        private readonly int m_rowCapacityInBytes;
        private readonly int m_offset;

        public EcsChunkEntityAccessor(IntPtr bodyPtr, int rowCapacityInBytes, int offset)
        {
            m_bodyPtr = bodyPtr;
            m_rowCapacityInBytes = rowCapacityInBytes;
            m_offset = offset;
        }
        
        public EcsEntity this[int index] => MemoryUtil.Read<EcsEntity>(m_bodyPtr, m_rowCapacityInBytes * index + m_offset);
    }
}