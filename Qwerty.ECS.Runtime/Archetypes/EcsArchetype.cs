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
        internal readonly unsafe IntMap* indexMap;
        internal readonly unsafe IntMap* offsetMap;

        internal readonly int chunkCapacity;
        internal readonly int chunkCapacityInBytes;
        internal readonly int entityOffset;
        
        internal unsafe EcsArchetype(int index, byte[] indices, PrimeStorage* primeStorage, EcsWorldSetting setting)
        {
            this.index = index;
            this.indices = indices;

            chunks = (Chunks*)MemoryUtil.Alloc<Chunks>(1);

            componentsOffset = (UnsafeArray*)MemoryUtil.Alloc<UnsafeArray>(1);
            componentsOffset->Alloc<int>(indices.Length);

            indexMap = (IntMap*)MemoryUtil.Alloc<IntMap>(1);
            indexMap->Alloc(primeStorage->GetPrime(2 * indices.Length));
            
            offsetMap = (IntMap*)MemoryUtil.Alloc<IntMap>(1);
            offsetMap->Alloc(primeStorage->GetPrime(2 * indices.Length));

            for (int i = 0; i < indices.Length; i++)
            {
                int typeIndex = indices[i];

                indexMap->Set(typeIndex, i);
                offsetMap->Set(typeIndex, rowCapacityInBytes);
                componentsOffset->Write(i, rowCapacityInBytes);
                
                rowCapacityInBytes += EcsTypeManager.Sizes[typeIndex];
            }

            entityOffset = rowCapacityInBytes;
            rowCapacityInBytes += Unsafe.SizeOf<EcsEntity>();
            
            chunkCapacity = setting.archetypeChunkMaxSizeInByte / rowCapacityInBytes;
            chunkCapacityInBytes = chunkCapacity * rowCapacityInBytes;
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
            offsetMap->Dispose();
            indexMap->Dispose();
            
            MemoryUtil.Free((IntPtr)chunks);
            MemoryUtil.Free((IntPtr)componentsOffset);
            MemoryUtil.Free((IntPtr)offsetMap);
            MemoryUtil.Free((IntPtr)indexMap);
        }
    }
}