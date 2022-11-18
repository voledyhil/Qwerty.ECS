using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime
{
    public partial class EcsWorld
    {
        public bool HasComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            return HasComponent(entity, EcsComponentType<T>.index);
        }

        private bool HasComponent(EcsEntity entity, byte index)
        {
            return m_archetypeManager[m_entityToArchetype[entity.Index]].TypeIndicesSet.Contains(index);
        }

        public T GetComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            if (!HasComponent<T>(entity))
            {
                throw new InvalidOperationException();
            }

            return ((EcsComponentPool<T>)m_componentPools[EcsComponentType<T>.index]).Read(entity.Index);
        }

        public void AddComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }

            int entityIndex = entity.Index;

            ((EcsComponentPool<T>)m_componentPools[EcsComponentType<T>.index]).Write(entityIndex, component);

            EcsArchetype currentArchetype = m_archetypeManager[m_entityToArchetype[entityIndex]];
            EcsArchetype newArchetype = m_archetypeManager.FindOrCreateNextArchetype(currentArchetype, componentTypeIndex);

            RemoveEntityFromArchetype(currentArchetype, entity);
            AddEntityToArchetype(newArchetype, entity);
        }

        public void SetComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }

            ((EcsComponentPool<T>)m_componentPools[EcsComponentType<T>.index]).Write(entity.Index, component);
        }

        public void RemoveComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }

            int entityIndex = entity.Index;
            EcsArchetype currentArchetype = m_archetypeManager[m_entityToArchetype[entityIndex]];
            EcsArchetype newArchetype = m_archetypeManager.FindOrCreatePriorArchetype(currentArchetype, componentTypeIndex);

            RemoveEntityFromArchetype(currentArchetype, entity);
            AddEntityToArchetype(newArchetype, entity);
        }
    }
}