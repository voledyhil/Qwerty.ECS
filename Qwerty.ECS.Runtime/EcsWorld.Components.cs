using System;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Chunks;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public partial class EcsWorld
    {
        public bool HasComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            return HasComponent(entity, EcsTypeIndex<T>.value.index);
        }

        private unsafe bool HasComponent(EcsEntity entity, int index)
        {
            EcsEntityInfo info = MemoryUtil.ReadElement<EcsEntityInfo>(m_entitiesInfo, entity.Index);
            return info.chunk->header->ContainType(index);
        }

        public unsafe T GetComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            if (!HasComponent<T>(entity))
            {
                throw new InvalidOperationException();
            }
            EcsEntityInfo info = MemoryUtil.ReadElement<EcsEntityInfo>(m_entitiesInfo, entity.Index);
            return info.chunk->ReadComponent<T>(info.indexInChunk, EcsTypeIndex<T>.value.index);
        }
        
        public unsafe void SetComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            int typeIndex = EcsTypeIndex<T>.value.index;
            if (!HasComponent(entity, typeIndex))
            {
                throw new InvalidOperationException();
            }

            EcsEntityInfo info = MemoryUtil.ReadElement<EcsEntityInfo>(m_entitiesInfo, entity.Index);
            info.chunk->WriteComponent<T>(info.indexInChunk, EcsTypeIndex<T>.value.index, component);
        }

        public void RemoveComponent<T>(EcsEntity entity) where T : struct, IEcsComponent
        {
            int typeIndex = EcsTypeIndex<T>.value.index;
            if (!HasComponent(entity, typeIndex))
            {
                throw new InvalidOperationException();
            }

            EcsEntityInfo fromInfo = MemoryUtil.ReadElement<EcsEntityInfo>(m_entitiesInfo, entity.Index);
            EcsArchetype fromArchetype = m_arcManager[fromInfo.archetypeIndex];
            EcsArchetype toArchetype = m_arcManager.FindOrCreatePriorArchetype(fromArchetype, typeIndex);
            
            PushEntity(toArchetype, entity, out EcsEntityInfo toInfo);
            CopyToPrior(fromInfo, toInfo, typeIndex);
            SwapRow(fromArchetype, fromInfo);
        }
        
        public unsafe void AddComponent<T>(EcsEntity entity, T component) where T : struct, IEcsComponent
        {
            int typeIndex = EcsTypeIndex<T>.value.index;
            if (HasComponent(entity, typeIndex))
            {
                throw new InvalidOperationException();
            }
            
            EcsEntityInfo fromInfo = MemoryUtil.ReadElement<EcsEntityInfo>(m_entitiesInfo, entity.Index);
            EcsArchetype fromArchetype = m_arcManager[fromInfo.archetypeIndex];
            
            EcsArchetype toArchetype = m_arcManager.FindOrCreateNextArchetype(fromArchetype, typeIndex);
            PushEntity(toArchetype, entity, out EcsEntityInfo toInfo);
            CopyToNext(fromInfo, toInfo, typeIndex);
            SwapRow(fromArchetype, fromInfo);
            
            toInfo.chunk->WriteComponent(toInfo.indexInChunk, typeIndex, component);
        }

        public EcsComponentDataFromEntity<T> GetComponentDataFromEntityAccessor<T>() where T : struct, IEcsComponent
        {
            return new EcsComponentDataFromEntity<T>(m_entitiesInfo, new EcsComponentTypeHandle<T>(EcsTypeIndex<T>.value.index));
        }
        
        public EcsComponentTypeHandle<T> GetComponentTypeHandle<T>() where T : struct, IEcsComponent
        {
            return new EcsComponentTypeHandle<T>(EcsTypeIndex<T>.value.index);
        }
        
        public unsafe EcsEntityWriter<T0, T1, T2> InstantiateEntityWriter<T0, T1, T2>(int capacity)  
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
                throw new ArgumentException(nameof(InstantiateEntityWriter));
            }
            
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 3);
            EcsChunkHeader* header = archetype.chunks->header;
            
            return new EcsEntityWriter<T0, T1, T2>(
                capacity, 
                header->rowSizeInBytes,
                header->ReadOffsetByType(typeIndex0),
                header->ReadOffsetByType(typeIndex1), 
                header->ReadOffsetByType(typeIndex2));
        }
        
        public unsafe EcsEntityWriter<T0, T1, T2, T3, T4> InstantiateEntityWriter<T0, T1, T2, T3, T4>(int capacity)  
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
            where T4 : struct, IEcsComponent
        {
            int typeIndex0 = EcsTypeIndex<T0>.value.index;
            int typeIndex1 = EcsTypeIndex<T1>.value.index;
            int typeIndex2 = EcsTypeIndex<T2>.value.index;
            int typeIndex3 = EcsTypeIndex<T3>.value.index;
            int typeIndex4 = EcsTypeIndex<T4>.value.index;
            
            m_indicesBuffer[0] = typeIndex0;
            m_indicesBuffer[1] = typeIndex1;
            m_indicesBuffer[2] = typeIndex2;
            m_indicesBuffer[3] = typeIndex3;
            m_indicesBuffer[4] = typeIndex4;
            
            Array.Sort(m_indicesBuffer, 0, 5);

            if (!ValidateIndices(m_indicesBuffer, 5))
            {
                throw new ArgumentException(nameof(InstantiateEntityWriter));
            }
            
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 5);
            EcsChunkHeader* header = archetype.chunks->header;
            
            return new EcsEntityWriter<T0, T1, T2, T3, T4>(
                capacity, 
                header->rowSizeInBytes,
                header->ReadOffsetByType(typeIndex0),
                header->ReadOffsetByType(typeIndex1), 
                header->ReadOffsetByType(typeIndex2), 
                header->ReadOffsetByType(typeIndex3), 
                header->ReadOffsetByType(typeIndex4));
        }
        
        public unsafe EcsEntityWriter<T0, T1, T2, T3, T4, T5> InstantiateEntityWriter<T0, T1, T2, T3, T4, T5>(int capacity)  
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
            where T4 : struct, IEcsComponent
            where T5 : struct, IEcsComponent
        {
            int typeIndex0 = EcsTypeIndex<T0>.value.index;
            int typeIndex1 = EcsTypeIndex<T1>.value.index;
            int typeIndex2 = EcsTypeIndex<T2>.value.index;
            int typeIndex3 = EcsTypeIndex<T3>.value.index;
            int typeIndex4 = EcsTypeIndex<T4>.value.index;
            int typeIndex5 = EcsTypeIndex<T5>.value.index;
            
            m_indicesBuffer[0] = typeIndex0;
            m_indicesBuffer[1] = typeIndex1;
            m_indicesBuffer[2] = typeIndex2;
            m_indicesBuffer[3] = typeIndex3;
            m_indicesBuffer[4] = typeIndex4;
            m_indicesBuffer[5] = typeIndex5;
            
            Array.Sort(m_indicesBuffer, 0, 6);

            if (!ValidateIndices(m_indicesBuffer, 6))
            {
                throw new ArgumentException(nameof(InstantiateEntityWriter));
            }
            
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 6);
            EcsChunkHeader* header = archetype.chunks->header;
            
            return new EcsEntityWriter<T0, T1, T2, T3, T4, T5>(
                capacity, 
                header->rowSizeInBytes,
                header->ReadOffsetByType(typeIndex0),
                header->ReadOffsetByType(typeIndex1), 
                header->ReadOffsetByType(typeIndex2), 
                header->ReadOffsetByType(typeIndex3), 
                header->ReadOffsetByType(typeIndex4), 
                header->ReadOffsetByType(typeIndex5));
        }
        
        public unsafe EcsEntityWriter<T0, T1, T2, T3, T4, T5, T6, T7> InstantiateEntityWriter<T0, T1, T2, T3, T4, T5, T6, T7>(int capacity)  
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
            where T4 : struct, IEcsComponent
            where T5 : struct, IEcsComponent
            where T6 : struct, IEcsComponent
            where T7 : struct, IEcsComponent
        {
            int typeIndex0 = EcsTypeIndex<T0>.value.index;
            int typeIndex1 = EcsTypeIndex<T1>.value.index;
            int typeIndex2 = EcsTypeIndex<T2>.value.index;
            int typeIndex3 = EcsTypeIndex<T3>.value.index;
            int typeIndex4 = EcsTypeIndex<T4>.value.index;
            int typeIndex5 = EcsTypeIndex<T5>.value.index;
            int typeIndex6 = EcsTypeIndex<T6>.value.index;
            int typeIndex7 = EcsTypeIndex<T7>.value.index;
            
            m_indicesBuffer[0] = typeIndex0;
            m_indicesBuffer[1] = typeIndex1;
            m_indicesBuffer[2] = typeIndex2;
            m_indicesBuffer[3] = typeIndex3;
            m_indicesBuffer[4] = typeIndex4;
            m_indicesBuffer[5] = typeIndex5;
            m_indicesBuffer[6] = typeIndex6;
            m_indicesBuffer[7] = typeIndex7;
            
            Array.Sort(m_indicesBuffer, 0, 8);

            if (!ValidateIndices(m_indicesBuffer, 8))
            {
                throw new ArgumentException(nameof(InstantiateEntityWriter));
            }
            
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 8);
            EcsChunkHeader* header = archetype.chunks->header;
            
            return new EcsEntityWriter<T0, T1, T2, T3, T4, T5, T6, T7>(
                capacity, 
                header->rowSizeInBytes,
                header->ReadOffsetByType(typeIndex0),
                header->ReadOffsetByType(typeIndex1), 
                header->ReadOffsetByType(typeIndex2), 
                header->ReadOffsetByType(typeIndex3), 
                header->ReadOffsetByType(typeIndex4), 
                header->ReadOffsetByType(typeIndex5), 
                header->ReadOffsetByType(typeIndex6), 
                header->ReadOffsetByType(typeIndex7));
        }
        
        public unsafe EcsEntityWriter<T0, T1, T2, T3, T4, T5, T6, T7, T8> InstantiateEntityWriter<T0, T1, T2, T3, T4, T5, T6, T7, T8>(int capacity)  
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
            where T4 : struct, IEcsComponent
            where T5 : struct, IEcsComponent
            where T6 : struct, IEcsComponent
            where T7 : struct, IEcsComponent
            where T8 : struct, IEcsComponent
        {
            int typeIndex0 = EcsTypeIndex<T0>.value.index;
            int typeIndex1 = EcsTypeIndex<T1>.value.index;
            int typeIndex2 = EcsTypeIndex<T2>.value.index;
            int typeIndex3 = EcsTypeIndex<T3>.value.index;
            int typeIndex4 = EcsTypeIndex<T4>.value.index;
            int typeIndex5 = EcsTypeIndex<T5>.value.index;
            int typeIndex6 = EcsTypeIndex<T6>.value.index;
            int typeIndex7 = EcsTypeIndex<T7>.value.index;
            int typeIndex8 = EcsTypeIndex<T8>.value.index;
            
            m_indicesBuffer[0] = typeIndex0;
            m_indicesBuffer[1] = typeIndex1;
            m_indicesBuffer[2] = typeIndex2;
            m_indicesBuffer[3] = typeIndex3;
            m_indicesBuffer[4] = typeIndex4;
            m_indicesBuffer[5] = typeIndex5;
            m_indicesBuffer[6] = typeIndex6;
            m_indicesBuffer[7] = typeIndex7;
            m_indicesBuffer[8] = typeIndex8;
            
            Array.Sort(m_indicesBuffer, 0, 9);

            if (!ValidateIndices(m_indicesBuffer, 9))
            {
                throw new ArgumentException(nameof(InstantiateEntityWriter));
            }
            
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 9);
            EcsChunkHeader* header = archetype.chunks->header;
            
            return new EcsEntityWriter<T0, T1, T2, T3, T4, T5, T6, T7, T8>(
                capacity, 
                header->rowSizeInBytes,
                header->ReadOffsetByType(typeIndex0),
                header->ReadOffsetByType(typeIndex1), 
                header->ReadOffsetByType(typeIndex2), 
                header->ReadOffsetByType(typeIndex3), 
                header->ReadOffsetByType(typeIndex4), 
                header->ReadOffsetByType(typeIndex5), 
                header->ReadOffsetByType(typeIndex6), 
                header->ReadOffsetByType(typeIndex7),
                header->ReadOffsetByType(typeIndex8));
        }
        
        public unsafe void CreateEntities<T0, T1, T2>(EcsEntityWriter<T0, T1, T2> writer)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
        {
            m_indicesBuffer[0] = EcsTypeIndex<T0>.value.index;
            m_indicesBuffer[1] = EcsTypeIndex<T1>.value.index;
            m_indicesBuffer[2] = EcsTypeIndex<T2>.value.index;
            
            Array.Sort(m_indicesBuffer, 0, 3);
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 3);
            
            for (int index = 0; index < writer.length; index++)
            {
                PushEntity(archetype, InstantiateEntity(), out EcsEntityInfo info);
                writer.CopyRow(index, (*info.chunk).body, info.indexInChunk);
            }
        }
        
        public unsafe void CreateEntities<T0, T1, T2, T3, T4>(EcsEntityWriter<T0, T1, T2, T3, T4> writer)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
            where T4 : struct, IEcsComponent
        {
            m_indicesBuffer[0] = EcsTypeIndex<T0>.value.index;
            m_indicesBuffer[1] = EcsTypeIndex<T1>.value.index;
            m_indicesBuffer[2] = EcsTypeIndex<T2>.value.index;
            m_indicesBuffer[3] = EcsTypeIndex<T3>.value.index;
            m_indicesBuffer[4] = EcsTypeIndex<T4>.value.index;
            
            Array.Sort(m_indicesBuffer, 0, 5);
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 5);
            
            for (int index = 0; index < writer.length; index++)
            {
                PushEntity(archetype, InstantiateEntity(), out EcsEntityInfo info);
                writer.CopyRow(index, (*info.chunk).body, info.indexInChunk);
            }
        }
        
        public unsafe void CreateEntities<T0, T1, T2, T3, T4, T5>(EcsEntityWriter<T0, T1, T2, T3, T4, T5> writer)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
            where T4 : struct, IEcsComponent
            where T5 : struct, IEcsComponent
        {
            m_indicesBuffer[0] = EcsTypeIndex<T0>.value.index;
            m_indicesBuffer[1] = EcsTypeIndex<T1>.value.index;
            m_indicesBuffer[2] = EcsTypeIndex<T2>.value.index;
            m_indicesBuffer[3] = EcsTypeIndex<T3>.value.index;
            m_indicesBuffer[4] = EcsTypeIndex<T4>.value.index;
            m_indicesBuffer[5] = EcsTypeIndex<T5>.value.index;
            
            Array.Sort(m_indicesBuffer, 0, 6);
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 6);
            
            for (int index = 0; index < writer.length; index++)
            {
                PushEntity(archetype, InstantiateEntity(), out EcsEntityInfo info);
                writer.CopyRow(index, (*info.chunk).body, info.indexInChunk);
            }
        }
        
        public unsafe void CreateEntities<T0, T1, T2, T3, T4, T5, T6, T7>(EcsEntityWriter<T0, T1, T2, T3, T4, T5, T6, T7> writer)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
            where T4 : struct, IEcsComponent
            where T5 : struct, IEcsComponent
            where T6 : struct, IEcsComponent
            where T7 : struct, IEcsComponent
        {
            m_indicesBuffer[0] = EcsTypeIndex<T0>.value.index;
            m_indicesBuffer[1] = EcsTypeIndex<T1>.value.index;
            m_indicesBuffer[2] = EcsTypeIndex<T2>.value.index;
            m_indicesBuffer[3] = EcsTypeIndex<T3>.value.index;
            m_indicesBuffer[4] = EcsTypeIndex<T4>.value.index;
            m_indicesBuffer[5] = EcsTypeIndex<T5>.value.index;
            m_indicesBuffer[6] = EcsTypeIndex<T6>.value.index;
            m_indicesBuffer[7] = EcsTypeIndex<T7>.value.index;
            
            Array.Sort(m_indicesBuffer, 0, 8);
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 8);
            
            for (int index = 0; index < writer.length; index++)
            {
                PushEntity(archetype, InstantiateEntity(), out EcsEntityInfo info);
                writer.CopyRow(index, (*info.chunk).body, info.indexInChunk);
            }
        }
        
        public unsafe void CreateEntities<T0, T1, T2, T3, T4, T5, T6, T7, T8>(EcsEntityWriter<T0, T1, T2, T3, T4, T5, T6, T7, T8> writer)
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
            where T4 : struct, IEcsComponent
            where T5 : struct, IEcsComponent
            where T6 : struct, IEcsComponent
            where T7 : struct, IEcsComponent
            where T8 : struct, IEcsComponent
        {
            m_indicesBuffer[0] = EcsTypeIndex<T0>.value.index;
            m_indicesBuffer[1] = EcsTypeIndex<T1>.value.index;
            m_indicesBuffer[2] = EcsTypeIndex<T2>.value.index;
            m_indicesBuffer[3] = EcsTypeIndex<T3>.value.index;
            m_indicesBuffer[4] = EcsTypeIndex<T4>.value.index;
            m_indicesBuffer[5] = EcsTypeIndex<T5>.value.index;
            m_indicesBuffer[6] = EcsTypeIndex<T6>.value.index;
            m_indicesBuffer[7] = EcsTypeIndex<T7>.value.index;
            m_indicesBuffer[8] = EcsTypeIndex<T8>.value.index;
            
            Array.Sort(m_indicesBuffer, 0, 9);
            EcsArchetype archetype = m_arcManager.FindOrCreateArchetype(m_indicesBuffer, 9);
            
            for (int index = 0; index < writer.length; index++)
            {
                PushEntity(archetype, InstantiateEntity(), out EcsEntityInfo info);
                writer.CopyRow(index, (*info.chunk).body, info.indexInChunk);
            }
        }
    }
}