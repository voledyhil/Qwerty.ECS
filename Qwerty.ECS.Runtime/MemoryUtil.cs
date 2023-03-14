using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MemoryUnsafe = System.Runtime.CompilerServices.Unsafe;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
	public static class MemoryUtil
	{
		public static unsafe IntPtr Alloc<T>(int capacity) where T : struct
		{
			int sizeOfInBytes = MemoryUnsafe.SizeOf<T>() * capacity;
			IntPtr ptr = Marshal.AllocHGlobal(sizeOfInBytes);
			if (sizeOfInBytes > 0)
			{
				MemoryUnsafe.InitBlock((void*)ptr, 0, (uint)sizeOfInBytes);
			}
			return ptr;
		}
		
		public static IntPtr Alloc(int newCapacityInBytes)
		{
			IntPtr pointer = Marshal.AllocHGlobal(newCapacityInBytes);
			if (newCapacityInBytes > 0)
			{
				MemClear(pointer, newCapacityInBytes);
			}
			return pointer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Free(IntPtr ptr)
		{
			Marshal.FreeHGlobal(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static unsafe void MemClear(IntPtr destination, int sizeOfInBytes)
		{
			MemoryUnsafe.InitBlock((void*)destination, 0, (uint)sizeOfInBytes);
		}
	}
}