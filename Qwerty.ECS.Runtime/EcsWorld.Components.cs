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
            int archetypeIndex = m_entityToArchetype->Read<int>(entity.Index);
            return m_archetypeManager[archetypeIndex].typeIndicesSet.Contains(index);
        }

        public T GetComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            if (!HasComponent<T>(entity))
            {
                throw new InvalidOperationException();
            }

            int indexInArchetype = m_entityInArchetype->Read<int>(entity.Index);
            int archetypeIndex = m_entityToArchetype->Read<int>(entity.Index);
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
            
            int indexInArchetype = m_entityInArchetype->Read<int>(entity.Index);
            int archetypeIndex = m_entityToArchetype->Read<int>(entity.Index);
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
            
            int curIndexInArchetype = m_entityInArchetype->Read<int>(entity.Index);
            int curArchetypeIndex = m_entityToArchetype->Read<int>(entity.Index);
            EcsArchetype currentArchetype = m_archetypeManager[curArchetypeIndex];

            EcsArchetype newArchetype = m_archetypeManager.FindOrCreatePriorArchetype(currentArchetype, componentTypeIndex);
            newArchetype.CreateNextChunkIfNeed();
            int newIndexInArchetype = newArchetype.PushEntity(entity);
            
            int index = currentArchetype.componentsMap->Get(componentTypeIndex);
            EcsArchetype.CopyRemove(curIndexInArchetype, currentArchetype, newIndexInArchetype, newArchetype, index);

            if (currentArchetype.TrySwapEntity(curIndexInArchetype, out int swapEntityIndex, out int lastIndex))
            {
                EcsArchetype.CopySwap(lastIndex, curIndexInArchetype, currentArchetype);
                m_entityInArchetype->Write(swapEntityIndex, curIndexInArchetype);
            }
            currentArchetype.PopLastEntity();
            currentArchetype.DestroyLastChunkIfNeed();
            
            m_entityInArchetype->Write(entity.Index, newIndexInArchetype);
            m_entityToArchetype->Write(entity.Index, newArchetype.archetypeIndex);
        }
        
        public void AddComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            byte componentTypeIndex = EcsComponentType<T>.index;
            if (HasComponent(entity, componentTypeIndex))
            {
                throw new InvalidOperationException();
            }
            
            int curIndexInArchetype = m_entityInArchetype->Read<int>(entity.Index);
            int curArchetypeIndex = m_entityToArchetype->Read<int>(entity.Index);
            EcsArchetype currentArchetype = m_archetypeManager[curArchetypeIndex];
            
            EcsArchetype newArchetype = m_archetypeManager.FindOrCreateNextArchetype(currentArchetype, componentTypeIndex);
            newArchetype.CreateNextChunkIfNeed();
            int newIndexInArchetype = newArchetype.PushEntity(entity);
            
            int index = newArchetype.componentsMap->Get(componentTypeIndex);
            int offset = newArchetype.componentsOffset->Read<int>(index);
            
            EcsArchetypeChunk* chunk = newArchetype.GetChunkByIndex(newIndexInArchetype / newArchetype.chunkCapacity);
            chunk->WriteComponent(newIndexInArchetype % newArchetype.chunkCapacity, offset, component);
            EcsArchetype.CopyAdd(curIndexInArchetype, currentArchetype, newIndexInArchetype, newArchetype, index);

            if (currentArchetype.TrySwapEntity(curIndexInArchetype, out int swapEntityIndex, out int lastIndex))
            {
                EcsArchetype.CopySwap(lastIndex, curIndexInArchetype, currentArchetype);
                m_entityInArchetype->Write(swapEntityIndex, curIndexInArchetype);
            }
            currentArchetype.PopLastEntity();
            currentArchetype.DestroyLastChunkIfNeed();
            
            m_entityInArchetype->Write(entity.Index, newIndexInArchetype);
            m_entityToArchetype->Write(entity.Index, newArchetype.archetypeIndex);
        }
    }
}