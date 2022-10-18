using System.Collections.Generic;
using Qwerty.Unsafe.DataStructures.Runtime;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
	public readonly struct EcsArchetypesAccessor
	{
		public int ArchetypeCount => m_items.Length;
		
		private readonly NativeArray<EcsEntityCollection> m_items;
		internal EcsArchetypesAccessor(IReadOnlyList<EcsArchetype> archetypes)
		{
			m_items = new NativeArray<EcsEntityCollection>(archetypes.Count);
			for (int i = 0; i < archetypes.Count; i++)
			{
				m_items[i] = archetypes[i].Entities;
			}
		}

		public EcsEntityCollection GetEntityArray(int index)
		{
			return m_items[index];
		}

		internal void Dispose()
		{
			m_items.Dispose();
		}
	}
}