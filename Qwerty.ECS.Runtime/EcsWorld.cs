using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    /// <summary>
    /// EcsWorld manages the filtering of all entities in the World.
    /// EcsWorld maintains a list of archetypes and organizes object-related data
    /// for optimal performance.
    /// </summary>
    public class EcsWorld : IDisposable
    {
        public int ArchetypeCount => m_archetypeManager.ArchetypeCount;
        
        private int[] m_entityInArchetype;
        private int[] m_entityToArchetype;
        private EcsEntity[] m_entities;
        private EcsEntity[] m_freeEntities;
        
        private int m_entityCounter = 1;
        private int m_freeEntitiesLen = 0;
        private readonly Dictionary<EcsFilter, EcsArchetypeGroup> m_archetypeGroups = new Dictionary<EcsFilter, EcsArchetypeGroup>();
        
        private readonly byte[] m_componentTypeIndices;
        private readonly EcsArchetype[] m_archetypes;
        private readonly EcsArchetypeManager m_archetypeManager;
        private readonly IEcsComponentPool[] m_componentPools;

        public EcsWorld(int archetypeCapacity = byte.MaxValue)
        {
            m_freeEntities = new EcsEntity[0x20000];
            m_entities = new EcsEntity[0x20000];
            m_entityInArchetype = new int[0x20000];
            m_entityToArchetype = new int[0x20000];
            
            m_archetypes = new EcsArchetype[archetypeCapacity];
            m_archetypeManager = new EcsArchetypeManager(archetypeCapacity);
            
            m_componentTypeIndices = new byte[EcsTypeManager.TypeCount];
            m_componentPools = new IEcsComponentPool[EcsTypeManager.TypeCount];
            
            foreach ((int, IEcsComponentPoolCreator) item in EcsTypeManager.componentsCreators)
            {
                (int typeIndex, IEcsComponentPoolCreator creator) = item;
                m_componentPools[typeIndex] = creator.Instantiate(1024);
            }
        }
        
        public void Dispose()
        {
            for (int i = 0; i < m_archetypeManager.ArchetypeCount; i++)
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

        private EcsEntity InstantiateEntity()
        {
            int entityIndex;
            int entityVersion = 0;
            if (m_freeEntitiesLen > 0)
            {
                EcsEntity freeEntity = m_freeEntities[--m_freeEntitiesLen];
                entityIndex = freeEntity.Index;
                entityVersion = freeEntity.Version;
            }
            else
            {
                entityIndex = m_entityCounter++;
            }

            if (entityIndex >= m_entities.Length)
            {
                int newCapacity = 2 * entityIndex + 1;
                Array.Resize(ref m_entities, newCapacity);
                Array.Resize(ref m_entityInArchetype, newCapacity);
                Array.Resize(ref m_entityToArchetype, newCapacity);
            }

            EcsEntity entity = new EcsEntity(entityIndex, entityVersion + 1);
            m_entities[entityIndex] = entity;
            
            return entity;
        }
        
        public void DestroyEntity(EcsEntity entity)
        {
            int entityIndex = entity.Index;
            if (m_entities[entityIndex] == EcsEntity.Null)
            {
                throw new InvalidOperationException(nameof(entity));
            }
            
            EcsArchetype archetype = m_archetypeManager[m_entityToArchetype[entityIndex]];
            RemoveEntityFromArchetype(archetype, entity);
            m_entities[entityIndex] = EcsEntity.Null;

            if (m_freeEntitiesLen >= m_freeEntities.Length)
            {
                Array.Resize(ref m_freeEntities, 2 * m_freeEntitiesLen + 1);
            }
           
            m_freeEntities[m_freeEntitiesLen++] = entity;
            
        }

        public EcsEntity CreateEntity()
        {
            EcsEntity entity = InstantiateEntity();
            AddEntityToArchetype(m_archetypeManager.Empty, entity);
            return entity;
        }

        public EcsEntity CreateEntity<T0>(T0 component0) where T0 : struct, IEcsComponent
        {
            EcsEntity entity = InstantiateEntity();
            
            ((EcsComponentPool<T0>)m_componentPools[EcsComponentType<T0>.Index]).Write(entity.Index, component0);
            m_componentTypeIndices[0] = EcsComponentType<T0>.Index;

            AddEntityToArchetype(m_archetypeManager.FindOrCreateArchetype(m_componentTypeIndices, 1), entity);
            
            return entity;
        }

        public EcsEntity CreateEntity<T0, T1>(T0 component0, T1 component1)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
        {
            EcsEntity entity = InstantiateEntity();
            
            int entityIndex = entity.Index;
            ((EcsComponentPool<T0>)m_componentPools[EcsComponentType<T0>.Index]).Write(entityIndex, component0);
            ((EcsComponentPool<T1>)m_componentPools[EcsComponentType<T1>.Index]).Write(entityIndex, component1);
            
            m_componentTypeIndices[0] = EcsComponentType<T0>.Index;
            m_componentTypeIndices[1] = EcsComponentType<T1>.Index;

            AddEntityToArchetype(m_archetypeManager.FindOrCreateArchetype(m_componentTypeIndices, 2), entity);
            
            return entity;
        }

        public EcsEntity CreateEntity<T0, T1, T2>(T0 component0, T1 component1, T2 component2)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
        {
            EcsEntity entity = InstantiateEntity();
            
            int entityIndex = entity.Index;
            ((EcsComponentPool<T0>)m_componentPools[EcsComponentType<T0>.Index]).Write(entityIndex, component0);
            ((EcsComponentPool<T1>)m_componentPools[EcsComponentType<T1>.Index]).Write(entityIndex, component1);
            ((EcsComponentPool<T2>)m_componentPools[EcsComponentType<T2>.Index]).Write(entityIndex, component2);
            
            m_componentTypeIndices[0] = EcsComponentType<T0>.Index;
            m_componentTypeIndices[1] = EcsComponentType<T1>.Index;
            m_componentTypeIndices[2] = EcsComponentType<T2>.Index;
            
            AddEntityToArchetype(m_archetypeManager.FindOrCreateArchetype(m_componentTypeIndices, 3), entity);

            return entity;
        }

        // Check A A B B (dublicate type)
        public EcsEntity CreateEntity<T0, T1, T2, T3>(T0 component0, T1 component1, T2 component2, T3 component3)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
        {
            EcsEntity entity = InstantiateEntity();
            
            int entityIndex = entity.Index;
            ((EcsComponentPool<T0>)m_componentPools[EcsComponentType<T0>.Index]).Write(entityIndex, component0);
            ((EcsComponentPool<T1>)m_componentPools[EcsComponentType<T1>.Index]).Write(entityIndex, component1);
            ((EcsComponentPool<T2>)m_componentPools[EcsComponentType<T2>.Index]).Write(entityIndex, component2);
            ((EcsComponentPool<T3>)m_componentPools[EcsComponentType<T3>.Index]).Write(entityIndex, component3);
            
            m_componentTypeIndices[0] = EcsComponentType<T0>.Index;
            m_componentTypeIndices[1] = EcsComponentType<T1>.Index;
            m_componentTypeIndices[2] = EcsComponentType<T2>.Index;
            m_componentTypeIndices[3] = EcsComponentType<T3>.Index;
            AddEntityToArchetype(m_archetypeManager.FindOrCreateArchetype(m_componentTypeIndices, 4), entity);
            
            return entity;
        }

        public ref EcsComponentAccessor<T> GetComponentAccessor<T>() where T : struct, IEcsComponent
        {
            return ref ((EcsComponentPool<T>)m_componentPools[EcsComponentType<T>.Index]).GetAccessor();
        }

        public unsafe EcsComponentAccessor<T>* GetComponentAccessorPtr<T>() where T : struct, IEcsComponent
        {
            fixed (EcsComponentAccessor<T>* ptr = &GetComponentAccessor<T>())
            {
                return ptr;
            }
        }

        public EcsArchetypeGroup Filter(EcsFilter filter)
        {
            int version = m_archetypeManager.ArchetypeCount - 1;
            if (m_archetypeGroups.TryGetValue(filter, out EcsArchetypeGroup result))
            {
                if (result.Version >= version)
                {
                    return result;
                }
            }

            byte[] all = filter.All?.ToArray();
            byte[] any = filter.Any?.ToArray();
            byte[] none = filter.None?.ToArray();

            if (result != null)
            {
                result.Update(version, GetArchetypes(all, any, none, result.Version));
                return result;
            }

            result = new EcsArchetypeGroup(version, GetArchetypes(all, any, none, 0));
            m_archetypeGroups.Add(filter.Clone(), result);
            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<EcsArchetype> GetArchetypes(byte[] all, byte[] any, byte[] none, int startId)
        {
            HashSet<EcsArchetype> buffer0 = null;
            HashSet<EcsArchetype> buffer1 = null;

            if (all != null || any != null)
            {
                if (all != null)
                {
                    IReadOnlyList<EcsArchetype>[] archetypes = new IReadOnlyList<EcsArchetype>[all.Length];
                    for (int i = 0; i < all.Length; i++)
                    {
                        int len = m_archetypeManager.GetArchetypes(all[i], startId, m_archetypes);
                        EcsArchetype[] items = new EcsArchetype[len];
                        for (int j = 0; j < len; j++)
                        {
                            items[j] = m_archetypes[j];
                        }
                        archetypes[i] = items;
                    }

                    Array.Sort(archetypes, (a, b) => a.Count - b.Count);

                    buffer0 = new HashSet<EcsArchetype>(archetypes[0]);
                    for (int i = 1; i < all.Length; i++)
                    {
                        buffer0.IntersectWith(archetypes[i]);
                    }
                }

                if (any != null)
                {
                    buffer1 = new HashSet<EcsArchetype>();
                    foreach (byte typeIndex in any)
                    {
                        int len = m_archetypeManager.GetArchetypes(typeIndex, startId, m_archetypes);
                        for (int j = 0; j < len; j++)
                        {
                            buffer1.Add(m_archetypes[j]);
                        }
                    }
                }

                if (buffer0 != null && buffer1 != null)
                {
                    buffer0.IntersectWith(buffer1);
                }
                else if (buffer1 != null)
                {
                    buffer0 = buffer1;
                }
            }
            else
            {
                buffer0 = new HashSet<EcsArchetype>();
                int len = m_archetypeManager.GetArchetypes(startId, m_archetypes);
                for (int i = 0; i < len; i++)
                {
                    buffer0.Add(m_archetypes[i]);
                }
            }

            if (none != null)
            {
                foreach (byte type in none)
                {
                    int len = m_archetypeManager.GetArchetypes(type, startId, m_archetypes);
                    for (int i = 0; i < len; i++)
                    {
                        buffer0.Remove(m_archetypes[i]);
                    }
                }
            }

            return buffer0;
        }

        public bool HasComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            return HasComponent(entity, EcsComponentType<T>.Index);
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
            
            return ((EcsComponentPool<T>)m_componentPools[EcsComponentType<T>.Index]).Read(entity.Index);
        }
        
        public void AddComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.Index;
            if (HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }
            
            int entityIndex = entity.Index;
            
            ((EcsComponentPool<T>)m_componentPools[EcsComponentType<T>.Index]).Write(entityIndex, component);
            
            EcsArchetype currentArchetype = m_archetypeManager[m_entityToArchetype[entityIndex]];
            EcsArchetype newArchetype = m_archetypeManager.FindOrCreateNextArchetype(currentArchetype, componentTypeIndex);
            
            RemoveEntityFromArchetype(currentArchetype, entity);
            AddEntityToArchetype(newArchetype, entity);
        }
        
        public void SetComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.Index;
            if (!HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }
            
            ((EcsComponentPool<T>)m_componentPools[EcsComponentType<T>.Index]).Write(entity.Index, component);
        }

        public void RemoveComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.Index;
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

        private void AddEntityToArchetype(EcsArchetype archetype, in EcsEntity entity)
        {
            EcsEntityCollection entities = archetype.Entities;
            int index = entities.count;
            if (index >= entities.length)
            {
                entities.Resize(2 * index + 1);
            }
            
            entities.IncCount();
            entities[index] = entity;
            m_entityInArchetype[entity.Index] = index;
            m_entityToArchetype[entity.Index] = archetype.Index;
        }

        private void RemoveEntityFromArchetype(EcsArchetype archetype, in EcsEntity entity)
        {
            EcsEntityCollection entities = archetype.Entities;
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