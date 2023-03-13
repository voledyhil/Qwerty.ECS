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

        internal int PushEntity(EcsEntity entity)
        {
            EcsArchetypeChunk* lastChunk = chunks->last;
            int lastIndex = *lastChunk->start + *lastChunk->count;
            lastChunk->WriteEntity(lastIndex % chunkCapacity, entity);
            ++*lastChunk->count;
            return lastIndex;
        }
        
        internal bool TrySwapEntity(int indexInArchetype, out int swapEntityIndex, out int lastIndex)
        {
            swapEntityIndex = -1;
            EcsArchetypeChunk* lastChunk = chunks->last;
            lastIndex = *lastChunk->start + *lastChunk->count - 1;
            if (indexInArchetype == lastIndex)
            {
                return false;
            }
            EcsEntity swapEntity = lastChunk->ReadEntity(lastIndex % chunkCapacity);
            swapEntityIndex = swapEntity.Index;
            EcsArchetypeChunk* chunk = GetChunkByIndex(indexInArchetype / chunkCapacity);
            chunk->WriteEntity(indexInArchetype % chunkCapacity, swapEntity);
            return true;
        }

        public void PopLastEntity()
        {
            --*chunks->last->count;
        }

        public void CreateNextChunkIfNeed()
        {
            if (*chunks->last->count != chunkCapacity)
            {
                return;
            }
            CreateNextChunk(*m_chunksCount * chunkCapacity);
        }
        
        public void DestroyLastChunkIfNeed()
        {
            EcsArchetypeChunk* lastChunk = chunks->last;
            if (*lastChunk->count != 0 || lastChunk->prior == null)
            {
                return;
            }
            --*m_chunksCount;
            chunks->last = lastChunk->prior;
            lastChunk->Dispose();
            MemoryUtilities.Free((IntPtr)lastChunk);
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

        public static void CopyRemove(int sourceIndex, EcsArchetype sourceArchetype, int targetIndex, EcsArchetype targetArchetype, int index)
        {
            EcsArchetypeChunk* sourceChunk = sourceArchetype.GetChunkByIndex(sourceIndex / sourceArchetype.chunkCapacity);
            int sourceIndexInChunk = sourceIndex % sourceArchetype.chunkCapacity;
            
            EcsArchetypeChunk* targetChunk = targetArchetype.GetChunkByIndex(targetIndex / targetArchetype.chunkCapacity);
            int targetIndexInChunk = targetIndex % targetArchetype.chunkCapacity;
            
            int sizeInBytes = sourceArchetype.componentsOffset->Read<int>(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)((IntPtr)sourceChunk->body + sourceArchetype.rowCapacityInBytes * sourceIndexInChunk);
                void* target = (void*)((IntPtr)targetChunk->body + targetArchetype.rowCapacityInBytes * targetIndexInChunk);
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
            
            if (++index < sourceArchetype.typeIndices.Length)
            {
                int offset = sourceArchetype.componentsOffset->Read<int>(index);
                void* source = (void*)((IntPtr)sourceChunk->body + sourceArchetype.rowCapacityInBytes * sourceIndexInChunk + offset);
                void* target = (void*)((IntPtr)targetChunk->body + targetArchetype.rowCapacityInBytes * targetIndexInChunk + sizeInBytes);
                sizeInBytes = sourceArchetype.rowCapacityInBytes - Unsafe.SizeOf<EcsEntity>() - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
        
        public static void CopyAdd(int sourceIndex, EcsArchetype sourceArchetype, int targetIndex, EcsArchetype targetArchetype, int index)
        {
            EcsArchetypeChunk* sourceChunk = sourceArchetype.GetChunkByIndex(sourceIndex / sourceArchetype.chunkCapacity);
            int sourceIndexInChunk = sourceIndex % sourceArchetype.chunkCapacity;
            
            EcsArchetypeChunk* targetChunk = targetArchetype.GetChunkByIndex(targetIndex / targetArchetype.chunkCapacity);
            int targetIndexInChunk = targetIndex % targetArchetype.chunkCapacity;
            
            int sizeInBytes = targetArchetype.componentsOffset->Read<int>(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)((IntPtr)sourceChunk->body + sourceArchetype.rowCapacityInBytes * sourceIndexInChunk);
                void* target = (void*)((IntPtr)targetChunk->body + targetArchetype.rowCapacityInBytes * targetIndexInChunk);
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
            
            if (++index < targetArchetype.typeIndices.Length)
            {
                int offset = targetArchetype.componentsOffset->Read<int>(index);
                void* source = (void*)((IntPtr)sourceChunk->body + sourceArchetype.rowCapacityInBytes * sourceIndexInChunk + sizeInBytes);
                void* target = (void*)((IntPtr)targetChunk->body + targetArchetype.rowCapacityInBytes * targetIndexInChunk + offset);
                sizeInBytes = sourceArchetype.rowCapacityInBytes - Unsafe.SizeOf<EcsEntity>() - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
        
        public static void CopySwap(int sourceIndex, int targetIndex, EcsArchetype archetype)
        {
            int chunkCapacity = archetype.chunkCapacity;
            int rowCapacityInBytes = archetype.rowCapacityInBytes;
            
            EcsArchetypeChunk* sourceChunk = archetype.GetChunkByIndex(sourceIndex / chunkCapacity);
            EcsArchetypeChunk* targetChunk = archetype.GetChunkByIndex(targetIndex / chunkCapacity);
            void* sourcePtr = (void*)((IntPtr)sourceChunk->body + rowCapacityInBytes * (sourceIndex % chunkCapacity));
            void* targetPtr = (void*)((IntPtr)targetChunk->body + rowCapacityInBytes * (targetIndex % chunkCapacity));
            Buffer.MemoryCopy(sourcePtr, targetPtr, rowCapacityInBytes, rowCapacityInBytes);
        }
    }
}