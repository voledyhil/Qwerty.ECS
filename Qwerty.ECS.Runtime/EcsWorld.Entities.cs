using System;
using System.ComponentModel;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Chunks;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public partial class EcsWorld
    {
        private EcsEntity InstantiateEntity()
        {
            int entityIndex;
            int entityVersion = 0;
            if (m_freeEntitiesLen > 0)
            {
                EcsEntity freeEntity = MemoryUtil.ReadElement<EcsEntity>(m_freeEntities, --m_freeEntitiesLen);
                entityIndex = freeEntity.Index;
                entityVersion = freeEntity.Version;
            }
            else
            {
                entityIndex = ++m_entityCounter;
            }

            if (entityIndex >= m_entitiesCapacity)
            {
                throw new InvalidEnumArgumentException(nameof(InstantiateEntity));
            }

            EcsEntity entity = new EcsEntity(entityIndex, entityVersion + 1);
            MemoryUtil.WriteElement(m_entities, entityIndex, entity);
            return entity;
        }
        
        public void DestroyEntity(EcsEntity entity)
        {
            int entityIndex = entity.Index;
            
            if (MemoryUtil.ReadElement<EcsEntity>(m_entities, entityIndex) == EcsEntity.Null)
            {
                throw new InvalidOperationException(nameof(entity));
            }

            EcsEntityInfo info = MemoryUtil.ReadElement<EcsEntityInfo>(m_entitiesInfo, entity.Index);
            EcsArchetype archetype = m_arcManager[info.archetypeIndex];
            
            SwapRow(archetype, info);
            
            MemoryUtil.WriteElement(m_entities, entityIndex, EcsEntity.Null);
            MemoryUtil.WriteElement(m_freeEntities, m_freeEntitiesLen++, entity);
        }
        
        public EcsEntity CreateEntity()
        {
            EcsEntity entity = InstantiateEntity();
            EcsArchetype archetype = m_arcManager.empty;
            PushEntity(archetype, entity, out _);
            return entity;
        }

        public unsafe EcsEntity CreateEntity<T0>(T0 c0) where T0 : struct, IEcsComponent
        {
            EcsEntity entity = InstantiateEntity();
            int typeIndex = EcsTypeIndex<T0>.value.index;
            m_indicesBuffer[0] = typeIndex;
            
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 1);
            PushEntity(archetype, entity, out EcsEntityInfo info);

            EcsChunk chunk = *info.chunk;
            chunk.WriteComponent(info.indexInChunk, typeIndex, c0);
            
            return entity;
        }

        public unsafe EcsEntity CreateEntity<T0, T1>(T0 c0, T1 c1)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
        {
            int typeIndex0 = EcsTypeIndex<T0>.value.index;
            int typeIndex1 = EcsTypeIndex<T1>.value.index;
            
            m_indicesBuffer[0] = typeIndex0;
            m_indicesBuffer[1] = typeIndex1;
            
            Array.Sort(m_indicesBuffer, 0, 2);

            if (!ValidateIndices(m_indicesBuffer, 2))
            {
                throw new ArgumentException(nameof(CreateEntity));
            }

            EcsEntity entity = InstantiateEntity();
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 2);
            PushEntity(archetype, entity, out EcsEntityInfo info);
            
            EcsChunk chunk = *info.chunk;
            chunk.WriteComponent(info.indexInChunk, typeIndex0, c0);
            chunk.WriteComponent(info.indexInChunk, typeIndex1, c1);
            
            return entity;
        }

        public unsafe EcsEntity CreateEntity<T0, T1, T2>(T0 c0, T1 c1, T2 c2)
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
                throw new ArgumentException(nameof(CreateEntity));
            }

            EcsEntity entity = InstantiateEntity();
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 3);
            PushEntity(archetype, entity, out EcsEntityInfo info);
            
            EcsChunk chunk = *info.chunk;
            chunk.WriteComponent(info.indexInChunk, typeIndex0, c0);
            chunk.WriteComponent(info.indexInChunk, typeIndex1, c1);
            chunk.WriteComponent(info.indexInChunk, typeIndex2, c2);

            return entity;
        }

        public unsafe EcsEntity CreateEntity<T0, T1, T2, T3>(T0 c0, T1 c1, T2 c2, T3 c3)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
        {
            int typeIndex0 = EcsTypeIndex<T0>.value.index;
            int typeIndex1 = EcsTypeIndex<T1>.value.index;
            int typeIndex2 = EcsTypeIndex<T2>.value.index;
            int typeIndex3 = EcsTypeIndex<T3>.value.index;

            m_indicesBuffer[0] = typeIndex0;
            m_indicesBuffer[1] = typeIndex1;
            m_indicesBuffer[2] = typeIndex2;
            m_indicesBuffer[3] = typeIndex3;
            
            Array.Sort(m_indicesBuffer, 0, 4);

            if (!ValidateIndices(m_indicesBuffer, 4))
            {
                throw new ArgumentException(nameof(CreateEntity));
            }

            EcsEntity entity = InstantiateEntity();
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 4);
            PushEntity(archetype, entity, out EcsEntityInfo info);
            
            EcsChunk chunk = *info.chunk;
            chunk.WriteComponent(info.indexInChunk, typeIndex0, c0);
            chunk.WriteComponent(info.indexInChunk, typeIndex1, c1);
            chunk.WriteComponent(info.indexInChunk, typeIndex2, c2);
            chunk.WriteComponent(info.indexInChunk, typeIndex3, c3);
            
            return entity;
        }

        private static bool ValidateIndices(int[] indices, int len)
        {
            for (int i = 0; i < len - 1; i++)
            {
                if (indices[i] == indices[i + 1])
                {
                    return false;
                }
            }
            return true;
        }
    }
}