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
            int indexInArchetype = curEcsArchetypeInfo.indexInArchetype;
            int archetypeIndex = curEcsArchetypeInfo.archetypeIndex;
            
            EcsArchetype archetype = m_archetypeManager[archetypeIndex];
            EcsArchetypeChunk* chunk = archetype.GetChunkByIndex(indexInArchetype / archetype.chunkCapacity);
            
            int index = archetype.componentsMap->Get(EcsComponentType<T>.index);
            int offset = archetype.componentsOffset->Read<int>(index);
            return chunk->ReadComponent<T>(indexInArchetype % archetype.chunkCapacity, offset);
        }
        
        public void SetComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsArchetypeInfo curEcsArchetypeInfo = m_entityArchetypeInfo->Read<EcsArchetypeInfo>(entity.Index);
            int indexInArchetype = curEcsArchetypeInfo.indexInArchetype;
            int archetypeIndex = curEcsArchetypeInfo.archetypeIndex;
            
            EcsArchetype archetype = m_archetypeManager[archetypeIndex];
            EcsArchetypeChunk* chunk = archetype.GetChunkByIndex(indexInArchetype / archetype.chunkCapacity);
            
            int index = archetype.componentsMap->Get(componentTypeIndex);
            int offset = archetype.componentsOffset->Read<int>(index);
            chunk->WriteComponent<T>(indexInArchetype % archetype.chunkCapacity, offset, component);
        }

        public void RemoveComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (!HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }

            EcsArchetypeInfo curEcsArchetypeInfo = m_entityArchetypeInfo->Read<EcsArchetypeInfo>(entity.Index);
            int curIndexInArchetype = curEcsArchetypeInfo.indexInArchetype;
            int curArchetypeIndex = curEcsArchetypeInfo.archetypeIndex;
            EcsArchetype currentArchetype = m_archetypeManager[curArchetypeIndex];

            EcsArchetype newArchetype = m_archetypeManager.FindOrCreatePriorArchetype(currentArchetype, componentTypeIndex);
            int newIndexInArchetype = newArchetype.PushEntity(entity);
            
            int index = currentArchetype.componentsMap->Get(componentTypeIndex);
            EcsArchetype.CopyRemove(curIndexInArchetype, currentArchetype, newIndexInArchetype, newArchetype, index);

            if (currentArchetype.TrySwapEntity(curIndexInArchetype, out int swapEntityIndex))
            {
                m_entityArchetypeInfo->Write(swapEntityIndex, curEcsArchetypeInfo);
            }
            currentArchetype.PopLastEntity();
            
            m_entityArchetypeInfo->Write(entity.Index, new EcsArchetypeInfo {archetypeIndex = newArchetype.archetypeIndex, indexInArchetype = newIndexInArchetype});
        }
        
        public void AddComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsArchetypeInfo curEcsArchetypeInfo = m_entityArchetypeInfo->Read<EcsArchetypeInfo>(entity.Index);
            int curIndexInArchetype = curEcsArchetypeInfo.indexInArchetype;
            int curArchetypeIndex = curEcsArchetypeInfo.archetypeIndex;
            EcsArchetype currentArchetype = m_archetypeManager[curArchetypeIndex];
            
            EcsArchetype newArchetype = m_archetypeManager.FindOrCreateNextArchetype(currentArchetype, componentTypeIndex);
            int newIndexInArchetype = newArchetype.PushEntity(entity);
            
            int index = newArchetype.componentsMap->Get(componentTypeIndex);
            int offset = newArchetype.componentsOffset->Read<int>(index);
            
            EcsArchetypeChunk* chunk = newArchetype.GetChunkByIndex(newIndexInArchetype / newArchetype.chunkCapacity);
            chunk->WriteComponent(newIndexInArchetype % newArchetype.chunkCapacity, offset, component);
            EcsArchetype.CopyAdd(curIndexInArchetype, currentArchetype, newIndexInArchetype, newArchetype, index);

            if (currentArchetype.TrySwapEntity(curIndexInArchetype, out int swapEntityIndex))
            {
                m_entityArchetypeInfo->Write(swapEntityIndex, curEcsArchetypeInfo);
            }
            currentArchetype.PopLastEntity();
            
            m_entityArchetypeInfo->Write(entity.Index, new EcsArchetypeInfo {archetypeIndex = newArchetype.archetypeIndex, indexInArchetype = newIndexInArchetype});
        }
    }
}