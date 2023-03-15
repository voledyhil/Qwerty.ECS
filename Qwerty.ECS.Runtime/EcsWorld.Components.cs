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
            return m_archetypeManager[info.archetypeIndex].indexMap->Contains(index);
        }

        public unsafe T GetComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            if (!HasComponent<T>(entity))
            {
                throw new InvalidOperationException();
            }
            EcsEntityInfo info = m_entitiesInfo->Read<EcsEntityInfo>(entity.Index);
            return info.chunk->ReadComponent<T>(info.chunkIndex);
        }
        
        public unsafe void SetComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsEntityInfo info = m_entitiesInfo->Read<EcsEntityInfo>(entity.Index);
            info.chunk->WriteComponent<T>(info.chunkIndex, component);
        }

        public unsafe void RemoveComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }

            EcsEntityInfo fromInfo = m_entitiesInfo->Read<EcsEntityInfo>(entity.Index);
            
            EcsArchetype fromArchetype = m_archetypeManager[fromInfo.archetypeIndex];
            EcsArchetype toArchetype = m_archetypeManager.FindOrCreatePriorArchetype(fromArchetype, componentTypeIndex);
            
            PushEntity(toArchetype, entity, out EcsEntityInfo toInfo);
            CopyRow(fromArchetype, fromInfo, toInfo, componentTypeIndex);
            SwapRow(fromArchetype, fromInfo);
        }
        
        public unsafe void AddComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsEntityInfo fromInfo = m_entitiesInfo->Read<EcsEntityInfo>(entity.Index);
            EcsArchetype fromArchetype = m_archetypeManager[fromInfo.archetypeIndex];
            
            EcsArchetype toArchetype = m_archetypeManager.FindOrCreateNextArchetype(fromArchetype, componentTypeIndex);
            PushEntity(toArchetype, entity, out EcsEntityInfo toInfo);
            CopyRow(fromInfo, toArchetype, toInfo, componentTypeIndex);
            SwapRow(fromArchetype, fromInfo);
            
            toInfo.chunk->WriteComponent(toInfo.chunkIndex, component);
        }

        public unsafe EcsComponentArrayAccessor<T> GetComponentArrayAccessor<T>() where T : struct, IEcsComponent
        {
            return new EcsComponentArrayAccessor<T>(m_entitiesInfo);
        }
    }
}