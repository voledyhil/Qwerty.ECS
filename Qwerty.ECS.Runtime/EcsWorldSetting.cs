namespace Qwerty.ECS.Runtime
{
    public class EcsWorldSetting
    {
        public int entitiesCapacity = 2 ^ 20;           // 1 048 576
        public int archetypeChunkSizeInByte = 2 ^ 14;   // 16 384
    }
}