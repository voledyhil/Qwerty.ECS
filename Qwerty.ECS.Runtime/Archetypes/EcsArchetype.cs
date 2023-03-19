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
        internal readonly int rowCapacityInBytes;
        
        
        internal readonly unsafe Chunks* chunks;

        internal readonly int chunkCapacity;
        internal readonly int entityOffset;
        
        internal unsafe EcsArchetype(int index, byte[] indices, EcsWorldSetting setting)
        {
            this.index = index;
            this.indices = indices;

            chunks = (Chunks*)MemoryUtil.Alloc<Chunks>();
            foreach (int typeIndex in indices)
            {
                rowCapacityInBytes += EcsTypeManager.Sizes[typeIndex];
            }
            entityOffset = rowCapacityInBytes;
            rowCapacityInBytes += Unsafe.SizeOf<EcsEntity>();
            chunkCapacity = setting.archetypeChunkMaxSizeInByte / rowCapacityInBytes;
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