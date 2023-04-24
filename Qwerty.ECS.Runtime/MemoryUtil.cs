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
		
		public static IntPtr Alloc<T>(int capacity) where T : struct
		{
			return Alloc((uint)(SizeOf<T>() * capacity));
		}
		
		public static unsafe IntPtr Alloc(uint sizeInBytes)
		{
			IntPtr ptr = Marshal.AllocHGlobal((int)sizeInBytes);
			//IntPtr ptr = (IntPtr)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(sizeInBytes, 4, Unity.Collections.Allocator.Persistent);
			Unsafe.InitBlock((void*)ptr, 0, sizeInBytes);
			return ptr;
		}
		
		public static void Free(IntPtr ptr)
		{
			Marshal.FreeHGlobal(ptr);
			//Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Free((void*)ptr, Unity.Collections.Allocator.Persistent);
		}

		public static int SizeOf<T>() where T : struct
		{
			return Unsafe.SizeOf<T>();
		}

		public static unsafe T Read<T>(IntPtr ptr, int offset) where T : struct
		{
			return Unsafe.Read<T>((void*)(ptr + offset));
		}
		
		public static unsafe T ReadElement<T>(IntPtr ptr, int index) where T : struct
		{
			return Unsafe.Read<T>(Unsafe.Add<T>((void*)ptr, index));
		}
		
		public static unsafe void Write<T>(IntPtr ptr, int offset, T value) where T : struct
		{
			Unsafe.Write((void*)(ptr + offset), value);
		}
		
		public static unsafe void WriteElement<T>(IntPtr ptr, int index, T value) where T : struct
		{
			Unsafe.Write(Unsafe.Add<T>((void*)ptr, index), value);
		}
	}
}