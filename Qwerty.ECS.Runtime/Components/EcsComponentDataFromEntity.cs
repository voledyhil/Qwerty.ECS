using System;
using Qwerty.ECS.Runtime.Chunks;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Components
{
#if UNITY_EDITOR
    [Unity.Burst.BurstCompile]
#endif
    public readonly struct EcsComponentDataFromEntity<T> where T : struct, IEcsComponent
    {
        private readonly IntPtr m_entities;
        private readonly int m_typeIndex;
        private readonly int m_sizeOfEntityInfo;

        internal EcsComponentDataFromEntity(IntPtr entities, EcsComponentTypeHandle<T> typeHandle)
        {
            m_entities = entities;
            m_typeIndex = typeHandle.typeIndex;
            m_sizeOfEntityInfo = MemoryUtil.SizeOf<EcsEntityInfo>();
        }

        public unsafe T this[in EcsEntity entity]
        {
            get
            {
                EcsEntityInfo entityInfo = MemoryUtil.Read<EcsEntityInfo>(m_entities, entity.Index * m_sizeOfEntityInfo);
                EcsChunk* chunk = entityInfo.chunk;
                return chunk->ReadComponent<T>(entityInfo.indexInChunk, m_typeIndex);
            }
            set
            {
                EcsEntityInfo entityInfo = MemoryUtil.Read<EcsEntityInfo>(m_entities, entity.Index * m_sizeOfEntityInfo);
                EcsChunk* chunk = entityInfo.chunk;
                chunk->WriteComponent<T>(entityInfo.indexInChunk, m_typeIndex, value);
            }
        }
    }
}