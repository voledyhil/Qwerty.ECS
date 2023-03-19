// ReSharper disable once CheckNamespace
using System.Runtime.CompilerServices;

namespace Qwerty.ECS.Runtime.Archetypes
{
    internal class EcsArchetype : IDisposable
    {
        internal struct Chunks
        {
            internal unsafe EcsChunk* last;
        }
        
        internal readonly int index;
        internal readonly byte[] indices;
        internal readonly Dictionary<int, int> next = new Dictionary<int, int>();
        internal readonly Dictionary<int, int> prior = new Dictionary<int, int>();

        internal int chunksCount;
        internal readonly unsafe Chunks* chunks;
        
        internal unsafe EcsArchetype(int index, byte[] indices)
        {
            this.index = index;
            this.indices = indices;

            chunks = (Chunks*)MemoryUtil.Alloc<Chunks>();
        }

        public unsafe void Dispose()
        {
            EcsChunk* chunk = chunks->last;
            while (chunk != null)
            {
                EcsChunk* toDispose = chunk;
                chunk = chunk->prior;
                toDispose->Dispose();
            }
            MemoryUtil.Free((IntPtr)chunks);
        }
    }
}