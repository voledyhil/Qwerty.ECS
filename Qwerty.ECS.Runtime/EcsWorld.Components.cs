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

        private unsafe bool HasComponent(EcsEntity entity, byte index)
        {
            EcsArchetypeChunkInfo chunkInfo = m_entitiesInfo->Read<EcsArchetypeChunkInfo>(entity.Index);
            return m_archetypeManager[chunkInfo.archetypeIndex].componentsMap->Contains(index);
        }

        public unsafe T GetComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            if (!HasComponent<T>(entity))
            {
                throw new InvalidOperationException();
            }

            EcsArchetypeChunkInfo chunkInfo = m_entitiesInfo->Read<EcsArchetypeChunkInfo>(entity.Index);
            EcsArchetype archetype = m_archetypeManager[chunkInfo.archetypeIndex];
            
            int index = archetype.componentsMap->Get(EcsComponentType<T>.index);
            int offset = archetype.componentsOffset->Read<int>(index);
            return chunkInfo.chunk->Read<T>(chunkInfo.index, offset);
        }
        
        public unsafe void SetComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsArchetypeChunkInfo chunkInfo = m_entitiesInfo->Read<EcsArchetypeChunkInfo>(entity.Index);
            EcsArchetype archetype = m_archetypeManager[chunkInfo.archetypeIndex];
            
            int index = archetype.componentsMap->Get(componentTypeIndex);
            int offset = archetype.componentsOffset->Read<int>(index);
            chunkInfo.chunk->Write<T>(chunkInfo.index, offset, component);
        }

        public unsafe void RemoveComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }

            EcsArchetypeChunkInfo fromChunkInfo = m_entitiesInfo->Read<EcsArchetypeChunkInfo>(entity.Index);
            
            EcsArchetype fromArchetype = m_archetypeManager[fromChunkInfo.archetypeIndex];
            EcsArchetype toArchetype = m_archetypeManager.FindOrCreatePriorArchetype(fromArchetype, componentTypeIndex);
            
            PushEntity(toArchetype, entity, out EcsArchetypeChunkInfo toChunkInfo);
            CopyEntity(fromArchetype, fromChunkInfo, toChunkInfo, componentTypeIndex);
            SwapEntity(fromArchetype, fromChunkInfo);
        }
        
        public unsafe void AddComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsArchetypeChunkInfo fromChunkInfo = m_entitiesInfo->Read<EcsArchetypeChunkInfo>(entity.Index);
            EcsArchetype fromArchetype = m_archetypeManager[fromChunkInfo.archetypeIndex];
            
            EcsArchetype toArchetype = m_archetypeManager.FindOrCreateNextArchetype(fromArchetype, componentTypeIndex);
            PushEntity(toArchetype, entity, out EcsArchetypeChunkInfo toChunkInfo);
            CopyEntity(fromChunkInfo, toArchetype, toChunkInfo, componentTypeIndex);
            SwapEntity(fromArchetype, fromChunkInfo);
            
            int index = toArchetype.componentsMap->Get(componentTypeIndex);
            int offset = toArchetype.componentsOffset->Read<int>(index);
            toChunkInfo.chunk->Write(toChunkInfo.index, offset, component);
        }
    }
}