
// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
	public readonly unsafe struct EcsArchetypeGroupAccessor
	{
		private readonly UnsafeArray* m_items;
		internal EcsArchetypeGroupAccessor(IReadOnlyList<EcsArchetype> archetypes)
		{
			m_items = (UnsafeArray*)MemoryUtil.Alloc<UnsafeArray>(1);
			m_items->Alloc<IntPtr>(archetypes.Count);

			for (int i = 0; i < archetypes.Count; i++)
			{
				m_items->Write(i, (IntPtr)archetypes[i].chunks);
			}
		}

		public EcsChunkEnumerator GetEnumerator() => new EcsChunkEnumerator(m_items);

		internal void Dispose()
		{
			m_items->Dispose();
		}
	}
}