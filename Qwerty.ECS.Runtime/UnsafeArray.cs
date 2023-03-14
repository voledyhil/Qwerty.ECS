using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
	public unsafe struct UnsafeArray
	{
		public int length => m_length;

		public byte* Ptr;
		private int m_capacityInBytes;
		private int m_length;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Realloc<T>(int newCapacity) where T : struct
		{
			int structSize = Unsafe.SizeOf<T>();
			int newCapacityInBytes = structSize * newCapacity;
			Ptr = Ptr != null
				? (byte*)MemoryUtilities.Realloc((IntPtr)Ptr, newCapacityInBytes, m_capacityInBytes)
				: (byte*)MemoryUtilities.Alloc(newCapacityInBytes, true);
			m_capacityInBytes = newCapacityInBytes;
			m_length = newCapacity;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (Ptr == null)
			{
				throw new Exception("UnsafeArray: try to dispose an already disposed array");
			}

			MemoryUtilities.Free((IntPtr)Ptr);
			Ptr = null;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>(int index) where T : struct
		{
			if (index < 0 || index >= m_length)
			{
				throw new IndexOutOfRangeException(nameof(Get));
			}
			return ref Unsafe.AsRef<T>(Unsafe.Add<T>(Ptr, index));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Read<T>(int index) where T : struct
		{
			if (index < 0 || index >= m_length)
			{
				throw new IndexOutOfRangeException(nameof(Read));
			}
			return Unsafe.Read<T>(Unsafe.Add<T>(Ptr, index));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write<T>(int index, in T value) where T : struct
		{
			if (index < 0 || index >= m_length)
			{
				throw new IndexOutOfRangeException(nameof(Write));
			}
			Unsafe.Write(Unsafe.Add<T>(Ptr, index), value);
		}
	}
}