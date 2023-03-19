namespace Qwerty.ECS.Runtime.Components
{
    public readonly struct EcsComponentTypeHandle<T> where T : struct, IEcsComponent
    {
        public readonly int typeIndex;
        internal EcsComponentTypeHandle(int typeIndex)
        {
            this.typeIndex = typeIndex;
        }
    }
}