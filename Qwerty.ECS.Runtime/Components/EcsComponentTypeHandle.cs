// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Components
{
    public readonly struct EcsComponentTypeHandle<T> where T : struct, IEcsComponent
    {
        public readonly short typeIndex;
        internal EcsComponentTypeHandle(short typeIndex)
        {
            this.typeIndex = typeIndex;
        }
    }
}