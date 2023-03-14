
// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Components
{
	public readonly struct EcsComponentArrayAccessor<T> where T : struct, IEcsComponent
	{
		private readonly unsafe UnsafeArray* m_entities;
		internal unsafe EcsComponentArrayAccessor(UnsafeArray* entities)
		{
			m_entities = entities;
		}

		public unsafe T this[in EcsEntity entity]
		{
			get
			{
				EcsEntityInfo entityInfo = m_entities->Read<EcsEntityInfo>(entity.Index);
				return entityInfo.chunk->ReadComponent<T>(entityInfo.chunkIndex);
			}
			set
			{
				EcsEntityInfo entityInfo = m_entities->Read<EcsEntityInfo>(entity.Index);
				entityInfo.chunk->WriteComponent<T>(entityInfo.chunkIndex, value);	
			}
		}
	}
}