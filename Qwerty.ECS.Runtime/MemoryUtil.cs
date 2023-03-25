using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
	public static class MemoryUtil
	{
		public static IntPtr Alloc<T>() where T : struct
		{
			return Alloc((uint)SizeOf<T>());
		}
		
		public static unsafe IntPtr Alloc(uint sizeInBytes)
		{
			IntPtr ptr = Marshal.AllocHGlobal((int)sizeInBytes);
			Unsafe.InitBlock((void*)ptr, 0, sizeInBytes);
			return ptr;
		}
		
		public static void Free(IntPtr ptr)
		{
			Marshal.FreeHGlobal(ptr);
		}

		public static int SizeOf<T>() where T : struct
		{
			return Unsafe.SizeOf<T>();
		}

		public static unsafe T Read<T>(IntPtr ptr, int offset) where T : struct
		{
			return Unsafe.Read<T>((void*)(ptr + offset));
		}
		
		public static unsafe void Write<T>(IntPtr ptr, int offset, T value) where T : struct
		{
			Unsafe.Write((void*)(ptr + offset), value);
		}
	}
}