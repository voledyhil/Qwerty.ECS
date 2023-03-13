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
            if (*chunks->last->count == chunkCapacity)
            {
                CreateNextChunk(*m_chunksCount * chunkCapacity);
            }
            
            EcsArchetypeChunk* lastChunk = chunks->last;
            int lastIndex = *lastChunk->start + *lastChunk->count;
            lastChunk->WriteEntity(lastIndex % chunkCapacity, entity);
            ++*lastChunk->count;
            return lastIndex;
        }
        
        internal bool TrySwapEntity(int currentIndex, out int swapEntityIndex)
        {
            swapEntityIndex = -1;
            EcsArchetypeChunk* lastChunk = chunks->last;
            int lastIndex = *lastChunk->start + *lastChunk->count - 1;
            if (currentIndex == lastIndex)
            {
                return false;
            }
            
            EcsEntity swapEntity = lastChunk->ReadEntity(lastIndex % chunkCapacity);
            swapEntityIndex = swapEntity.Index;
            
            EcsArchetypeChunk* targetChunk = GetChunkByIndex(currentIndex / chunkCapacity);
            EcsArchetypeChunk* sourceChunk = GetChunkByIndex(lastIndex / chunkCapacity);
            void* sourcePtr = (void*)((IntPtr)sourceChunk->body + rowCapacityInBytes * (lastIndex % chunkCapacity));
            void* targetPtr = (void*)((IntPtr)targetChunk->body + rowCapacityInBytes * (currentIndex % chunkCapacity));
            Buffer.MemoryCopy(sourcePtr, targetPtr, rowCapacityInBytes, rowCapacityInBytes);
            
            return true;
        }


        public void PopLastEntity()
        {
            --*chunks->last->count;
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
            int sourceChunkCapacity = sourceArchetype.chunkCapacity;
            int sourceRowCapacityInBytes = sourceArchetype.rowCapacityInBytes;
            
            int targetChunkCapacity = targetArchetype.chunkCapacity;
            int targetRowCapacityInBytes = targetArchetype.rowCapacityInBytes;
            
            EcsArchetypeChunk* sourceChunk = sourceArchetype.GetChunkByIndex(sourceIndex / sourceChunkCapacity);
            EcsArchetypeChunk* targetChunk = targetArchetype.GetChunkByIndex(targetIndex / targetChunkCapacity);
            int sizeInBytes = sourceArchetype.componentsOffset->Read<int>(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)((IntPtr)sourceChunk->body + sourceRowCapacityInBytes * (sourceIndex % sourceChunkCapacity));
                void* target = (void*)((IntPtr)targetChunk->body + targetRowCapacityInBytes * (targetIndex % targetChunkCapacity));
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
            
            if (++index < sourceArchetype.typeIndices.Length)
            {
                int offset = sourceArchetype.componentsOffset->Read<int>(index);
                void* source = (void*)((IntPtr)sourceChunk->body + sourceRowCapacityInBytes * (sourceIndex % sourceChunkCapacity) + offset);
                void* target = (void*)((IntPtr)targetChunk->body + targetRowCapacityInBytes * (targetIndex % targetChunkCapacity) + sizeInBytes);
                sizeInBytes = sourceRowCapacityInBytes - Unsafe.SizeOf<EcsEntity>() - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
        
        public static void CopyAdd(int sourceIndex, EcsArchetype sourceArchetype, int targetIndex, EcsArchetype targetArchetype, int index)
        {
            int sourceChunkCapacity = sourceArchetype.chunkCapacity;
            int sourceRowCapacityInBytes = sourceArchetype.rowCapacityInBytes;
            
            int targetChunkCapacity = targetArchetype.chunkCapacity;
            int targetRowCapacityInBytes = targetArchetype.rowCapacityInBytes;
            
            EcsArchetypeChunk* sourceChunk = sourceArchetype.GetChunkByIndex(sourceIndex / sourceChunkCapacity);
            EcsArchetypeChunk* targetChunk = targetArchetype.GetChunkByIndex(targetIndex / targetChunkCapacity);
            int sizeInBytes = targetArchetype.componentsOffset->Read<int>(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)((IntPtr)sourceChunk->body + sourceRowCapacityInBytes * (sourceIndex % sourceChunkCapacity));
                void* target = (void*)((IntPtr)targetChunk->body + targetRowCapacityInBytes * (targetIndex % targetChunkCapacity));
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
            
            if (++index < targetArchetype.typeIndices.Length)
            {
                int offset = targetArchetype.componentsOffset->Read<int>(index);
                void* source = (void*)((IntPtr)sourceChunk->body + sourceRowCapacityInBytes * (sourceIndex % sourceChunkCapacity) + sizeInBytes);
                void* target = (void*)((IntPtr)targetChunk->body + targetRowCapacityInBytes * (targetIndex % targetChunkCapacity) + offset);
                sizeInBytes = sourceRowCapacityInBytes - Unsafe.SizeOf<EcsEntity>() - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
    }
}