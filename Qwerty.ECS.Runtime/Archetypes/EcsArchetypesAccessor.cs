using Qwerty.Unsafe.Runtime;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
	public readonly unsafe struct EcsArchetypesAccessor
	{
		public int archetypeCount => m_items->Count;
		
		private readonly UnsafeArray* m_items;
		internal EcsArchetypesAccessor(IReadOnlyList<EcsArchetype> archetypes)
		{
			m_items = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
			m_items->Realloc<EcsEntityCollection>(archetypes.Count);
			m_items->SetCount<EcsEntityCollection>(archetypes.Count);
			
			for (int i = 0; i < archetypes.Count; i++)
			{
				m_items->Write(i, archetypes[i].entities);
			}
		}

		public EcsEntityCollection GetEntityArray(int index)
		{
			return m_items->Read<EcsEntityCollection>(index);
		}

		internal void Dispose()
		{
			m_items->Dispose();
		}
	}
}