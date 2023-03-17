using System;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public partial class EcsWorld
    {
        public bool HasComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            return HasComponent(entity, EcsComponentType<T>.index);
        }

        private unsafe bool HasComponent(EcsEntity entity, byte index)
        {
            EcsEntityInfo info = m_entitiesInfo->Read<EcsEntityInfo>(entity.Index);
            return m_arcManager[info.archetypeIndex].indexMap->Contains(index);
        }

        public unsafe T GetComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            if (!HasComponent<T>(entity))
            {
                throw new InvalidOperationException();
            }
            EcsEntityInfo info = m_entitiesInfo->Get<EcsEntityInfo>(entity.Index);
            return info.chunk->ReadComponent<T>(info.indexInChunk, EcsComponentType<T>.index);
        }
        
        public unsafe void SetComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte typeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, typeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsEntityInfo info = m_entitiesInfo->Get<EcsEntityInfo>(entity.Index);
            info.chunk->WriteComponent<T>(info.indexInChunk, EcsComponentType<T>.index, component);
        }

        public unsafe void RemoveComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            byte typeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, typeIndex))
            {
                throw new InvalidOperationException();
            }

            EcsEntityInfo fromInfo = m_entitiesInfo->Read<EcsEntityInfo>(entity.Index);
            
            EcsArchetype fromArchetype = m_arcManager[fromInfo.archetypeIndex];
            EcsArchetype toArchetype = m_arcManager.FindOrCreatePriorArchetype(fromArchetype, typeIndex);
            
            PushEntity(toArchetype, entity, out EcsEntityInfo toInfo);
            CopyRow(fromArchetype, fromInfo, toInfo, typeIndex);
            SwapRow(fromArchetype, fromInfo);
        }
        
        public unsafe void AddComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte typeIndex = EcsComponentType<T>.index;
            if (HasComponent(entity, typeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsEntityInfo fromInfo = m_entitiesInfo->Read<EcsEntityInfo>(entity.Index);
            EcsArchetype fromArchetype = m_arcManager[fromInfo.archetypeIndex];
            
            EcsArchetype toArchetype = m_arcManager.FindOrCreateNextArchetype(fromArchetype, typeIndex);
            PushEntity(toArchetype, entity, out EcsEntityInfo toInfo);
            CopyRow(fromInfo, toArchetype, toInfo, typeIndex);
            SwapRow(fromArchetype, fromInfo);
            
            toInfo.chunk->WriteComponent(toInfo.indexInChunk, typeIndex, component);
        }

        public unsafe EcsComponentDataFromEntity<T> GetComponentArrayAccessor<T>() where T : struct, IEcsComponent
        {
            return new EcsComponentDataFromEntity<T>(m_entitiesInfo);
        }
    }
}