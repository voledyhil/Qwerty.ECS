using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
	internal static class MemoryUtil
	{
		internal static IntPtr Alloc<T>() where T : struct
		{
			return Alloc((uint)SizeOf<T>());
		}
		
		internal static unsafe IntPtr Alloc(uint sizeInBytes)
		{
			IntPtr ptr = Marshal.AllocHGlobal((int)sizeInBytes);
			Unsafe.InitBlock((void*)ptr, 0, sizeInBytes);
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