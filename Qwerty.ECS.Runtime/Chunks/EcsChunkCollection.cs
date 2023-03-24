using Qwerty.ECS.Runtime.Archetypes;

namespace Qwerty.ECS.Runtime.Chunks
{

    public readonly struct EcsChunkCollection : IDisposable
    {
        public int count => m_chunksCount;

        private readonly IntPtr m_archetypes;
        private readonly IntPtr m_chunks;

        private readonly int m_archetypesCount;
        private readonly int m_chunksCount;

        private readonly int m_sizeOfIntPtr;

        internal unsafe EcsChunkCollection(IntPtr archetypes, int archetypesCount, int chunksCount)
        {
            m_archetypes = archetypes;
            m_archetypesCount = archetypesCount;
            m_sizeOfIntPtr = MemoryUtil.SizeOf<IntPtr>();
            m_chunks = MemoryUtil.Alloc((uint)(m_sizeOfIntPtr * chunksCount));
            m_chunksCount = chunksCount;

            int archetypeIndex = 0;
            int index = 0;
            while (archetypeIndex < archetypesCount)
            {
                IntPtr intPtr = MemoryUtil.Read<IntPtr>(archetypes, archetypeIndex++ * m_sizeOfIntPtr);
                EcsChunk* chunk = ((EcsArchetype.Chunks*)intPtr)->last;
                while (chunk != null)
                {
                    MemoryUtil.Write(m_chunks, m_sizeOfIntPtr * index++, (IntPtr)chunk);
                    chunk = chunk->prior;
                }
            }
        }

        public unsafe EcsChunkAccessor this[int index] =>
            new EcsChunkAccessor((EcsChunk*)MemoryUtil.Read<IntPtr>(m_chunks, index * m_sizeOfIntPtr));

        public EcsChunkEnumerator GetEnumerator() => new EcsChunkEnumerator(m_archetypes, m_archetypesCount);

        public void Dispose()
        {
            MemoryUtil.Free(m_chunks);
        }
    }
}