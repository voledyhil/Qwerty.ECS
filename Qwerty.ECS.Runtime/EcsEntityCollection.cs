using System.Runtime.CompilerServices;
using Qwerty.Unsafe.Runtime;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
	public readonly unsafe struct EcsEntityCollection
	{
		public int count => m_array->Count;
		internal int length => m_array->CapacityInBytes / MemoryUtilities.SizeOf<EcsEntity>();

		private readonly UnsafeArray* m_array;
		public EcsEntityCollection(int capacity)
		{
			m_array = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
			m_array->Realloc<EcsEntity>(capacity);
		}

		public EcsEntity this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => m_array->Read<EcsEntity>(index);
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal set => m_array->Write(index, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void Resize(int newCapacity)
		{
			m_array->Realloc<EcsEntity>(newCapacity);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void IncCount()
		{
			m_array->SetCount<EcsEntity>(count + 1);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void DecCount()
		{
			m_array->SetCount<EcsEntity>(count - 1);
		}

		internal void Dispose()
		{
			m_array->Dispose();
		}
	}
}