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

        internal readonly int chunkCapacity;
        internal readonly int entityOffset;
        
        internal unsafe EcsArchetype(int index, byte[] indices, PrimeStorage* primeStorage, EcsWorldSetting setting)
        {
            this.index = index;
            this.indices = indices;

            chunks = (Chunks*)MemoryUtil.Alloc<Chunks>();

            componentsOffset = (UnsafeArray*)MemoryUtil.Alloc<UnsafeArray>();
            componentsOffset->Alloc<int>(indices.Length);

            indexMap = (IntMap*)MemoryUtil.Alloc<IntMap>();
            indexMap->Alloc(primeStorage->GetPrime(2 * indices.Length));
            
            for (int i = 0; i < indices.Length; i++)
            {
                int typeIndex = indices[i];

                indexMap->Set(typeIndex, i);
                componentsOffset->Write(i, rowCapacityInBytes);
                
                rowCapacityInBytes += EcsTypeManager.Sizes[typeIndex];
            }

            entityOffset = rowCapacityInBytes;
            rowCapacityInBytes += Unsafe.SizeOf<EcsEntity>();
            chunkCapacity = setting.archetypeChunkMaxSizeInByte / rowCapacityInBytes;
        }
        
        
        /*
            private const int MaxComponentCount = 20;
            private const byte Length = 47;
            private void Add(int[] storage, int key, int value)
            {
                int hash = key % Length;
                while (storage[hash] > -1) hash = (hash + 1) % Length;
                storage[hash] = key;
                storage[Length + hash] = value;
            }

            private int Get(int[] storage, int key)
            {
                int hash = key % Length;
                int curKey;
                while ((curKey = storage[hash]) > -1)
                {
                    if (curKey == key) return storage[Length + hash];
                    hash = (hash + 1) % Length;
                }
                throw new ArgumentException(nameof(Get));
            }
*/

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
            indexMap->Dispose();
            
            MemoryUtil.Free((IntPtr)chunks);
            MemoryUtil.Free((IntPtr)componentsOffset);
            MemoryUtil.Free((IntPtr)indexMap);
        }
    }
}