using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public partial class EcsWorld : IDisposable
    {
        public int archetypeCount => m_archetypeManager.archetypeCounter;

        private readonly unsafe UnsafeArray* m_entitiesInfo;
        private readonly unsafe UnsafeArray* m_entities;
        private readonly unsafe UnsafeArray* m_freeEntities;

        private int m_entityCounter;
        private int m_freeEntitiesLen;

        private readonly Dictionary<EcsFilter, EcsArchetypeGroup> m_archetypeGroups = new Dictionary<EcsFilter, EcsArchetypeGroup>();

        private readonly byte[] m_componentTypeIndices;
        private readonly EcsArchetypeManager m_archetypeManager;
        //private readonly IEcsComponentPool[] m_componentPools;

        public unsafe EcsWorld(EcsWorldSetting setting)
        {
            m_freeEntities = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
            m_freeEntities->Realloc<EcsEntity>(0x20000);
            
            m_entities = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
            m_entities->Realloc<EcsEntity>(0x20000);
            
            m_entitiesInfo = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
            m_entitiesInfo->Realloc<EcsArchetypeChunkInfo>(0x20000);

            m_archetypeManager = new EcsArchetypeManager(setting);

            m_componentTypeIndices = new byte[EcsTypeManager.typeCount];
            //m_componentPools = new IEcsComponentPool[EcsTypeManager.typeCount];

            // foreach ((int, IEcsComponentPoolCreator) item in EcsTypeManager.ComponentsCreators)
            // {
            //     (int typeIndex, IEcsComponentPoolCreator creator) = item;
            //     m_componentPools[typeIndex] = creator.Instantiate(1024);
            // }
        }

        public EcsArchetype GetArchetype<T0>() where T0 : struct, IEcsComponent
        {
            m_componentTypeIndices[0] = EcsComponentType<T0>.index;
            return m_archetypeManager.FindOrCreateArchetype(m_componentTypeIndices, 1);
        }
        
        public EcsArchetype GetArchetype<T0, T1>() where T0 : struct, IEcsComponent where T1 : struct, IEcsComponent
        {
            m_componentTypeIndices[0] = EcsComponentType<T0>.index;
            m_componentTypeIndices[1] = EcsComponentType<T1>.index;
            Array.Sort(m_componentTypeIndices, 0, 2);

            if (!CheckIndices(m_componentTypeIndices, 2))
            {
                throw new ArgumentException(nameof(GetArchetype));
            }
            return m_archetypeManager.FindOrCreateArchetype(m_componentTypeIndices, 2);
        }
        
        public EcsArchetype GetArchetype<T0, T1, T2>() where T0 : struct, IEcsComponent where T1 : struct, IEcsComponent where T2 : struct, IEcsComponent
        {
            m_componentTypeIndices[0] = EcsComponentType<T0>.index;
            m_componentTypeIndices[1] = EcsComponentType<T1>.index;
            m_componentTypeIndices[2] = EcsComponentType<T2>.index;
            Array.Sort(m_componentTypeIndices, 0, 3);

            if (!CheckIndices(m_componentTypeIndices, 3))
            {
                throw new ArgumentException(nameof(GetArchetype));
            }
            return m_archetypeManager.FindOrCreateArchetype(m_componentTypeIndices, 3);
        }

        public unsafe void Dispose()
        {
            for (int i = 0; i < m_archetypeManager.archetypeCounter; i++)
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
        
        private unsafe void PushEntity(EcsArchetype archetype, EcsEntity entity, out EcsArchetypeChunkInfo chunkInfo)
        {
            EcsArchetype.Chunks* chunks = archetype.chunks;
            EcsArchetypeChunk* chunk = chunks->last;
            int chunkCapacity = archetype.chunkCapacity;
            if (chunk == null || *chunk->count == chunkCapacity)
            {
                EcsArchetypeChunk* lastChunk = (EcsArchetypeChunk*)MemoryUtilities.Alloc<EcsArchetypeChunk>(1);
                lastChunk->Alloc(archetype.chunkSizeInBytes, archetype.rowCapacityInBytes, archetype.componentsMap, archetype.componentsOffset);
                lastChunk->prior = chunks->last;
                *lastChunk->start = archetype.chunksCnt * chunkCapacity;
                *lastChunk->count = 0;
            
                chunks->last = lastChunk;
                archetype.chunksCnt++;
            }
            chunk = chunks->last;
            int indexInChunk = (*chunk->count)++;
            chunk->WriteEntity(indexInChunk, entity);
            
            chunkInfo = new EcsArchetypeChunkInfo(archetype.index, indexInChunk, chunk);
            m_entitiesInfo->Write(entity.Index, chunkInfo);
        }
        
        private unsafe void SwapEntity(EcsArchetype archetype, EcsArchetypeChunkInfo info)
        {
            EcsArchetypeChunk* toChunk = info.chunk;
            EcsArchetypeChunk* lastChunk = archetype.chunks->last;
            int lastIndex = *lastChunk->count - 1;
            if (toChunk != lastChunk || info.index != lastIndex)
            {
                int rowCapacityInBytes = archetype.rowCapacityInBytes;
                EcsEntity swapEntity = lastChunk->ReadEntity(lastIndex);
                void* sourcePtr = (void*)((IntPtr)lastChunk->body + rowCapacityInBytes * lastIndex);
                void* targetPtr = (void*)((IntPtr)toChunk->body + rowCapacityInBytes * info.index);
                Buffer.MemoryCopy(sourcePtr, targetPtr, rowCapacityInBytes, rowCapacityInBytes);
                m_entitiesInfo->Write(swapEntity.Index, new EcsArchetypeChunkInfo(archetype.index, info.index, toChunk));
            }
            
            --*lastChunk->count;
            if (*lastChunk->count > 0)
            {
                return;
            }
            
            archetype.chunksCnt--;
            archetype.chunks->last = lastChunk->prior;
            
            lastChunk->Dispose();
            MemoryUtilities.Free((IntPtr)lastChunk);
        }

        private static unsafe void CopyEntity(EcsArchetype fromArchetype, EcsArchetypeChunkInfo fromChunkInfo, EcsArchetypeChunkInfo toChunkInfo, int componentTypeIndex)
        {
            EcsArchetypeChunk* fromChunk = fromChunkInfo.chunk;
            EcsArchetypeChunk* toChunk = toChunkInfo.chunk;
            
            int fromRowCapacityInBytes = fromChunk->rowCapacityInBytes;
            int toRowCapacityInBytes = toChunk->rowCapacityInBytes;
            
            int index = fromArchetype.componentsMap->Get(componentTypeIndex);
            int sizeInBytes = fromArchetype.componentsOffset->Read<int>(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)((IntPtr)fromChunk->body + fromRowCapacityInBytes * fromChunkInfo.index);
                void* target = (void*)((IntPtr)toChunk->body + toRowCapacityInBytes * toChunkInfo.index);
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
            
            if (++index < fromArchetype.typeIndices.Length)
            {
                int offset = fromArchetype.componentsOffset->Read<int>(index);
                void* source = (void*)((IntPtr)fromChunk->body + fromRowCapacityInBytes * fromChunkInfo.index + offset);
                void* target = (void*)((IntPtr)toChunk->body + toRowCapacityInBytes * toChunkInfo.index + sizeInBytes);
                sizeInBytes = fromRowCapacityInBytes - Unsafe.SizeOf<EcsEntity>() - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
        
        private static unsafe void CopyEntity(EcsArchetypeChunkInfo fromChunkInfo, EcsArchetype toArchetype, EcsArchetypeChunkInfo toChunkInfo, int componentTypeIndex)
        {
            EcsArchetypeChunk* fromChunk = fromChunkInfo.chunk;
            EcsArchetypeChunk* toChunk = toChunkInfo.chunk;

            int fromRowCapacityInBytes = fromChunk->rowCapacityInBytes;
            int toRowCapacityInBytes = toChunk->rowCapacityInBytes;
            
            int index = toArchetype.componentsMap->Get(componentTypeIndex);
            int sizeInBytes = toArchetype.componentsOffset->Read<int>(index);
            if (sizeInBytes > 0)
            {
                void* source = (void*)((IntPtr)fromChunk->body + fromRowCapacityInBytes * fromChunkInfo.index);
                void* target = (void*)((IntPtr)toChunk->body + toRowCapacityInBytes * toChunkInfo.index);
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
            
            if (++index < toArchetype.typeIndices.Length)
            {
                int offset = toArchetype.componentsOffset->Read<int>(index);
                void* source = (void*)((IntPtr)fromChunk->body + fromRowCapacityInBytes * fromChunkInfo.index + sizeInBytes);
                void* target = (void*)((IntPtr)toChunk->body + toRowCapacityInBytes * toChunkInfo.index + offset);
                sizeInBytes = fromRowCapacityInBytes - Unsafe.SizeOf<EcsEntity>() - sizeInBytes;
                Buffer.MemoryCopy(source, target, sizeInBytes, sizeInBytes);
            }
        }
    }
}