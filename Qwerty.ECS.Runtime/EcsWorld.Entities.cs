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
                EcsEntity freeEntity = m_freeEntities->Read<EcsEntity>(--m_freeEntitiesLen);
                entityIndex = freeEntity.Index;
                entityVersion = freeEntity.Version;
            }
            else
            {
                entityIndex = ++m_entityCounter;
            }

            if (entityIndex >= m_entities->length)
            {
                int newCapacity = 2 * entityIndex + 1;
                m_entities->Realloc<EcsEntity>(newCapacity);
                m_freeEntities->Realloc<EcsEntity>(newCapacity);
                m_entitiesInfo->Realloc<EcsArchetypeChunkInfo>(newCapacity);
            }

            EcsEntity entity = new EcsEntity(entityIndex, entityVersion + 1);
            m_entities->Write(entityIndex, entity);

            return entity;
        }
        
        public void DestroyEntity(EcsEntity entity)
        {
            int entityIndex = entity.Index;
            if (m_entities->Read<EcsEntity>(entityIndex) == EcsEntity.Null)
            {
                throw new InvalidOperationException(nameof(entity));
            }

            EcsArchetypeChunkInfo info = m_entitiesInfo->Read<EcsArchetypeChunkInfo>(entity.Index);
            EcsArchetype archetype = m_archetypeManager[info.archetypeIndex];
            
            Swap(archetype, info);

            m_entities->Write(entityIndex, EcsEntity.Null);
            m_freeEntities->Write(m_freeEntitiesLen++, entity);
        }
        
        public EcsEntity CreateEntity()
        {
            EcsEntity entity = InstantiateEntity();
            EcsArchetype archetype = m_archetypeManager.empty;
            EcsArchetypeChunkInfo archetypeChunkInfo = archetype.PushEntity(entity);
            m_entitiesInfo->Write(entity.Index, archetypeChunkInfo);
            return entity;
        }

        public EcsEntity CreateEntity<T0>(T0 c0) where T0 : struct, IEcsComponent
        {
            EcsEntity entity = InstantiateEntity();
            m_componentTypeIndices[0] = EcsComponentType<T0>.index;
            
            EcsArchetype archetype = m_archetypeManager.FindOrCreateArchetype(m_componentTypeIndices, 1);
            EcsArchetypeChunkInfo archetypeChunkInfo = archetype.PushEntity(entity);
            m_entitiesInfo->Write(entity.Index, archetypeChunkInfo);

            EcsArchetypeComponentsMap map = *archetype.componentsMap;
            UnsafeArray offsets = *archetype.componentsOffset;
            EcsArchetypeChunk chunk = *archetypeChunkInfo.chunk;
            chunk.Write(archetypeChunkInfo.index, offsets.Read<int>(map.Get(EcsComponentType<T0>.index)), c0);
            
            return entity;
        }

        public EcsEntity CreateEntity<T0, T1>(T0 c0, T1 c1)
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
            EcsArchetypeChunkInfo archetypeChunkInfo = archetype.PushEntity(entity);
            m_entitiesInfo->Write(entity.Index, archetypeChunkInfo);

            EcsArchetypeComponentsMap map = *archetype.componentsMap;
            UnsafeArray offsets = *archetype.componentsOffset;
            EcsArchetypeChunk chunk = *archetypeChunkInfo.chunk;
            chunk.Write(archetypeChunkInfo.index, offsets.Read<int>(map.Get(EcsComponentType<T0>.index)), c0);
            chunk.Write(archetypeChunkInfo.index, offsets.Read<int>(map.Get(EcsComponentType<T1>.index)), c1);
            
            return entity;
        }

        public EcsEntity CreateEntity<T0, T1, T2>(T0 c0, T1 c1, T2 c2)
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
            EcsArchetypeChunkInfo archetypeChunkInfo = archetype.PushEntity(entity);
            m_entitiesInfo->Write(entity.Index, archetypeChunkInfo);

            EcsArchetypeComponentsMap map = *archetype.componentsMap;
            UnsafeArray offsets = *archetype.componentsOffset;
            EcsArchetypeChunk chunk = *archetypeChunkInfo.chunk;
            chunk.Write(archetypeChunkInfo.index, offsets.Read<int>(map.Get(EcsComponentType<T0>.index)), c0);
            chunk.Write(archetypeChunkInfo.index, offsets.Read<int>(map.Get(EcsComponentType<T1>.index)), c1);
            chunk.Write(archetypeChunkInfo.index, offsets.Read<int>(map.Get(EcsComponentType<T2>.index)), c2);

            return entity;
        }

        public EcsEntity CreateEntity<T0, T1, T2, T3>(T0 c0, T1 c1, T2 c2, T3 c3)
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
            EcsArchetypeChunkInfo archetypeChunkInfo = archetype.PushEntity(entity);
            m_entitiesInfo->Write(entity.Index, archetypeChunkInfo);

            EcsArchetypeComponentsMap map = *archetype.componentsMap;
            UnsafeArray offsets = *archetype.componentsOffset;
            EcsArchetypeChunk chunk = *archetypeChunkInfo.chunk;
            chunk.Write(archetypeChunkInfo.index, offsets.Read<int>(map.Get(EcsComponentType<T0>.index)), c0);
            chunk.Write(archetypeChunkInfo.index, offsets.Read<int>(map.Get(EcsComponentType<T1>.index)), c1);
            chunk.Write(archetypeChunkInfo.index, offsets.Read<int>(map.Get(EcsComponentType<T2>.index)), c2);
            chunk.Write(archetypeChunkInfo.index, offsets.Read<int>(map.Get(EcsComponentType<T3>.index)), c3);
            
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