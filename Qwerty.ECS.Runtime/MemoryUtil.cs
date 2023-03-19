using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Qwerty.ECS.Runtime
{
	internal static class MemoryUtil
	{
		internal static IntPtr Alloc<T>() where T : struct
		{
			return Alloc(Unsafe.SizeOf<T>());
		}
		
		internal static unsafe IntPtr Alloc(int sizeInBytes = 1)
		{
			IntPtr ptr = Marshal.AllocHGlobal(sizeInBytes);
			if (sizeInBytes > 0)
			{
				Unsafe.InitBlock((void*)ptr, 0, (uint)sizeInBytes);
			}
			return ptr;
		}
		
		internal static void Free(IntPtr ptr)
		{
			Marshal.FreeHGlobal(ptr);
		}

		internal static int SizeOf<T>() where T : struct
		{
			return Unsafe.SizeOf<T>();
		}

		internal static unsafe T Read<T>(IntPtr ptr, int offset)
		{
			return Unsafe.Read<T>((void*)(ptr + offset));
		}
		
		internal static unsafe void Write<T>(IntPtr ptr, int offset, T value)
		{
			Unsafe.Write((void*)(ptr + offset), value);
		}
	}
}