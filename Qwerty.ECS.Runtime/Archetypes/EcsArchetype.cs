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
        internal readonly unsafe UnsafeArray* componentsOffset;
        internal readonly unsafe IntMap* componentsMap;

        internal readonly int chunkCapacity;
        
        internal unsafe EcsArchetype(int index, byte[] indices, PrimeStorage* primeStorage, EcsWorldSetting setting)
        {
            this.index = index;
            this.indices = indices;

            chunks = (Chunks*)MemoryUtilities.Alloc<Chunks>(1);

            componentsOffset = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
            componentsOffset->Realloc<int>(indices.Length + 1);

            componentsMap = (IntMap*)MemoryUtilities.Alloc<IntMap>(1);
            componentsMap->Alloc(primeStorage->GetPrime(2 * indices.Length));

            for (int i = 0; i < indices.Length; i++)
            {
                int typeIndex = indices[i];
                componentsOffset->Write(i, rowCapacityInBytes);
                componentsMap->Set(typeIndex, i);
                rowCapacityInBytes += EcsTypeManager.Sizes[typeIndex];
            }
            
            componentsOffset->Write(indices.Length, rowCapacityInBytes);
            rowCapacityInBytes += Unsafe.SizeOf<EcsEntity>();
            chunkCapacity = setting.archetypeChunkSizeInByte / rowCapacityInBytes;
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
            
            componentsOffset->Dispose();
            componentsMap->Dispose();
            
            MemoryUtilities.Free((IntPtr)chunks);
            MemoryUtilities.Free((IntPtr)componentsOffset);
            MemoryUtilities.Free((IntPtr)componentsMap);
        }
    }
}