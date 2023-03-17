// ReSharper disable once CheckNamespace

using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime
{
    public readonly unsafe ref struct EcsArchetypeGroupAccessor
    {
        private readonly UnsafeArray* m_archetypesRoot;
        internal EcsArchetypeGroupAccessor(UnsafeArray* archetypesRoot)
        {
            m_archetypesRoot = archetypesRoot;
        }
        public EcsChunkEnumerator GetEnumerator() => new EcsChunkEnumerator(m_archetypesRoot);
    }
    
    public unsafe readonly struct EcsChunkAccessor
    {
        public int count { get; }
        
        private readonly EcsChunk* m_chunk;
        private readonly int m_entitySizeOf;

        internal EcsChunkAccessor(EcsChunk* chunk)
        {
            m_chunk = chunk;
            m_entitySizeOf = Unsafe.SizeOf<EcsEntity>();
            count = *m_chunk->count;
        }

        public EcsChunkEntityAccessor GetEntityAccessor()
        {
            int rowCapacityInBytes = m_chunk->rowByteSize;
            return new EcsChunkEntityAccessor(m_chunk->body, rowCapacityInBytes, rowCapacityInBytes - m_entitySizeOf);
        }
        
        public EcsChunkComponentAccessor<T> GetComponentAccessor<T>() where T : struct, IEcsComponent
        {
            int offset = m_chunk->offsets->Get(EcsComponentType<T>.index);
            return new EcsChunkComponentAccessor<T>(m_chunk->body, m_chunk->rowByteSize, offset);
        }
    }
    
    public unsafe readonly ref struct EcsChunkComponentAccessor<T> where T : struct, IEcsComponent
    {
        private readonly byte* m_bodyPtr;
        private readonly int m_rowCapacityInBytes;
        private readonly int m_offset;
        
        public EcsChunkComponentAccessor(byte* bodyPtr, int rowCapacityInBytes, int offset)
        {
            m_bodyPtr = bodyPtr;
            m_rowCapacityInBytes = rowCapacityInBytes;
            m_offset = offset;
        }

        public T this[int index]
        {
            get => Unsafe.Read<T>((void*)((IntPtr)m_bodyPtr + m_rowCapacityInBytes * index + m_offset));
            set => Unsafe.Write((void*)((IntPtr)m_bodyPtr + m_rowCapacityInBytes * index + m_offset), value);
        }
    }
    
    public unsafe readonly ref struct EcsChunkEntityAccessor
    {
        private readonly byte* m_bodyPtr;
        private readonly int m_rowCapacityInBytes;
        private readonly int m_offset;

        public EcsChunkEntityAccessor(byte* bodyPtr, int rowCapacityInBytes, int offset)
        {
            m_bodyPtr = bodyPtr;
            m_rowCapacityInBytes = rowCapacityInBytes;
            m_offset = offset;
        }
        
        public EcsEntity this[int index] => Unsafe.Read<EcsEntity>((void*)((IntPtr)m_bodyPtr + m_rowCapacityInBytes * index + m_offset));
    }
}