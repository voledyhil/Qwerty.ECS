using System;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Chunks
{
    public readonly struct EcsArchetypeGroupAccessor
    {
        private readonly IntPtr m_archetypes;
        private readonly int m_archetypesCount;

        internal EcsArchetypeGroupAccessor(IntPtr archetypes, int archetypesCount)
        {
            m_archetypes = archetypes;
            m_archetypesCount = archetypesCount;
        }
        public EcsChunkEnumerator GetEnumerator() => new EcsChunkEnumerator(m_archetypes, m_archetypesCount);
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
        
        public unsafe EcsChunkComponentAccessor<T> GetComponentAccessor<T>() where T : struct, IEcsComponent
        {
            int offset = m_chunk->header->ReadOffsetByType(EcsComponentType<T>.index);
            return new EcsChunkComponentAccessor<T>(m_chunk->body, m_chunk->header->rowSizeInBytes, offset);
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