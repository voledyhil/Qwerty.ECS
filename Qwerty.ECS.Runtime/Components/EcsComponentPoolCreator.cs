namespace Qwerty.ECS.Runtime.Components
{
	internal class EcsComponentPoolCreator<T> : IEcsComponentPoolCreator where T : struct, IEcsComponent 
	{
		public IEcsComponentPool Instantiate(int capacity)
		{
			return new EcsComponentPool<T>(capacity);
		}
	}
}