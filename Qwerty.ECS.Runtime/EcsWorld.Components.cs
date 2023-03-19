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
            EcsEntityInfo info = MemoryUtil.Read<EcsEntityInfo>(m_entitiesInfo, entity.Index * m_sizeOfEntityInfo);
            return info.chunk->header->ContainType(index);
        }

        public unsafe T GetComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            if (!HasComponent<T>(entity))
            {
                throw new InvalidOperationException();
            }
            EcsEntityInfo info = MemoryUtil.Read<EcsEntityInfo>(m_entitiesInfo, entity.Index * m_sizeOfEntityInfo);
            return info.chunk->ReadComponent<T>(info.indexInChunk, EcsComponentType<T>.index);
        }
        
        public unsafe void SetComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte typeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, typeIndex))
            {
                throw new InvalidOperationException();
            }

            EcsEntityInfo info = MemoryUtil.Read<EcsEntityInfo>(m_entitiesInfo, entity.Index * m_sizeOfEntityInfo);
            info.chunk->WriteComponent<T>(info.indexInChunk, EcsComponentType<T>.index, component);
        }

        public void RemoveComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            byte typeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, typeIndex))
            {
                throw new InvalidOperationException();
            }

            EcsEntityInfo fromInfo = MemoryUtil.Read<EcsEntityInfo>(m_entitiesInfo, entity.Index * m_sizeOfEntityInfo);
            EcsArchetype fromArchetype = m_arcManager[fromInfo.archetypeIndex];
            EcsArchetype toArchetype = m_arcManager.FindOrCreatePriorArchetype(fromArchetype, typeIndex);
            
            PushEntity(toArchetype, entity, out EcsEntityInfo toInfo);
            CopyToPrior(fromInfo, toInfo, typeIndex);
            SwapRow(fromArchetype, fromInfo);
        }
        
        public unsafe void AddComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte typeIndex = EcsComponentType<T>.index;
            if (HasComponent(entity, typeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsEntityInfo fromInfo = MemoryUtil.Read<EcsEntityInfo>(m_entitiesInfo, entity.Index * m_sizeOfEntityInfo);
            EcsArchetype fromArchetype = m_arcManager[fromInfo.archetypeIndex];
            
            EcsArchetype toArchetype = m_arcManager.FindOrCreateNextArchetype(fromArchetype, typeIndex);
            PushEntity(toArchetype, entity, out EcsEntityInfo toInfo);
            CopyToNext(fromInfo, toInfo, typeIndex);
            SwapRow(fromArchetype, fromInfo);
            
            toInfo.chunk->WriteComponent(toInfo.indexInChunk, typeIndex, component);
        }

        public EcsComponentDataFromEntity<T> GetComponentArrayAccessor<T>() where T : struct, IEcsComponent
        {
            return new EcsComponentDataFromEntity<T>(m_entitiesInfo);
        }
    }
}