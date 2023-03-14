using Qwerty.ECS.Runtime.Archetypes;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public partial class EcsWorld : IDisposable
    {
        private readonly unsafe UnsafeArray* m_entitiesInfo;
        private readonly unsafe UnsafeArray* m_entities;
        private readonly unsafe UnsafeArray* m_freeEntities;

        private int m_entityCounter;
        private int m_freeEntitiesLen;

        private readonly Dictionary<EcsFilter, EcsArchetypeGroup> m_archetypeGroups = new Dictionary<EcsFilter, EcsArchetypeGroup>();

        private readonly byte[] m_indicesBuffer;
        private readonly EcsArchetypeManager m_archetypeManager;
        private readonly EcsWorldSetting m_setting;
        //private readonly IEcsComponentPool[] m_componentPools;

        public unsafe EcsWorld(EcsWorldSetting setting)
        {
            m_setting = setting;
            m_freeEntities = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
            m_freeEntities->Realloc<EcsEntity>(setting.entitiesCapacity);
            
            m_entities = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
            m_entities->Realloc<EcsEntity>(setting.entitiesCapacity);
            
            m_entitiesInfo = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
            m_entitiesInfo->Realloc<EcsEntityInfo>(setting.entitiesCapacity);

            m_archetypeManager = new EcsArchetypeManager(setting);

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
            int count = m_archetypeManager.archetypeCount;
            for (int i = 0; i < count; i++)
            {
                m_archetypeManager[i].Dispose();
            }

            // foreach ((int, IEcsComponentPoolCreator) item in EcsTypeManager.ComponentsCreators)
            // {
            //     m_componentPools[item.Item1].Dispose();
            // }

            foreach (EcsArchetypeGroup archetypeGroup in m_archetypeGroups.Values)
            {
                archetypeGroup.Dispose();
            }
            
            m_archetypeManager.Dispose();
            
            m_entitiesInfo->Dispose();
            m_entities->Dispose();
            m_freeEntities->Dispose();
            
            MemoryUtilities.Free((IntPtr)m_entitiesInfo);
            MemoryUtilities.Free((IntPtr)m_entities);
            MemoryUtilities.Free((IntPtr)m_freeEntities);
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
        
        private unsafe void PushEntity(EcsArchetype archetype, EcsEntity entity, out EcsEntityInfo info)
        {
            EcsArchetype.Chunks* chunks = archetype.chunks;
            EcsChunk* chunk = chunks->last;
            int chunkCapacity = archetype.chunkCapacity;
            if (chunk == null || *chunk->count == chunkCapacity)
            {
                EcsChunk* lastChunk = (EcsChunk*)MemoryUtilities.Alloc<EcsChunk>(1);
                lastChunk->Alloc(m_setting.archetypeChunkSizeInByte, archetype.rowCapacityInBytes, archetype.componentsMap, archetype.componentsOffset);
                lastChunk->prior = chunks->last;
                *lastChunk->start = archetype.chunksCount * chunkCapacity;
                *lastChunk->count = 0;
            
                chunks->last = lastChunk;
                archetype.chunksCount++;
            }
            chunk = chunks->last;
            int indexInChunk = (*chunk->count)++;
            chunk->WriteEntity(indexInChunk, entity);
            
            info = new EcsEntityInfo(archetype.index, indexInChunk, chunk);
            m_entitiesInfo->Write(entity.Index, info);
        }
        
        private unsafe void SwapRow(EcsArchetype archetype, EcsEntityInfo info)
        {
            EcsChunk* toChunk = info.chunk;
            EcsChunk* lastChunk = archetype.chunks->last;
            int lastIndex = *lastChunk->count - 1;
            if (toChunk != lastChunk || info.chunkIndex != lastIndex)
            {
                int rowCapacityInBytes = archetype.rowCapacityInBytes;
                EcsEntity swapEntity = lastChunk->ReadEntity(lastIndex);
                void* sourcePtr = (void*)((IntPtr)lastChunk->body + rowCapacityInBytes * lastIndex);
                void* targetPtr = (void*)((IntPtr)toChunk->body + rowCapacityInBytes * info.chunkIndex);
                Buffer.MemoryCopy(sourcePtr, targetPtr, rowCapacityInBytes, rowCapacityInBytes);
                m_entitiesInfo->Write(swapEntity.Index, new EcsEntityInfo(archetype.index, info.chunkIndex, toChunk));
            }
            
            --*lastChunk->count;
            if (*lastChunk->count > 0)
            {
                return;
            }
            
            archetype.chunksCount--;
            archetype.chunks->last = lastChunk->prior;
            
            lastChunk->Dispose();
            MemoryUtilities.Free((IntPtr)lastChunk);
        }

        private static unsafe void CopyRow(EcsArchetype fromArchetype, EcsEntityInfo fromInfo, EcsEntityInfo toInfo, int componentTypeIndex)
        {
            EcsChunk* fromChunk = fromInfo.chunk;
            EcsChunk* toChunk = toInfo.chunk;
            
            int fromRowCapacityInBytes = fromChunk->rowCapacityInBytes;
            int toRowCapacityInBytes = toChunk->rowCapacityInBytes;
            
            UnsafeArray componentsOffset = *fromArchetype.componentsOffset;
            int index = fromArchetype.componentsMap->Get(componentTypeIndex);
            int sizeInBytes = componentsOffset.Read<int>(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)((IntPtr)fromChunk->body + fromRowCapacityInBytes * fromInfo.chunkIndex);
                void* target = (void*)((IntPtr)toChunk->body + toRowCapacityInBytes * toInfo.chunkIndex);
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
            
            if (++index < fromArchetype.indices.Length)
            {
                void* source = (void*)((IntPtr)fromChunk->body + fromRowCapacityInBytes * fromInfo.chunkIndex + componentsOffset.Read<int>(index));
                void* target = (void*)((IntPtr)toChunk->body + toRowCapacityInBytes * toInfo.chunkIndex + sizeInBytes);
                sizeInBytes = componentsOffset.Read<int>(componentsOffset.length - 1) - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
        
        private static unsafe void CopyRow(EcsEntityInfo fromInfo, EcsArchetype toArchetype, EcsEntityInfo toInfo, int componentTypeIndex)
        {
            EcsChunk* fromChunk = fromInfo.chunk;
            EcsChunk* toChunk = toInfo.chunk;

            int fromRowCapacityInBytes = fromChunk->rowCapacityInBytes;
            int toRowCapacityInBytes = toChunk->rowCapacityInBytes;
            
            UnsafeArray componentsOffset = *toArchetype.componentsOffset;
            int index = toArchetype.componentsMap->Get(componentTypeIndex);
            int sizeInBytes = componentsOffset.Read<int>(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)((IntPtr)fromChunk->body + fromRowCapacityInBytes * fromInfo.chunkIndex);
                void* target = (void*)((IntPtr)toChunk->body + toRowCapacityInBytes * toInfo.chunkIndex);
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
            
            if (++index < toArchetype.indices.Length)
            {
                void* source = (void*)((IntPtr)fromChunk->body + fromRowCapacityInBytes * fromInfo.chunkIndex + sizeInBytes);
                void* target = (void*)((IntPtr)toChunk->body + toRowCapacityInBytes * toInfo.chunkIndex + componentsOffset.Read<int>(index));
                sizeInBytes = componentsOffset.Read<int>(componentsOffset.length - 1) - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
    }
}