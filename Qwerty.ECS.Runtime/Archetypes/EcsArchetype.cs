// ReSharper disable once CheckNamespace
using System.Runtime.CompilerServices;

namespace Qwerty.ECS.Runtime.Archetypes
{
    public unsafe class EcsArchetype : IDisposable
    {
        public int chunksCount => *m_chunksCount;
        internal struct Chunks
        {
            internal EcsArchetypeChunk* last;
        }
        
        internal readonly int archetypeIndex;
        internal readonly byte[] typeIndices;
        internal readonly EcsArchetype[] next;
        internal readonly EcsArchetype[] prior;

        internal readonly int* m_chunksCount;
        internal readonly Chunks* chunks;
        private readonly int m_chunkCapacity;

        private readonly int m_chunkSizeInBytes;
        internal readonly int rowCapacityInBytes;
        
        internal readonly UnsafeArray* componentsOffset;
        internal readonly IntMap* componentsMap;
        internal EcsArchetype(int archetypeIndex, byte[] typeIndices, int chunkSizeInBytes, PrimeStorage* primeStorage)
        {
            m_chunkSizeInBytes = chunkSizeInBytes;
            this.typeIndices = typeIndices;

            componentsOffset = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
            componentsOffset->Realloc<int>(typeIndices.Length + 1);

            componentsMap = (IntMap*)MemoryUtilities.Alloc<IntMap>(1);
            componentsMap->Alloc(primeStorage->GetPrime(typeIndices.Length));
            
            int index = 0;
            for (; index < typeIndices.Length; index++)
            {
                int typeIndex = typeIndices[index];
                componentsOffset->Write(index, rowCapacityInBytes);
                componentsMap->Set(typeIndex, index);
                rowCapacityInBytes += EcsTypeManager.Sizes[typeIndex];
            }
            rowCapacityInBytes += Unsafe.SizeOf<EcsEntity>();
            
            m_chunkCapacity = chunkSizeInBytes / rowCapacityInBytes;
            this.archetypeIndex = archetypeIndex;
            
            m_chunksCount = (int*)MemoryUtilities.Alloc<int>(1);
            chunks = (Chunks*)MemoryUtilities.Alloc<Chunks>(1);

            CreateNextChunk(0);

            next = new EcsArchetype[EcsTypeManager.typeCount];
            prior = new EcsArchetype[EcsTypeManager.typeCount];
        }

        internal EcsArchetypeChunkInfo PushEntity(EcsEntity entity)
        {
            if (*chunks->last->count == m_chunkCapacity)
            {
                CreateNextChunk(*m_chunksCount * m_chunkCapacity);
            }
            EcsArchetypeChunk* chunk = chunks->last;
            int indexInChunk = (*chunk->count)++;
            chunk->WriteEntity(indexInChunk, entity);
            return new EcsArchetypeChunkInfo(archetypeIndex, indexInChunk, chunk);
        }

        public EcsArchetypeChunkAccessor GetChunk(int chunkIndex)
        {
            EcsArchetypeChunk* chunk = chunks->last;
            int chunkCount = *m_chunksCount;
            while (--chunkCount > chunkIndex) chunk = chunk->prior;
            return new EcsArchetypeChunkAccessor(chunk->body, *chunk->count, rowCapacityInBytes, componentsMap, componentsOffset);
        }
        
        private void CreateNextChunk(int startIndex)
        {
            EcsArchetypeChunk* lastChunk = (EcsArchetypeChunk*)MemoryUtilities.Alloc<EcsArchetypeChunk>(1);
            lastChunk->Alloc(m_chunkSizeInBytes, rowCapacityInBytes, componentsMap, componentsOffset);
            lastChunk->prior = chunks->last;
            *lastChunk->start = startIndex;
            *lastChunk->count = 0;
            
            chunks->last = lastChunk;
            ++*m_chunksCount;
        }

        public void Dispose()
        {
            EcsArchetypeChunk* chunk = chunks->last;
            while (chunk != null)
            {
                EcsArchetypeChunk* toDispose = chunk;
                chunk = chunk->prior;
                toDispose->Dispose();
            }
            
            componentsOffset->Dispose();
            componentsMap->Dispose();
            
            MemoryUtilities.Free((IntPtr)chunks);
            MemoryUtilities.Free((IntPtr)m_chunksCount);
            MemoryUtilities.Free((IntPtr)componentsOffset);
            MemoryUtilities.Free((IntPtr)componentsMap);
        }
    }
}