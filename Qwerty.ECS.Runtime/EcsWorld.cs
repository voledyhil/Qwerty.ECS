using System;
using System.Collections.Generic;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public partial class EcsWorld : IDisposable
    {
        public int archetypeCount => m_archetypeManager.archetypeCount;

        private int[] m_entityInArchetype;
        private int[] m_entityToArchetype;
        private EcsEntity[] m_entities;
        private EcsEntity[] m_freeEntities;

        private int m_entityCounter = 1;
        private int m_freeEntitiesLen = 0;

        private readonly Dictionary<EcsFilter, EcsArchetypeGroup> m_archetypeGroups = new Dictionary<EcsFilter, EcsArchetypeGroup>();

        private readonly byte[] m_componentTypeIndices;
        private readonly EcsArchetypeManager m_archetypeManager;
        private readonly IEcsComponentPool[] m_componentPools;

        public EcsWorld(int archetypeCapacity = byte.MaxValue)
        {
            m_freeEntities = new EcsEntity[0x20000];
            m_entities = new EcsEntity[0x20000];
            m_entityInArchetype = new int[0x20000];
            m_entityToArchetype = new int[0x20000];

            m_archetypeManager = new EcsArchetypeManager(archetypeCapacity);

            m_componentTypeIndices = new byte[EcsTypeManager.typeCount];
            m_componentPools = new IEcsComponentPool[EcsTypeManager.typeCount];

            foreach ((int, IEcsComponentPoolCreator) item in EcsTypeManager.componentsCreators)
            {
                (int typeIndex, IEcsComponentPoolCreator creator) = item;
                m_componentPools[typeIndex] = creator.Instantiate(1024);
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < m_archetypeManager.archetypeCount; i++)
            {
                m_archetypeManager[i].Dispose();
            }

            foreach ((int, IEcsComponentPoolCreator) item in EcsTypeManager.componentsCreators)
            {
                m_componentPools[item.Item1].Dispose();
            }

            foreach (EcsArchetypeGroup archetypeGroup in m_archetypeGroups.Values)
            {
                archetypeGroup.Dispose();
            }
        }

        public ref EcsComponentAccessor<T> GetComponentAccessor<T>() where T : struct, IEcsComponent
        {
            return ref ((EcsComponentPool<T>)m_componentPools[EcsComponentType<T>.index]).GetAccessor();
        }

        public unsafe EcsComponentAccessor<T>* GetComponentAccessorPtr<T>() where T : struct, IEcsComponent
        {
            fixed (EcsComponentAccessor<T>* ptr = &GetComponentAccessor<T>())
            {
                return ptr;
            }
        }

        private void AddEntityToArchetype(EcsArchetype archetype, in EcsEntity entity)
        {
            EcsEntityCollection entities = archetype.entities;
            int index = entities.count;
            if (index >= entities.length)
            {
                entities.Resize(2 * index + 1);
            }

            entities.IncCount();
            entities[index] = entity;
            m_entityInArchetype[entity.Index] = index;
            m_entityToArchetype[entity.Index] = archetype.index;
        }

        private void RemoveEntityFromArchetype(EcsArchetype archetype, in EcsEntity entity)
        {
            EcsEntityCollection entities = archetype.entities;
            int index = m_entityInArchetype[entity.Index];
            int lastIndex = entities.count - 1;
            if (index == lastIndex)
            {
                entities.DecCount();
                return;
            }

            EcsEntity swapEntity = entities[lastIndex];
            entities[index] = entities[lastIndex];
            entities.DecCount();

            m_entityInArchetype[swapEntity.Index] = index;
        }
    }
}