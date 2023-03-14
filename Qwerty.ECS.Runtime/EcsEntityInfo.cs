using Qwerty.ECS.Runtime.Archetypes;

namespace Qwerty.ECS.Runtime
{
    internal unsafe readonly struct EcsEntityInfo
    {
        public readonly int archetypeIndex;
        public readonly int chunkIndex;
        public readonly EcsArchetypeChunk* chunk;

        public EcsEntityInfo(int archetypeIndex, int chunkIndex, EcsArchetypeChunk* chunk)
        {
            this.archetypeIndex = archetypeIndex;
            this.chunkIndex = chunkIndex;
            this.chunk = chunk;
        }
    }
}