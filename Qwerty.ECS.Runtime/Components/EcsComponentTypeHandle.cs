// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Components
{
#if UNITY_EDITOR
    [Unity.Burst.BurstCompile]
#endif
    public readonly struct EcsComponentTypeHandle<T> where T : struct, IEcsComponent
    {
        public readonly int typeIndex;
        internal EcsComponentTypeHandle(int typeIndex)
        {
            this.typeIndex = typeIndex;
        }
    }
}