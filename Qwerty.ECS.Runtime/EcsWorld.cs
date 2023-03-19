using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Chunks;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public class EcsWorldSetting
    {
        public int entitiesCapacity = 0x200000;             // 2 097 152
        public short archetypeChunkMaxSizeInByte = 0x4000;  // 16 384
    }
    
    public partial class EcsWorld : IDisposable
    {
        private readonly EcsWorldSetting m_setting;
        private readonly unsafe UnsafeArray* m_entitiesInfo;
        private readonly unsafe UnsafeArray* m_entities;
        private readonly unsafe UnsafeArray* m_freeEntities;

        private int m_entityCounter;
        private int m_freeEntitiesLen;

        private readonly Dictionary<EcsFilter, EcsArchetypeGroup> m_archGroups = new Dictionary<EcsFilter, EcsArchetypeGroup>();

        private readonly byte[] m_indicesBuffer;
        private readonly EcsArchetypeManager m_arcManager;
        public unsafe EcsWorld(EcsWorldSetting setting)
        {
            m_setting = setting;
            m_freeEntities = (UnsafeArray*)MemoryUtil.Alloc<UnsafeArray>();
            m_freeEntities->Alloc<EcsEntity>(setting.entitiesCapacity);
            
            m_entities = (UnsafeArray*)MemoryUtil.Alloc<UnsafeArray>();
            m_entities->Alloc<EcsEntity>(setting.entitiesCapacity);
            
            m_entitiesInfo = (UnsafeArray*)MemoryUtil.Alloc<UnsafeArray>();
            m_entitiesInfo->Alloc<EcsEntityInfo>(setting.entitiesCapacity);

            m_arcManager = new EcsArchetypeManager(setting);
            m_indicesBuffer = new byte[EcsTypeManager.typeCount];
        }
        
        public unsafe void Dispose()
        {
            foreach (EcsArchetypeGroup archetypeGroup in m_archGroups.Values)
            {
                archetypeGroup.Dispose();
            }
            
            m_arcManager.Dispose();
            m_entitiesInfo->Dispose();
            m_entities->Dispose();
            m_freeEntities->Dispose();
            
            MemoryUtil.Free((IntPtr)m_entitiesInfo);
            MemoryUtil.Free((IntPtr)m_entities);
            MemoryUtil.Free((IntPtr)m_freeEntities);
        }
        
        private unsafe void PushEntity(EcsArchetype arch, EcsEntity entity, out EcsEntityInfo info)
        {
            EcsArchetype.Chunks* chunks = arch.chunks;
            EcsChunk* chunk = chunks->last;
            if (chunk == null || *chunk->count == chunks->header->chunkCapacity)
            {
                EcsChunk* lastChunk = (EcsChunk*)MemoryUtil.Alloc<EcsChunk>();
                lastChunk->Alloc(m_setting.archetypeChunkMaxSizeInByte, chunks->header);
                lastChunk->prior = chunks->last;
                
                
                chunks->last = lastChunk;
            }
            chunk = chunks->last;
            int indexInChunk = (*chunk->count)++;
            chunk->WriteEntity(indexInChunk, entity);
            
            info = new EcsEntityInfo(arch.index, indexInChunk, chunk);
            m_entitiesInfo->Write(entity.Index, info);
        }
        
        private unsafe void SwapRow(EcsArchetype arch, EcsEntityInfo info)
        {
            EcsChunk* toChunk = info.chunk;
            EcsChunk* lastChunk = arch.chunks->last;
            int lastIndex = *lastChunk->count - 1;
            if (toChunk != lastChunk || info.indexInChunk != lastIndex)
            {
                int rowSizeInBytes = lastChunk->header->rowSizeInBytes;
                EcsEntity swapEntity = lastChunk->ReadEntity(lastIndex);
                void* sourcePtr = (void*)(lastChunk->body + rowSizeInBytes * lastIndex);
                void* targetPtr = (void*)(toChunk->body + rowSizeInBytes * info.indexInChunk);
                Buffer.MemoryCopy(sourcePtr, targetPtr, rowSizeInBytes, rowSizeInBytes);
                m_entitiesInfo->Write(swapEntity.Index, new EcsEntityInfo(arch.index, info.indexInChunk, toChunk));
            }
            
            --*lastChunk->count;
            if (*lastChunk->count > 0)
            {
                return;
            }
            
            arch.chunks->last = lastChunk->prior;
            
            lastChunk->Dispose();
            MemoryUtil.Free((IntPtr)lastChunk);
        }

        private static unsafe void CopyToPrior(EcsEntityInfo fromInfo, EcsEntityInfo toInfo, short componentTypeIndex)
        {
            EcsChunk* fromChunk = fromInfo.chunk;
            EcsChunkHeader* fromHeader = fromChunk->header;
            int fromRowSizeInBytes = fromHeader->rowSizeInBytes;
            
            EcsChunk* toChunk = toInfo.chunk;
            EcsChunkHeader* toHeader = toChunk->header;
            int toRowSizeInBytes = toHeader->rowSizeInBytes;
            
            short index = fromHeader->ReadIndex(componentTypeIndex);
            int sizeInBytes = fromHeader->ReadOffsetByIndex(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)(fromChunk->body + fromRowSizeInBytes * fromInfo.indexInChunk);
                void* target = (void*)(toChunk->body + toRowSizeInBytes * toInfo.indexInChunk);
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
            
            if (++index < fromHeader->typesCount)
            {
                void* source = (void*)(fromChunk->body + fromRowSizeInBytes * fromInfo.indexInChunk + fromHeader->ReadOffsetByIndex(index));
                void* target = (void*)(toChunk->body + toRowSizeInBytes * toInfo.indexInChunk + sizeInBytes);
                sizeInBytes = fromHeader->ReadOffsetByIndex(fromHeader->typesCount - 1) - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
        
        private static unsafe void CopyToNext(EcsEntityInfo fromInfo, EcsEntityInfo toInfo, short componentTypeIndex)
        {
            EcsChunk* fromChunk = fromInfo.chunk;
            EcsChunkHeader* fromHeader = fromChunk->header;
            int fromRowSizeInBytes = fromHeader->rowSizeInBytes;
            
            EcsChunk* toChunk = toInfo.chunk;
            EcsChunkHeader* toHeader = toChunk->header;
            int toRowSizeInBytes = toHeader->rowSizeInBytes;
            
            short index = toHeader->ReadIndex(componentTypeIndex);
            int sizeInBytes = toHeader->ReadOffsetByIndex(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)(fromChunk->body + fromRowSizeInBytes * fromInfo.indexInChunk);
                void* target = (void*)(toChunk->body + toRowSizeInBytes * toInfo.indexInChunk);
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }

            if (++index < toHeader->typesCount)
            {
                void* source = (void*)(fromChunk->body + fromRowSizeInBytes * fromInfo.indexInChunk + sizeInBytes);
                void* target = (void*)(toChunk->body + toRowSizeInBytes * toInfo.indexInChunk + toHeader->ReadOffsetByIndex(index));
                sizeInBytes = toHeader->ReadOffsetByIndex(toHeader->typesCount - 1) - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
    }
}