using Qwerty.ECS.Runtime.Archetypes;

namespace Qwerty.ECS.Runtime
{
    internal unsafe readonly struct EcsArchetypeInfo
    {
        public readonly int archetypeIndex;
        public readonly int indexInChunk;
        public readonly EcsArchetypeChunk* chunk;

        public EcsArchetypeInfo(int archetypeIndex, int indexInChunk, EcsArchetypeChunk* chunk)
        {
            this.archetypeIndex = archetypeIndex;
            this.indexInChunk = indexInChunk;
            this.chunk = chunk;
        }
    }
}