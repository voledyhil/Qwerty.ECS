using System.Collections.Generic;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
	internal static class EcsTypeManager
	{
		public static byte TypeCount => m_typeCount;

		internal static readonly List<(int, IEcsComponentPoolCreator)> componentsCreators = new List<(int, IEcsComponentPoolCreator)>();

		private static byte m_typeCount;
		
		internal static byte RegisterComponent<TC>() where TC : struct, IEcsComponent
		{
			int index = m_typeCount++;
			componentsCreators.Add((index, new EcsComponentPoolCreator<TC>()));
			return (byte) index;
		}
	}
}