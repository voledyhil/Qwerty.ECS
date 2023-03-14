
// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
	public readonly unsafe struct EcsArchetypeAccessor
	{
		private readonly UnsafeArray* m_items;
		internal EcsArchetypeAccessor(IReadOnlyList<EcsArchetype> archetypes)
		{
			m_items = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
			m_items->Realloc<IntPtr>(archetypes.Count);

			for (int i = 0; i < archetypes.Count; i++)
			{
				m_items->Write(i, (IntPtr)archetypes[i].chunks);
			}
		}

		public EcsArchetypeChunkEnumerator GetEnumerator() => new EcsArchetypeChunkEnumerator(m_items);

		internal void Dispose()
		{
			m_items->Dispose();
		}
	}
}