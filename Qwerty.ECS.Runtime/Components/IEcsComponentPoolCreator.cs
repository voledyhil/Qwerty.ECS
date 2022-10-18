namespace Qwerty.ECS.Runtime.Components
{
	internal interface IEcsComponentPoolCreator
	{
		IEcsComponentPool Instantiate(int capacity);
	}
}