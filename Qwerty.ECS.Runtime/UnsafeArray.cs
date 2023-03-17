using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
	public unsafe struct UnsafeArray
	{
		public int length => m_length;

		private byte* m_ptr;
		private int m_length;
		
		public void Alloc<T>(int capacity) where T : struct
		{
			m_ptr = (byte*)MemoryUtil.Alloc(Unsafe.SizeOf<T>() * capacity);
			m_length = capacity;
		}
		
		public void Dispose()
		{
			MemoryUtil.Free((IntPtr)m_ptr);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>(int index) where T : struct
		{
			return ref Unsafe.AsRef<T>(Unsafe.Add<T>(m_ptr, index));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Read<T>(int index) where T : struct
		{
			return Unsafe.Read<T>(Unsafe.Add<T>(m_ptr, index));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Write<T>(int index, in T value) where T : struct
		{
			Unsafe.Write(Unsafe.Add<T>(m_ptr, index), value);
		}
	}
}