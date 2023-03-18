using System;
using System.Collections.Generic;
using Qwerty.ECS.Runtime.Archetypes;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public class EcsWorldSetting
    {
        public int entitiesCapacity = 0x200000;         // 2 097 152
        public int archetypeChunkMaxSizeInByte = 0x4000;   // 16 384
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
        //private readonly IEcsComponentPool[] m_componentPools;

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
            //m_componentPools = new IEcsComponentPool[EcsTypeManager.typeCount];

            // foreach ((int, IEcsComponentPoolCreator) item in EcsTypeManager.ComponentsCreators)
            // {
            //     (int typeIndex, IEcsComponentPoolCreator creator) = item;
            //     m_componentPools[typeIndex] = creator.Instantiate(1024);
            // }
        }
        
        public unsafe void Dispose()
        {
            int count = m_arcManager.count;
            for (int i = 0; i < count; i++)
            {
                m_arcManager[i].Dispose();
            }

            // foreach ((int, IEcsComponentPoolCreator) item in EcsTypeManager.ComponentsCreators)
            // {
            //     m_componentPools[item.Item1].Dispose();
            // }

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

        // public ref EcsComponentAccessor<T> GetComponentAccessor<T>() where T : struct, IEcsComponent
        // {
        //     return ref ((EcsComponentPool<T>)m_componentPools[EcsComponentType<T>.index]).GetAccessor();
        // }
        //
        // public EcsComponentAccessor<T>* GetComponentAccessorPtr<T>() where T : struct, IEcsComponent
        // {
        //     fixed (EcsComponentAccessor<T>* ptr = &GetComponentAccessor<T>())
        //     {
        //         return ptr;
        //     }
        // }
        
        private unsafe void PushEntity(EcsArchetype arch, EcsEntity entity, out EcsEntityInfo info)
        {
            EcsArchetype.Chunks* chunks = arch.chunks;
            EcsChunk* chunk = chunks->last;
            int chunkCapacity = arch.chunkCapacity;
            if (chunk == null || *chunk->count == chunkCapacity)
            {
                EcsChunk* lastChunk = (EcsChunk*)MemoryUtil.Alloc<EcsChunk>();
                lastChunk->Alloc(m_setting.archetypeChunkMaxSizeInByte, arch.rowCapacityInBytes);
                lastChunk->FillHeader(arch.indices);
                lastChunk->prior = chunks->last;
                *lastChunk->start = arch.chunksCount * chunkCapacity;
                *lastChunk->count = 0;
            
                chunks->last = lastChunk;
                arch.chunksCount++;
            }
            chunk = chunks->last;
            int indexInChunk = (*chunk->count)++;
            chunk->WriteEntity(indexInChunk, arch.entityOffset, entity);
            
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
                int rowCapacityInBytes = arch.rowCapacityInBytes;
                EcsEntity swapEntity = lastChunk->ReadEntity(lastIndex, arch.entityOffset);
                void* sourcePtr = (void*)(lastChunk->body + EcsChunk.HeaderSize + rowCapacityInBytes * lastIndex);
                void* targetPtr = (void*)(toChunk->body + EcsChunk.HeaderSize + rowCapacityInBytes * info.indexInChunk);
                Buffer.MemoryCopy(sourcePtr, targetPtr, rowCapacityInBytes, rowCapacityInBytes);
                m_entitiesInfo->Write(swapEntity.Index, new EcsEntityInfo(arch.index, info.indexInChunk, toChunk));
            }
            
            --*lastChunk->count;
            if (*lastChunk->count > 0)
            {
                return;
            }
            
            arch.chunksCount--;
            arch.chunks->last = lastChunk->prior;
            
            lastChunk->Dispose();
            MemoryUtil.Free((IntPtr)lastChunk);
        }

        private static unsafe void CopyRow(EcsArchetype fromArchetype, EcsEntityInfo fromInfo, EcsEntityInfo toInfo, int componentTypeIndex)
        {
            EcsChunk* fromChunk = fromInfo.chunk;
            EcsChunk* toChunk = toInfo.chunk;
            
            int fromRowCapacityInBytes = fromChunk->rowByteSize;
            int toRowCapacityInBytes = toChunk->rowByteSize;
            
            UnsafeArray componentsOffset = *fromArchetype.componentsOffset;
            int index = fromArchetype.indexMap->Get(componentTypeIndex);
            int sizeInBytes = componentsOffset.Read<int>(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)(fromChunk->body + EcsChunk.HeaderSize + fromRowCapacityInBytes * fromInfo.indexInChunk);
                void* target = (void*)(toChunk->body + EcsChunk.HeaderSize + toRowCapacityInBytes * toInfo.indexInChunk);
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
            
            if (++index < fromArchetype.indices.Length)
            {
                void* source = (void*)(fromChunk->body + EcsChunk.HeaderSize + fromRowCapacityInBytes * fromInfo.indexInChunk + componentsOffset.Read<int>(index));
                void* target = (void*)(toChunk->body + EcsChunk.HeaderSize + toRowCapacityInBytes * toInfo.indexInChunk + sizeInBytes);
                sizeInBytes = componentsOffset.Read<int>(componentsOffset.length - 1) - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
        
        private static unsafe void CopyRow(EcsEntityInfo fromInfo, EcsArchetype toArchetype, EcsEntityInfo toInfo, int componentTypeIndex)
        {
            EcsChunk* fromChunk = fromInfo.chunk;
            EcsChunk* toChunk = toInfo.chunk;

            int fromRowCapacityInBytes = fromChunk->rowByteSize;
            int toRowCapacityInBytes = toChunk->rowByteSize;
            
            UnsafeArray componentsOffset = *toArchetype.componentsOffset;
            int index = toArchetype.indexMap->Get(componentTypeIndex);
            int sizeInBytes = componentsOffset.Read<int>(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)(fromChunk->body + EcsChunk.HeaderSize + fromRowCapacityInBytes * fromInfo.indexInChunk);
                void* target = (void*)(toChunk->body + EcsChunk.HeaderSize + toRowCapacityInBytes * toInfo.indexInChunk);
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
            
            if (++index < toArchetype.indices.Length)
            {
                void* source = (void*)(fromChunk->body + EcsChunk.HeaderSize + fromRowCapacityInBytes * fromInfo.indexInChunk + sizeInBytes);
                void* target = (void*)(toChunk->body + EcsChunk.HeaderSize + toRowCapacityInBytes * toInfo.indexInChunk + componentsOffset.Read<int>(index));
                sizeInBytes = componentsOffset.Read<int>(componentsOffset.length - 1) - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
    }
}