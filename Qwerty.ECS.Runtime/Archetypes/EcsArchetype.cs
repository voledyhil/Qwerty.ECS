// ReSharper disable once CheckNamespace
using System.Runtime.CompilerServices;

namespace Qwerty.ECS.Runtime.Archetypes
{
    public unsafe class EcsArchetype : IDisposable
    {
        internal struct Chunks
        {
            internal EcsArchetypeChunk* last;
        }
        
        public int chunksCount => chunksCnt;
        
        internal readonly int archetypeIndex;
        internal readonly byte[] typeIndices;
        internal readonly EcsArchetype[] next;
        internal readonly EcsArchetype[] prior;

        internal int chunksCnt;
        internal readonly int rowCapacityInBytes;
        
        internal readonly Chunks* chunks;
        internal readonly UnsafeArray* componentsOffset;
        internal readonly IntMap* componentsMap;
        
        private readonly int m_chunkCapacity;
        private readonly int m_chunkSizeInBytes;
        
        internal EcsArchetype(int index, byte[] indices, int chunkSizeInBytes, PrimeStorage* primeStorage)
        {
            m_chunkSizeInBytes = chunkSizeInBytes;
            typeIndices = indices;

            componentsOffset = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
            componentsOffset->Realloc<int>(indices.Length + 1);

            componentsMap = (IntMap*)MemoryUtilities.Alloc<IntMap>(1);
            componentsMap->Alloc(primeStorage->GetPrime(indices.Length));
            
            int i = 0;
            for (; i < indices.Length; i++)
            {
                int typeIndex = indices[i];
                componentsOffset->Write(i, rowCapacityInBytes);
                componentsMap->Set(typeIndex, i);
                rowCapacityInBytes += EcsTypeManager.Sizes[typeIndex];
            }
            rowCapacityInBytes += Unsafe.SizeOf<EcsEntity>();
            
            m_chunkCapacity = chunkSizeInBytes / rowCapacityInBytes;
            archetypeIndex = index;
            chunks = (Chunks*)MemoryUtilities.Alloc<Chunks>(1);

            CreateNextChunk(0);

            next = new EcsArchetype[EcsTypeManager.typeCount];
            prior = new EcsArchetype[EcsTypeManager.typeCount];
        }

        internal EcsArchetypeChunkInfo PushEntity(EcsEntity entity)
        {
            if (*chunks->last->count == m_chunkCapacity)
            {
                CreateNextChunk(chunksCnt * m_chunkCapacity);
            }
            EcsArchetypeChunk* chunk = chunks->last;
            int indexInChunk = (*chunk->count)++;
            chunk->WriteEntity(indexInChunk, entity);
            return new EcsArchetypeChunkInfo(archetypeIndex, indexInChunk, chunk);
        }

        public EcsArchetypeChunkAccessor GetChunk(int chunkIndex)
        {
            EcsArchetypeChunk* chunk = chunks->last;
            int cnt = chunksCnt;
            while (--cnt > chunkIndex) chunk = chunk->prior;
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
            chunksCnt++;
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
            MemoryUtilities.Free((IntPtr)componentsOffset);
            MemoryUtilities.Free((IntPtr)componentsMap);
        }
    }
}