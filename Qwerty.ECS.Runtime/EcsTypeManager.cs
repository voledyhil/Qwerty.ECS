using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime

{
	internal static class EcsTypeManager
	{
		public static byte typeCount => m_typeCount;

		//internal static readonly List<(int, IEcsComponentPoolCreator)> ComponentsCreators = new List<(int, IEcsComponentPoolCreator)>();
		internal static readonly int[] Sizes = new int[byte.MaxValue];

		private static byte m_typeCount;
		
		internal static byte RegisterComponent<TC>() where TC : struct, IEcsComponent
		{
			int index = m_typeCount++;
			//ComponentsCreators.Add((index, new EcsComponentPoolCreator<TC>()));
			Sizes[index] = Unsafe.SizeOf<TC>();
			return (byte) index;
		}
	}
}