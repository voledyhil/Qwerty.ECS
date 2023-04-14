using System;
using Qwerty.ECS.Runtime.Chunks;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Components
{
    public readonly struct EcsComponentDataFromEntity<T> where T : struct, IEcsComponent
    {
        private readonly IntPtr m_entities;
        private readonly int m_typeIndex;

        internal EcsComponentDataFromEntity(IntPtr entities, EcsComponentTypeHandle<T> typeHandle)
        {
            m_entities = entities;
            m_typeIndex = typeHandle.typeIndex;
        }

        public unsafe T this[in EcsEntity entity]
        {
            get
            {
                EcsEntityInfo entityInfo = MemoryUtil.ReadElement<EcsEntityInfo>(m_entities, entity.Index);
                EcsChunk* chunk = entityInfo.chunk;
                return chunk->ReadComponent<T>(entityInfo.indexInChunk, m_typeIndex);
            }
            set
            {
                EcsEntityInfo entityInfo = MemoryUtil.ReadElement<EcsEntityInfo>(m_entities, entity.Index);
                EcsChunk* chunk = entityInfo.chunk;
                chunk->WriteComponent<T>(entityInfo.indexInChunk, m_typeIndex, value);
            }
        }
    }
}