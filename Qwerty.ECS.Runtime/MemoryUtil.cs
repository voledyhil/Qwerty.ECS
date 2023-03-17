using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
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
	}
}