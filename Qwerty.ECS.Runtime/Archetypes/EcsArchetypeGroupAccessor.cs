using System;
using Qwerty.ECS.Runtime.Chunks;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    public readonly struct EcsArchetypeGroupAccessor
    {
        private readonly IntPtr m_archetypes;
        private readonly int m_archetypesCount;

        internal EcsArchetypeGroupAccessor(IntPtr archetypes, int archetypesCount)
        {
            m_archetypes = archetypes;
            m_archetypesCount = archetypesCount;
        }

        public EcsChunkEnumerator GetEnumerator() => new EcsChunkEnumerator(m_archetypes, m_archetypesCount);
    }
}