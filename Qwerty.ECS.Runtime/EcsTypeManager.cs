using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
	internal static class EcsTypeManager
	{
		public static byte typeCount => m_typeCount;
		internal static readonly int[] Sizes = new int[byte.MaxValue];

		private static byte m_typeCount;
		
		internal static byte Register<TC>() where TC : struct
		{
			int index = m_typeCount++;
			Sizes[index] = Unsafe.SizeOf<TC>();
			return (byte) index;
		}
	}
}