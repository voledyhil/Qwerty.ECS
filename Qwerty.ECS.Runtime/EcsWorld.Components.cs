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
            EcsArchetypeInfo curEcsArchetypeInfo = m_entityArchetypeInfo->Read<EcsArchetypeInfo>(entity.Index);
            int curArchetypeIndex = curEcsArchetypeInfo.archetypeIndex;
            return m_archetypeManager[curArchetypeIndex].typeIndicesSet.Contains(index);
        }

        public T GetComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            if (!HasComponent<T>(entity))
            {
                throw new InvalidOperationException();
            }

            EcsArchetypeInfo curEcsArchetypeInfo = m_entityArchetypeInfo->Read<EcsArchetypeInfo>(entity.Index);
            int indexInChunk = curEcsArchetypeInfo.indexInChunk;
            EcsArchetypeChunk* chunk = curEcsArchetypeInfo.chunk;
            int archetypeIndex = curEcsArchetypeInfo.archetypeIndex;
            
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
            
            EcsArchetypeInfo curEcsArchetypeInfo = m_entityArchetypeInfo->Read<EcsArchetypeInfo>(entity.Index);
            int indexInChunk = curEcsArchetypeInfo.indexInChunk;
            EcsArchetypeChunk* chunk = curEcsArchetypeInfo.chunk;
            int archetypeIndex = curEcsArchetypeInfo.archetypeIndex;
            
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

            EcsArchetypeInfo curEcsArchetypeInfo = m_entityArchetypeInfo->Read<EcsArchetypeInfo>(entity.Index);
            int curIndexInChunk = curEcsArchetypeInfo.indexInChunk;
            EcsArchetypeChunk* curChunk = curEcsArchetypeInfo.chunk;
            int curArchetypeIndex = curEcsArchetypeInfo.archetypeIndex;
            EcsArchetype currentArchetype = m_archetypeManager[curArchetypeIndex];

            EcsArchetype newArchetype = m_archetypeManager.FindOrCreatePriorArchetype(currentArchetype, componentTypeIndex);
            EcsArchetypeChunk* newChunk = newArchetype.PushEntity(entity, out int newIndexInChunk);
            
            EcsArchetype.CopyRemove(curIndexInChunk, curChunk, currentArchetype, newIndexInChunk, newChunk, componentTypeIndex);

            currentArchetype.Swap(curIndexInChunk, curChunk, m_entityArchetypeInfo);
            m_entityArchetypeInfo->Write(entity.Index, new EcsArchetypeInfo(newArchetype.archetypeIndex, newIndexInChunk, newChunk));
        }
        
        public void AddComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsArchetypeInfo curEcsArchetypeInfo = m_entityArchetypeInfo->Read<EcsArchetypeInfo>(entity.Index);
            int curIndexInChunk = curEcsArchetypeInfo.indexInChunk;
            EcsArchetypeChunk* currentChunk = curEcsArchetypeInfo.chunk;
            int curArchetypeIndex = curEcsArchetypeInfo.archetypeIndex;
            
            
            EcsArchetype currentArchetype = m_archetypeManager[curArchetypeIndex];
            
            EcsArchetype newArchetype = m_archetypeManager.FindOrCreateNextArchetype(currentArchetype, componentTypeIndex);
            EcsArchetypeChunk* newChunk = newArchetype.PushEntity(entity, out int newIndexInChunk);
            
            int index = newArchetype.componentsMap->Get(componentTypeIndex);
            int offset = newArchetype.componentsOffset->Read<int>(index);
            newChunk->WriteComponent(newIndexInChunk, offset, component);
            
            EcsArchetype.CopyAdd(curIndexInChunk, currentChunk, newIndexInChunk, newChunk, newArchetype, componentTypeIndex);
            currentArchetype.Swap(curIndexInChunk, currentChunk, m_entityArchetypeInfo);

            m_entityArchetypeInfo->Write(entity.Index, new EcsArchetypeInfo(newArchetype.archetypeIndex, newIndexInChunk, newChunk));
        }
    }
}