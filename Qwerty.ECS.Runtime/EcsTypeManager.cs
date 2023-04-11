using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
	internal static class EcsTypeManager
	{
		public static short typeCount => m_typeCount;
		internal static readonly int[] Sizes = new int[short.MaxValue];

		private static short m_typeCount;
		
		internal static short Register<TC>() where TC : struct
		{
			int index = m_typeCount++;
			Sizes[index] = Unsafe.SizeOf<TC>();
			return (short) index;
		}
	}
}