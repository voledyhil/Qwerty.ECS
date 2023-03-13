using Qwerty.ECS.Runtime.Archetypes;

namespace Qwerty.ECS.Runtime
{
    internal unsafe readonly struct EcsArchetypeChunkInfo
    {
        public readonly int archetypeIndex;
        public readonly int index;
        public readonly EcsArchetypeChunk* chunk;

        public EcsArchetypeChunkInfo(int archetypeIndex, int index, EcsArchetypeChunk* chunk)
        {
            this.archetypeIndex = archetypeIndex;
            this.index = index;
            this.chunk = chunk;
        }
    }
}