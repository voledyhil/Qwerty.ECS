// ReSharper disable once CheckNamespace

using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime
{
    public unsafe partial class EcsWorld
    {
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
                entityIndex = ++m_entityCounter;
            }

            if (entityIndex >= m_entities.Length)
            {
                int newCapacity = 2 * entityIndex + 1;
                Array.Resize(ref m_entities, newCapacity);
                Array.Resize(ref m_freeEntities, newCapacity);
                m_entityArchetypeInfo->Realloc<EcsArchetypeInfo>(newCapacity);
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

            EcsArchetypeInfo curEcsArchetypeInfo = m_entityArchetypeInfo->Read<EcsArchetypeInfo>(entity.Index);
            int indexInChunk = curEcsArchetypeInfo.indexInChunk;
            EcsArchetypeChunk* chunk = curEcsArchetypeInfo.chunk;
            int archetypeIndex = curEcsArchetypeInfo.archetypeIndex;
            
            EcsArchetype archetype = m_archetypeManager[archetypeIndex];
            archetype.Swap(indexInChunk, chunk, m_entityArchetypeInfo);
            

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
            EcsArchetype archetype = m_archetypeManager.empty;
            EcsArchetypeChunk* chunk = archetype.PushEntity(entity, out int indexInChunk);
            m_entityArchetypeInfo->Write(entity.Index, new EcsArchetypeInfo(archetype.archetypeIndex, indexInChunk, chunk));
            return entity;
        }

        public EcsEntity CreateEntity<T0>(T0 component0) where T0 : struct, IEcsComponent
        {
            EcsEntity entity = InstantiateEntity();
            m_componentTypeIndices[0] = EcsComponentType<T0>.index;
            
            EcsArchetype archetype = m_archetypeManager.FindOrCreateArchetype(m_componentTypeIndices, 1);
            EcsArchetypeChunk* chunk = archetype.PushEntity(entity, out int indexInChunk);
            m_entityArchetypeInfo->Write(entity.Index, new EcsArchetypeInfo(archetype.archetypeIndex, indexInChunk, chunk));

            int index = archetype.componentsMap->Get(EcsComponentType<T0>.index);
            int offset = archetype.componentsOffset->Read<int>(index);
            chunk->WriteComponent(indexInChunk, offset, component0);
            
            return entity;
        }

        public EcsEntity CreateEntity<T0, T1>(T0 component0, T1 component1)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
        {
            m_componentTypeIndices[0] = EcsComponentType<T0>.index;
            m_componentTypeIndices[1] = EcsComponentType<T1>.index;
            Array.Sort(m_componentTypeIndices, 0, 2);

            if (!CheckIndices(m_componentTypeIndices, 2))
            {
                throw new ArgumentException(nameof(CreateEntity));
            }

            EcsEntity entity = InstantiateEntity();
            EcsArchetype archetype = m_archetypeManager.FindOrCreateArchetype(m_componentTypeIndices, 2);
            EcsArchetypeChunk* chunk = archetype.PushEntity(entity, out int indexInChunk);
            m_entityArchetypeInfo->Write(entity.Index, new EcsArchetypeInfo(archetype.archetypeIndex, indexInChunk, chunk));

            int index = archetype.componentsMap->Get(EcsComponentType<T0>.index);
            int offset = archetype.componentsOffset->Read<int>(index);
            chunk->WriteComponent(indexInChunk, offset, component0);
            
            index = archetype.componentsMap->Get(EcsComponentType<T1>.index);
            offset = archetype.componentsOffset->Read<int>(index);
            chunk->WriteComponent(indexInChunk, offset, component1);
            
            return entity;
        }

        public EcsEntity CreateEntity<T0, T1, T2>(T0 component0, T1 component1, T2 component2)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
        {
            m_componentTypeIndices[0] = EcsComponentType<T0>.index;
            m_componentTypeIndices[1] = EcsComponentType<T1>.index;
            m_componentTypeIndices[2] = EcsComponentType<T2>.index;
            Array.Sort(m_componentTypeIndices, 0, 3);

            if (!CheckIndices(m_componentTypeIndices, 3))
            {
                throw new ArgumentException(nameof(CreateEntity));
            }

            EcsEntity entity = InstantiateEntity();
            EcsArchetype archetype = m_archetypeManager.FindOrCreateArchetype(m_componentTypeIndices, 3);
            EcsArchetypeChunk* chunk = archetype.PushEntity(entity, out int indexInChunk);
            m_entityArchetypeInfo->Write(entity.Index, new EcsArchetypeInfo(archetype.archetypeIndex, indexInChunk, chunk));

            int index = archetype.componentsMap->Get(EcsComponentType<T0>.index);
            int offset = archetype.componentsOffset->Read<int>(index);
            chunk->WriteComponent(indexInChunk, offset, component0);
            
            index = archetype.componentsMap->Get(EcsComponentType<T1>.index);
            offset = archetype.componentsOffset->Read<int>(index);
            chunk->WriteComponent(indexInChunk, offset, component1);
            
            index = archetype.componentsMap->Get(EcsComponentType<T2>.index);
            offset = archetype.componentsOffset->Read<int>(index);
            chunk->WriteComponent(indexInChunk, offset, component2);

            return entity;
        }

        public EcsEntity CreateEntity<T0, T1, T2, T3>(T0 component0, T1 component1, T2 component2, T3 component3)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
        {
            m_componentTypeIndices[0] = EcsComponentType<T0>.index;
            m_componentTypeIndices[1] = EcsComponentType<T1>.index;
            m_componentTypeIndices[2] = EcsComponentType<T2>.index;
            m_componentTypeIndices[3] = EcsComponentType<T3>.index;
            Array.Sort(m_componentTypeIndices, 0, 4);

            if (!CheckIndices(m_componentTypeIndices, 4))
            {
                throw new ArgumentException(nameof(CreateEntity));
            }

            EcsEntity entity = InstantiateEntity();
            EcsArchetype archetype = m_archetypeManager.FindOrCreateArchetype(m_componentTypeIndices, 4);
            EcsArchetypeChunk* chunk = archetype.PushEntity(entity, out int indexInChunk);
            m_entityArchetypeInfo->Write(entity.Index, new EcsArchetypeInfo(archetype.archetypeIndex, indexInChunk, chunk));

            int index = archetype.componentsMap->Get(EcsComponentType<T0>.index);
            int offset = archetype.componentsOffset->Read<int>(index);
            chunk->WriteComponent(indexInChunk, offset, component0);
            
            index = archetype.componentsMap->Get(EcsComponentType<T1>.index);
            offset = archetype.componentsOffset->Read<int>(index);
            chunk->WriteComponent(indexInChunk, offset, component1);
            
            index = archetype.componentsMap->Get(EcsComponentType<T2>.index);
            offset = archetype.componentsOffset->Read<int>(index);
            chunk->WriteComponent(indexInChunk, offset, component2);
            
            index = archetype.componentsMap->Get(EcsComponentType<T3>.index);
            offset = archetype.componentsOffset->Read<int>(index);
            chunk->WriteComponent(indexInChunk, offset, component3);
            
            return entity;
        }

        private static bool CheckIndices(byte[] indices, int len)
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