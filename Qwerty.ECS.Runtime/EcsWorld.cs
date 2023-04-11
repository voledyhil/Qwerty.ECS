using System;
using System.Collections.Generic;
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
        private readonly IntPtr m_entitiesInfo;
        private readonly IntPtr m_entities;
        private readonly IntPtr m_freeEntities;

        private int m_entityCounter;
        private int m_freeEntitiesLen;

        private readonly Dictionary<EcsFilter, EcsArchetypeGroup> m_archGroups = new Dictionary<EcsFilter, EcsArchetypeGroup>();

        private readonly short[] m_indicesBuffer;
        private readonly EcsArchetypeManager m_arcManager;
        private readonly int m_sizeOfEntity = MemoryUtil.SizeOf<EcsEntity>();
        private readonly int m_sizeOfEntityInfo = MemoryUtil.SizeOf<EcsEntityInfo>();
        private readonly int m_entitiesCapacity;

        public EcsWorld(EcsWorldSetting setting)
        {
            m_setting = setting;
            m_entitiesCapacity = setting.entitiesCapacity;
            
            m_freeEntities = MemoryUtil.Alloc((uint)(m_sizeOfEntity * m_entitiesCapacity));
            m_entities = MemoryUtil.Alloc((uint)(m_sizeOfEntity * m_entitiesCapacity));
            m_entitiesInfo = MemoryUtil.Alloc((uint)(m_sizeOfEntityInfo * m_entitiesCapacity));

            m_arcManager = new EcsArchetypeManager(setting);
            m_indicesBuffer = new short[EcsTypeManager.typeCount];
        }
        
        public void Dispose()
        {
            foreach (EcsArchetypeGroup archetypeGroup in m_archGroups.Values)
            {
                archetypeGroup.Dispose();
            }
            
            m_arcManager.Dispose();

            MemoryUtil.Free(m_entitiesInfo);
            MemoryUtil.Free(m_entities);
            MemoryUtil.Free(m_freeEntities);
        }
        
        private unsafe void PushEntity(EcsArchetype arch, EcsEntity entity, out EcsEntityInfo info)
        {
            EcsArchetype.Chunks* chunks = arch.chunks;
            EcsChunk* chunk = chunks->last;
            bool empty = chunk == null;
            if (empty || *chunk->count == chunks->header->chunkCapacity)
            {
                int index = empty ? 0 : chunk->index + 1;
                EcsChunk* lastChunk = (EcsChunk*)MemoryUtil.Alloc<EcsChunk>();
                lastChunk->Alloc(index, (uint)m_setting.archetypeChunkMaxSizeInByte, chunks->header);
                lastChunk->prior = chunks->last;
                
                chunks->last = lastChunk;
            }
            chunk = chunks->last;
            int indexInChunk = (*chunk->count)++;
            chunk->WriteEntity(indexInChunk, entity);
            
            info = new EcsEntityInfo(arch.index, indexInChunk, chunk);
            MemoryUtil.Write(m_entitiesInfo, entity.Index * m_sizeOfEntityInfo, info);
        }
        
        private unsafe void SwapRow(EcsArchetype arch, EcsEntityInfo info)
        {
            EcsArchetype.Chunks* chunks = arch.chunks;
            EcsChunk* toChunk = info.chunk;
            EcsChunk* lastChunk = chunks->last;
            int lastIndex = *lastChunk->count - 1;
            if (toChunk != lastChunk || info.indexInChunk != lastIndex)
            {
                int rowSizeInBytes = lastChunk->header->rowSizeInBytes;
                EcsEntity swapEntity = lastChunk->ReadEntity(lastIndex);
                void* sourcePtr = (void*)(lastChunk->body + rowSizeInBytes * lastIndex);
                void* targetPtr = (void*)(toChunk->body + rowSizeInBytes * info.indexInChunk);
                Buffer.MemoryCopy(sourcePtr, targetPtr, rowSizeInBytes, rowSizeInBytes);
                
                EcsEntityInfo swapEntityInfo = new EcsEntityInfo(arch.index, info.indexInChunk, toChunk);
                MemoryUtil.Write(m_entitiesInfo, swapEntity.Index * m_sizeOfEntityInfo, swapEntityInfo);
            }
            
            if (--*lastChunk->count > 0)
            {
                return;
            }
            
            chunks->last = lastChunk->prior;
            
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
        
        private static unsafe void CopyToNext(EcsEntityInfo fromInfo, EcsEntityInfo toInfo, short typeIndex)
        {
            EcsChunk* fromChunk = fromInfo.chunk;
            EcsChunkHeader* fromHeader = fromChunk->header;
            int fromRowSizeInBytes = fromHeader->rowSizeInBytes;
            
            EcsChunk* toChunk = toInfo.chunk;
            EcsChunkHeader* toHeader = toChunk->header;
            int toRowSizeInBytes = toHeader->rowSizeInBytes;
            
            short index = toHeader->ReadIndex(typeIndex);
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