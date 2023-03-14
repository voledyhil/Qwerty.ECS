// ReSharper disable once CheckNamespace

using System.ComponentModel;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime
{
    public partial class EcsWorld
    {
        private unsafe EcsEntity InstantiateEntity()
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
                throw new InvalidEnumArgumentException(nameof(InstantiateEntity));
            }

            EcsEntity entity = new EcsEntity(entityIndex, entityVersion + 1);
            m_entities->Write(entityIndex, entity);

            return entity;
        }
        
        public unsafe void DestroyEntity(EcsEntity entity)
        {
            int entityIndex = entity.Index;
            if (m_entities->Read<EcsEntity>(entityIndex) == EcsEntity.Null)
            {
                throw new InvalidOperationException(nameof(entity));
            }

            EcsEntityInfo info = m_entitiesInfo->Read<EcsEntityInfo>(entity.Index);
            EcsArchetype archetype = m_archetypeManager[info.archetypeIndex];
            
            SwapRow(archetype, info);

            m_entities->Write(entityIndex, EcsEntity.Null);
            m_freeEntities->Write(m_freeEntitiesLen++, entity);
        }
        
        public EcsEntity CreateEntity()
        {
            
            EcsEntity entity = InstantiateEntity();
            EcsArchetype archetype = m_archetypeManager.empty;
            PushEntity(archetype, entity, out _);
            return entity;
        }

        public unsafe EcsEntity CreateEntity<T0>(T0 c0) where T0 : struct, IEcsComponent
        {
            EcsEntity entity = InstantiateEntity();
            m_indicesBuffer[0] = EcsComponentType<T0>.index;
            
            EcsArchetype archetype = m_archetypeManager.FindOrCreateArchetype(m_indicesBuffer, 1);
            PushEntity(archetype, entity, out EcsEntityInfo info);

            EcsChunk chunk = *info.chunk;
            chunk.WriteComponent(info.chunkIndex, c0);
            
            return entity;
        }

        public unsafe EcsEntity CreateEntity<T0, T1>(T0 c0, T1 c1)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
        {
            m_indicesBuffer[0] = EcsComponentType<T0>.index;
            m_indicesBuffer[1] = EcsComponentType<T1>.index;
            Array.Sort(m_indicesBuffer, 0, 2);

            if (!ValidateIndices(m_indicesBuffer, 2))
            {
                throw new ArgumentException(nameof(CreateEntity));
            }

            EcsEntity entity = InstantiateEntity();
            EcsArchetype archetype = m_archetypeManager.FindOrCreateArchetype(m_indicesBuffer, 2);
            PushEntity(archetype, entity, out EcsEntityInfo info);
            
            EcsChunk chunk = *info.chunk;
            chunk.WriteComponent(info.chunkIndex, c0);
            chunk.WriteComponent(info.chunkIndex, c1);
            
            return entity;
        }

        public unsafe EcsEntity CreateEntity<T0, T1, T2>(T0 c0, T1 c1, T2 c2)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
        {
            m_indicesBuffer[0] = EcsComponentType<T0>.index;
            m_indicesBuffer[1] = EcsComponentType<T1>.index;
            m_indicesBuffer[2] = EcsComponentType<T2>.index;
            Array.Sort(m_indicesBuffer, 0, 3);

            if (!ValidateIndices(m_indicesBuffer, 3))
            {
                throw new ArgumentException(nameof(CreateEntity));
            }

            EcsEntity entity = InstantiateEntity();
            EcsArchetype archetype = m_archetypeManager.FindOrCreateArchetype(m_indicesBuffer, 3);
            PushEntity(archetype, entity, out EcsEntityInfo info);
            
            EcsChunk chunk = *info.chunk;
            chunk.WriteComponent(info.chunkIndex, c0);
            chunk.WriteComponent(info.chunkIndex, c1);
            chunk.WriteComponent(info.chunkIndex, c2);

            return entity;
        }

        public unsafe 
            
            EcsEntity CreateEntity<T0, T1, T2, T3>(T0 c0, T1 c1, T2 c2, T3 c3)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
        {
            m_indicesBuffer[0] = EcsComponentType<T0>.index;
            m_indicesBuffer[1] = EcsComponentType<T1>.index;
            m_indicesBuffer[2] = EcsComponentType<T2>.index;
            m_indicesBuffer[3] = EcsComponentType<T3>.index;
            Array.Sort(m_indicesBuffer, 0, 4);

            if (!ValidateIndices(m_indicesBuffer, 4))
            {
                throw new ArgumentException(nameof(CreateEntity));
            }

            EcsEntity entity = InstantiateEntity();
            EcsArchetype archetype = m_archetypeManager.FindOrCreateArchetype(m_indicesBuffer, 4);
            PushEntity(archetype, entity, out EcsEntityInfo info);
            
            EcsChunk chunk = *info.chunk;
            chunk.WriteComponent(info.chunkIndex, c0);
            chunk.WriteComponent(info.chunkIndex, c1);
            chunk.WriteComponent(info.chunkIndex, c2);
            chunk.WriteComponent(info.chunkIndex, c3);
            
            return entity;
        }

        private static bool ValidateIndices(byte[] indices, int len)
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