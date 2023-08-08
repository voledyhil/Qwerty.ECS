using System;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Chunks;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public partial class EcsWorld
    {
        public bool HasComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            return HasComponent(entity, EcsTypeIndex<T>.value.index);
        }

        private unsafe bool HasComponent(EcsEntity entity, int index)
        {
            EcsEntityInfo info = MemoryUtil.ReadElement<EcsEntityInfo>(m_entitiesInfo, entity.Index);
            return info.chunk->header->ContainType(index);
        }

        public unsafe T GetComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            if (!HasComponent<T>(entity))
            {
                throw new InvalidOperationException();
            }
            EcsEntityInfo info = MemoryUtil.ReadElement<EcsEntityInfo>(m_entitiesInfo, entity.Index);
            return info.chunk->ReadComponent<T>(info.indexInChunk, EcsTypeIndex<T>.value.index);
        }
        
        public unsafe void SetComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            int typeIndex = EcsTypeIndex<T>.value.index;
            if (!HasComponent(entity, typeIndex))
            {
                throw new InvalidOperationException();
            }

            EcsEntityInfo info = MemoryUtil.ReadElement<EcsEntityInfo>(m_entitiesInfo, entity.Index);
            info.chunk->WriteComponent<T>(info.indexInChunk, EcsTypeIndex<T>.value.index, component);
        }

        public void RemoveComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            int typeIndex = EcsTypeIndex<T>.value.index;
            if (!HasComponent(entity, typeIndex))
            {
                throw new InvalidOperationException();
            }

            EcsEntityInfo fromInfo = MemoryUtil.ReadElement<EcsEntityInfo>(m_entitiesInfo, entity.Index);
            EcsArchetype fromArchetype = m_arcManager[fromInfo.archetypeIndex];
            EcsArchetype toArchetype = m_arcManager.FindOrCreatePriorArchetype(fromArchetype, typeIndex);
            
            PushEntity(toArchetype, entity, out EcsEntityInfo toInfo);
            CopyToPrior(fromInfo, toInfo, typeIndex);
            SwapRow(fromArchetype, fromInfo);
        }
        
        public unsafe void AddComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            int typeIndex = EcsTypeIndex<T>.value.index;
            if (HasComponent(entity, typeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsEntityInfo fromInfo = MemoryUtil.ReadElement<EcsEntityInfo>(m_entitiesInfo, entity.Index);
            EcsArchetype fromArchetype = m_arcManager[fromInfo.archetypeIndex];
            
            EcsArchetype toArchetype = m_arcManager.FindOrCreateNextArchetype(fromArchetype, typeIndex);
            PushEntity(toArchetype, entity, out EcsEntityInfo toInfo);
            CopyToNext(fromInfo, toInfo, typeIndex);
            SwapRow(fromArchetype, fromInfo);
            
            toInfo.chunk->WriteComponent(toInfo.indexInChunk, typeIndex, component);
        }

        public EcsComponentDataFromEntity<T> GetComponentDataFromEntityAccessor<T>() where T : struct, IEcsComponent
        {
            return new EcsComponentDataFromEntity<T>(m_entitiesInfo, new EcsComponentTypeHandle<T>(EcsTypeIndex<T>.value.index));
        }
        
        public EcsComponentTypeHandle<T> GetComponentTypeHandle<T>() where T : struct, IEcsComponent
        {
            return new EcsComponentTypeHandle<T>(EcsTypeIndex<T>.value.index);
        }
        
        public unsafe EcsEntityWriter<T0, T1, T2> InstantiateEntityWriter<T0, T1, T2>(int capacity)  
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
        {
            int typeIndex0 = EcsTypeIndex<T0>.value.index;
            int typeIndex1 = EcsTypeIndex<T1>.value.index;
            int typeIndex2 = EcsTypeIndex<T2>.value.index;
            
            m_indicesBuffer[0] = typeIndex0;
            m_indicesBuffer[1] = typeIndex1;
            m_indicesBuffer[2] = typeIndex2;
            
            Array.Sort(m_indicesBuffer, 0, 3);

            if (!ValidateIndices(m_indicesBuffer, 3))
            {
                throw new ArgumentException(nameof(InstantiateEntityWriter));
            }
            
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 3);
            EcsChunkHeader* header = archetype.chunks->header;
            
            return new EcsEntityWriter<T0, T1, T2>(
                capacity, 
                header->rowSizeInBytes,
                header->ReadOffsetByType(typeIndex0),
                header->ReadOffsetByType(typeIndex1), 
                header->ReadOffsetByType(typeIndex2));
        }
        
        public unsafe void Write<T0, T1, T2>(EcsEntityWriter<T0, T1, T2> writer)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
        {
            m_indicesBuffer[0] = EcsTypeIndex<T0>.value.index;
            m_indicesBuffer[1] = EcsTypeIndex<T1>.value.index;
            m_indicesBuffer[2] = EcsTypeIndex<T2>.value.index;
            
            Array.Sort(m_indicesBuffer, 0, 3);
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 3);
            
            for (int index = 0; index < writer.length; index++)
            {
                PushEntity(archetype, InstantiateEntity(), out EcsEntityInfo info);
                writer.CopyRow(index, (*info.chunk).body, info.indexInChunk);
            }

        }
    }
}