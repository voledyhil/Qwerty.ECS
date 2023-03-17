
// ReSharper disable once CheckNamespace

using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Archetypes;

namespace Qwerty.ECS.Runtime.Components
{
	public readonly struct EcsComponentDataFromEntity<T> where T : struct, IEcsComponent
	{
		private readonly unsafe UnsafeArray* m_entities;
		private readonly int m_typeIndex;
		internal unsafe EcsComponentDataFromEntity(UnsafeArray* entities)
		{
			m_entities = entities;
			m_typeIndex = EcsComponentType<T>.index;
		}

		public unsafe T this[in EcsEntity entity]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				EcsEntityInfo entityInfo = m_entities->Get<EcsEntityInfo>(entity.Index);
				EcsChunk* chunk = entityInfo.chunk;
				return chunk->ReadComponent<T>(entityInfo.indexInChunk, m_typeIndex);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				EcsEntityInfo entityInfo = m_entities->Get<EcsEntityInfo>(entity.Index);
				EcsChunk* chunk = entityInfo.chunk;
				chunk->WriteComponent<T>(entityInfo.indexInChunk, m_typeIndex, value);	
			}
		}
	}
}