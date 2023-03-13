using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public unsafe partial class EcsWorld : IDisposable
    {
        public int archetypeCount => m_archetypeManager.archetypeCount;

        private readonly UnsafeArray* m_entityArchetypeInfo;
        private EcsEntity[] m_entities;
        private EcsEntity[] m_freeEntities;

        private int m_entityCounter;
        private int m_freeEntitiesLen;

        private readonly Dictionary<EcsFilter, EcsArchetypeGroup> m_archetypeGroups = new Dictionary<EcsFilter, EcsArchetypeGroup>();

        private readonly byte[] m_componentTypeIndices;
        private readonly EcsArchetypeManager m_archetypeManager;
        //private readonly IEcsComponentPool[] m_componentPools;

        public EcsWorld(EcsWorldSetting setting)
        {
            m_freeEntities = new EcsEntity[0x20000];
            m_entities = new EcsEntity[0x20000];
            
            m_entityArchetypeInfo = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
            m_entityArchetypeInfo->Realloc<EcsArchetypeChunkInfo>(0x20000);

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

        public void Dispose()
        {
            for (int i = 0; i < m_archetypeManager.archetypeCount; i++)
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
            
            m_entityArchetypeInfo->Dispose();
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
        
        private void Swap(EcsArchetype archetype, EcsArchetypeChunkInfo chunkInfo)
        {
            EcsArchetypeChunk* toChunk = chunkInfo.chunk;
            EcsArchetypeChunk* lastChunk = archetype.chunks->last;
            int lastIndex = *lastChunk->count - 1;
            if (toChunk != lastChunk || chunkInfo.index != lastIndex)
            {
                int rowCapacityInBytes = archetype.m_rowCapacityInBytes;
                EcsEntity swapEntity = lastChunk->ReadEntity(lastIndex);
                void* sourcePtr = (void*)((IntPtr)lastChunk->body + rowCapacityInBytes * lastIndex);
                void* targetPtr = (void*)((IntPtr)toChunk->body + rowCapacityInBytes * chunkInfo.index);
                Buffer.MemoryCopy(sourcePtr, targetPtr, rowCapacityInBytes, rowCapacityInBytes);
                m_entityArchetypeInfo->Write(swapEntity.Index, new EcsArchetypeChunkInfo(archetype.archetypeIndex, chunkInfo.index, toChunk));
            }
            
            --*lastChunk->count;
            if (*lastChunk->count != 0 || lastChunk->prior == null)
            {
                return;
            }
            --*archetype.m_chunksCount;
            archetype.chunks->last = lastChunk->prior;
            lastChunk->Dispose();
            MemoryUtilities.Free((IntPtr)lastChunk);
        }

        private static void Copy(EcsArchetype fromArchetype, EcsArchetypeChunkInfo fromChunkInfo, EcsArchetypeChunkInfo toChunkInfo, int componentTypeIndex)
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
        
        private static void Copy(EcsArchetypeChunkInfo fromChunkInfo, EcsArchetype toArchetype, EcsArchetypeChunkInfo toChunkInfo, int componentTypeIndex)
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