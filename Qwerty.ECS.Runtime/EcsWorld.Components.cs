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
            return chunkInfo.chunk->ReadComponent<T>(chunkInfo.index);
        }
        
        public unsafe void SetComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsArchetypeChunkInfo chunkInfo = m_entitiesInfo->Read<EcsArchetypeChunkInfo>(entity.Index);
            chunkInfo.chunk->WriteComponent<T>(chunkInfo.index, component);
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
            CopyRow(fromArchetype, fromChunkInfo, toChunkInfo, componentTypeIndex);
            SwapRow(fromArchetype, fromChunkInfo);
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
            CopyRow(fromChunkInfo, toArchetype, toChunkInfo, componentTypeIndex);
            SwapRow(fromArchetype, fromChunkInfo);
            
            toChunkInfo.chunk->WriteComponent(toChunkInfo.index, component);
        }
    }
}