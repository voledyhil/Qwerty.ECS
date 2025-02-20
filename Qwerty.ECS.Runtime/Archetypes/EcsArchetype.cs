using System;
using System.Collections.Generic;
using Qwerty.ECS.Runtime.Chunks;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    internal class EcsArchetype : IDisposable
    {
        internal struct Chunks : IDisposable
        {
            internal unsafe EcsChunkHeader* header;
            internal unsafe EcsChunk* last;

            internal unsafe void Alloc(int[] indices, int bodySize)
            {
                header = (EcsChunkHeader*)MemoryUtil.Alloc<EcsChunkHeader>();
                header->Alloc(bodySize, indices);
            }

            public unsafe void Dispose()
            {
                EcsChunk* chunk = last;
                while (chunk != null)
                {
                    EcsChunk* toDispose = chunk;
                    chunk = chunk->prior;
                    toDispose->Dispose();
                }
                header->Dispose();
                MemoryUtil.Free((IntPtr)header);
            }
        }
        
        internal readonly int index;
        internal readonly int[] indices;
        internal readonly Dictionary<int, int> next = new Dictionary<int, int>();
        internal readonly Dictionary<int, int> prior = new Dictionary<int, int>();
        
        internal readonly unsafe Chunks* chunks;
        internal unsafe EcsArchetype(int index, int[] indices, EcsWorldSetting setting)
        {
            this.index = index;
            this.indices = indices;
            
            chunks = (Chunks*)MemoryUtil.Alloc<Chunks>();
            chunks->Alloc(indices, setting.archetypeChunkMaxSizeInByte);
        }
        
        public unsafe void Dispose()
        {
            chunks->Dispose();
            MemoryUtil.Free((IntPtr)chunks);
        }
    }
}