namespace Qwerty.ECS.Runtime
{
    public class EcsWorldSetting
    {
        public int archetypeCapacity = byte.MaxValue;
        public int archetypeChunkSizeInByte = 2 ^ 14;
    }
}