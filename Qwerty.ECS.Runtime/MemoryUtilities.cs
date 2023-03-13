using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MemoryUnsafe = System.Runtime.CompilerServices.Unsafe;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
	public static class MemoryUtilities
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
		
		public static IntPtr Alloc(int newCapacityInBytes, bool clear)
		{
			IntPtr pointer = Marshal.AllocHGlobal(newCapacityInBytes);
			//Note MemClear is actually necessary
			if (clear && newCapacityInBytes > 0)
			{
				MemClear(pointer, newCapacityInBytes);
			}

			return pointer;
		}

		public static unsafe IntPtr Realloc(IntPtr realBuffer, int newCapacityInBytes, int numberOfElementsToCopyInBytes)
		{
			//Alloc returns the correct Signed Pointer already
			//if copy == true, memclear is optimised, otherwise memclear if set to true
			IntPtr signedPointer = Alloc(newCapacityInBytes, false);

			//Copy only the real data
			if (numberOfElementsToCopyInBytes > 0)
			{
				if (newCapacityInBytes > numberOfElementsToCopyInBytes)
				{
					MemoryUnsafe.CopyBlock((void*)signedPointer, (void*)realBuffer, (uint)numberOfElementsToCopyInBytes);
					int bytesToClear = newCapacityInBytes - numberOfElementsToCopyInBytes;
					IntPtr startingBytePointerToClear = signedPointer + numberOfElementsToCopyInBytes;
					MemClear(startingBytePointerToClear, bytesToClear);
				}
				else
				{
					MemoryUnsafe.CopyBlock((void*)signedPointer, (void*)realBuffer, (uint)newCapacityInBytes);
				}
			}

			//Free unsigns the pointer itself
			Free(realBuffer);
			return signedPointer;
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
		
		public static unsafe void MemMove<T>(IntPtr source, int srcStartIndex, int targetStartIndex, int count) where T : struct
		{
			int sizeOf = MemoryUnsafe.SizeOf<T>();
			uint sizeOfInBytes = (uint)(sizeOf * count);
			Buffer.MemoryCopy((void*)(source + srcStartIndex * sizeOf), (void*)(source + targetStartIndex * sizeOf), sizeOfInBytes, sizeOfInBytes);
		}
	}
}