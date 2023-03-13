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
        internal readonly HashSet<byte> typeIndicesSet;
        internal readonly EcsArchetype[] next;
        internal readonly EcsArchetype[] prior;

        private readonly int* m_chunksCount;
        internal readonly Chunks* chunks;
        public readonly int chunkCapacity;

        internal readonly int chunkSizeInBytes;
        internal readonly int rowCapacityInBytes;
        internal readonly UnsafeArray* componentsOffset;
        internal readonly EcsArchetypeComponentsMap* componentsMap;
        internal EcsArchetype(int archetypeIndex, byte[] typeIndices, int chunkSizeInBytes, int maxComponentsCount)
        {
            this.chunkSizeInBytes = chunkSizeInBytes;
            this.typeIndices = typeIndices;
            typeIndicesSet = new HashSet<byte>(typeIndices);

            componentsOffset = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
            componentsOffset->Realloc<int>(typeIndices.Length + 1);

            componentsMap = (EcsArchetypeComponentsMap*)MemoryUtilities.Alloc<EcsArchetypeComponentsMap>(1);
            componentsMap->Alloc(maxComponentsCount);
            
            int index = 0;
            for (; index < typeIndices.Length; index++)
            {
                int typeIndex = typeIndices[index];
                componentsOffset->Write(index, rowCapacityInBytes);
                componentsMap->Set(typeIndex, index);
                rowCapacityInBytes += EcsTypeManager.Sizes[typeIndex];
            }
            rowCapacityInBytes += Unsafe.SizeOf<EcsEntity>();
            
            chunkCapacity = chunkSizeInBytes / rowCapacityInBytes;
            this.archetypeIndex = archetypeIndex;
            
            m_chunksCount = (int*)MemoryUtilities.Alloc<int>(1);
            chunks = (Chunks*)MemoryUtilities.Alloc<Chunks>(1);

            CreateNextChunk(0);

            next = new EcsArchetype[EcsTypeManager.typeCount];
            prior = new EcsArchetype[EcsTypeManager.typeCount];
        }

        internal EcsArchetypeChunk* PushEntity(EcsEntity entity, out int indexInChunk)
        {
            if (*chunks->last->count == chunkCapacity)
            {
                CreateNextChunk(*m_chunksCount * chunkCapacity);
            }
            
            EcsArchetypeChunk* lastChunk = chunks->last;
            indexInChunk = *lastChunk->count;
            lastChunk->WriteEntity(indexInChunk, entity);
            ++*lastChunk->count;
            return lastChunk;
        }
        
        internal void Swap(int currentIndexInChunk, EcsArchetypeChunk* currentChunk, UnsafeArray* entityArchetypeInfo)
        {
            EcsArchetypeChunk* lastChunk = chunks->last;
            int lastIndex = *lastChunk->count - 1;
            if (currentChunk != chunks->last || currentIndexInChunk != lastIndex)
            {
                EcsEntity swapEntity = lastChunk->ReadEntity(lastIndex);
                void* sourcePtr = (void*)((IntPtr)lastChunk->body + rowCapacityInBytes * lastIndex);
                void* targetPtr = (void*)((IntPtr)currentChunk->body + rowCapacityInBytes * currentIndexInChunk);
                Buffer.MemoryCopy(sourcePtr, targetPtr, rowCapacityInBytes, rowCapacityInBytes);

                entityArchetypeInfo->Write(swapEntity.Index, new EcsArchetypeInfo(archetypeIndex, currentIndexInChunk, currentChunk));
            }
            
            --*chunks->last->count;
            if (*lastChunk->count != 0 || lastChunk->prior == null)
            {
                return;
            }
            --*m_chunksCount;
            chunks->last = lastChunk->prior;
            lastChunk->Dispose();
            MemoryUtilities.Free((IntPtr)lastChunk);
        }
        
        
        internal static void CopyRemove(int priorIndexInChunk, EcsArchetypeChunk* priorChunk, EcsArchetype priorArchetype, int nextIndexInChunk, EcsArchetypeChunk* nextChunk, int componentTypeIndex)
        {
            int priorRowCapacityInBytes = priorChunk->rowCapacityInBytes;
            int nextRowCapacityInBytes = nextChunk->rowCapacityInBytes;
            
            int index = priorArchetype.componentsMap->Get(componentTypeIndex);
            int sizeInBytes = priorArchetype.componentsOffset->Read<int>(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)((IntPtr)priorChunk->body + priorRowCapacityInBytes * priorIndexInChunk);
                void* target = (void*)((IntPtr)nextChunk->body + nextRowCapacityInBytes * nextIndexInChunk);
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
            
            if (++index < priorArchetype.typeIndices.Length)
            {
                int offset = priorArchetype.componentsOffset->Read<int>(index);
                void* source = (void*)((IntPtr)priorChunk->body + priorRowCapacityInBytes * priorIndexInChunk + offset);
                void* target = (void*)((IntPtr)nextChunk->body + nextRowCapacityInBytes * nextIndexInChunk + sizeInBytes);
                sizeInBytes = priorRowCapacityInBytes - Unsafe.SizeOf<EcsEntity>() - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
        
        internal static void CopyAdd(int priorIndexInChunk, EcsArchetypeChunk* priorChunk, int nextIndexInChunk, EcsArchetypeChunk* nextChunk, EcsArchetype nextArchetype, int componentTypeIndex)
        {
            int priorRowCapacityInBytes = priorChunk->rowCapacityInBytes;
            int nextRowCapacityInBytes = nextChunk->rowCapacityInBytes;
            
            int index = nextArchetype.componentsMap->Get(componentTypeIndex);
            int sizeInBytes = nextArchetype.componentsOffset->Read<int>(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)((IntPtr)priorChunk->body + priorRowCapacityInBytes * priorIndexInChunk);
                void* target = (void*)((IntPtr)nextChunk->body + nextRowCapacityInBytes * nextIndexInChunk);
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
            
            if (++index < nextArchetype.typeIndices.Length)
            {
                int offset = nextArchetype.componentsOffset->Read<int>(index);
                void* source = (void*)((IntPtr)priorChunk->body + priorRowCapacityInBytes * priorIndexInChunk + sizeInBytes);
                void* target = (void*)((IntPtr)nextChunk->body + nextRowCapacityInBytes * nextIndexInChunk + offset);
                sizeInBytes = priorRowCapacityInBytes - Unsafe.SizeOf<EcsEntity>() - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
        
        internal EcsArchetypeChunk* GetChunkByIndex(int chunkIndex)
        {
            EcsArchetypeChunk* chunk = chunks->last;
            int chunkCount = *m_chunksCount;
            while (--chunkCount > chunkIndex) chunk = chunk->prior;
            return chunk;
        }

        public EcsArchetypeChunkAccessor GetChunk(int chunkIndex)
        {
            EcsArchetypeChunk* chunk = GetChunkByIndex(chunkIndex);
            return new EcsArchetypeChunkAccessor(chunk->body, *chunk->count, rowCapacityInBytes, componentsMap, componentsOffset);
        }
        
        private void CreateNextChunk(int startIndex)
        {
            EcsArchetypeChunk* lastChunk = (EcsArchetypeChunk*)MemoryUtilities.Alloc<EcsArchetypeChunk>(1);
            lastChunk->Alloc(chunkSizeInBytes, rowCapacityInBytes, componentsMap, componentsOffset);
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