using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Runtime
{
    public unsafe partial class EcsWorld
    {
        public bool HasComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            return HasComponent(entity, EcsComponentType<T>.index);
        }

        private bool HasComponent(EcsEntity entity, byte index)
        {
            EcsArchetypeChunkInfo curEcsArchetypeChunkInfo = m_entityArchetypeInfo->Read<EcsArchetypeChunkInfo>(entity.Index);
            int curArchetypeIndex = curEcsArchetypeChunkInfo.archetypeIndex;
            return m_archetypeManager[curArchetypeIndex].typeIndicesSet.Contains(index);
        }

        public T GetComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            if (!HasComponent<T>(entity))
            {
                throw new InvalidOperationException();
            }

            EcsArchetypeChunkInfo curEcsArchetypeChunkInfo = m_entityArchetypeInfo->Read<EcsArchetypeChunkInfo>(entity.Index);
            int indexInChunk = curEcsArchetypeChunkInfo.index;
            EcsArchetypeChunk* chunk = curEcsArchetypeChunkInfo.chunk;
            int archetypeIndex = curEcsArchetypeChunkInfo.archetypeIndex;
            
            EcsArchetype archetype = m_archetypeManager[archetypeIndex];
            
            int index = archetype.componentsMap->Get(EcsComponentType<T>.index);
            int offset = archetype.componentsOffset->Read<int>(index);
            return chunk->ReadComponent<T>(indexInChunk, offset);
        }
        
        public void SetComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsArchetypeChunkInfo curEcsArchetypeChunkInfo = m_entityArchetypeInfo->Read<EcsArchetypeChunkInfo>(entity.Index);
            int indexInChunk = curEcsArchetypeChunkInfo.index;
            EcsArchetypeChunk* chunk = curEcsArchetypeChunkInfo.chunk;
            int archetypeIndex = curEcsArchetypeChunkInfo.archetypeIndex;
            
            EcsArchetype archetype = m_archetypeManager[archetypeIndex];
            
            int index = archetype.componentsMap->Get(componentTypeIndex);
            int offset = archetype.componentsOffset->Read<int>(index);
            chunk->WriteComponent<T>(indexInChunk, offset, component);
        }

        public void RemoveComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }

            EcsArchetypeChunkInfo fromChunkInfo = m_entityArchetypeInfo->Read<EcsArchetypeChunkInfo>(entity.Index);
            EcsArchetype fromArchetype = m_archetypeManager[fromChunkInfo.archetypeIndex];

            EcsArchetype toArchetype = m_archetypeManager.FindOrCreatePriorArchetype(fromArchetype, componentTypeIndex);
            EcsArchetypeChunkInfo toChunkInfo = toArchetype.PushEntity(entity);
            
            Copy(fromArchetype, fromChunkInfo, toChunkInfo, componentTypeIndex);
            Swap(fromArchetype, fromChunkInfo);
            
            m_entityArchetypeInfo->Write(entity.Index, toChunkInfo);
        }
        
        public void AddComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsArchetypeChunkInfo fromChunkInfo = m_entityArchetypeInfo->Read<EcsArchetypeChunkInfo>(entity.Index);
            EcsArchetype fromArchetype = m_archetypeManager[fromChunkInfo.archetypeIndex];
            
            EcsArchetype toArchetype = m_archetypeManager.FindOrCreateNextArchetype(fromArchetype, componentTypeIndex);
            EcsArchetypeChunkInfo toChunkInfo = toArchetype.PushEntity(entity);
            
            int index = toArchetype.componentsMap->Get(componentTypeIndex);
            int offset = toArchetype.componentsOffset->Read<int>(index);
            toChunkInfo.chunk->WriteComponent(toChunkInfo.index, offset, component);
            
            Copy(fromChunkInfo, toArchetype, toChunkInfo, componentTypeIndex);
            Swap(fromArchetype, fromChunkInfo);

            m_entityArchetypeInfo->Write(entity.Index, toChunkInfo);
        }
    }
}