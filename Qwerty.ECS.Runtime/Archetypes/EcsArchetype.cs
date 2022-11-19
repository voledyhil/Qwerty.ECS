using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Archetypes
{
    public class EcsArchetype : IDisposable
    {
        public readonly int index;
        
        internal readonly byte[] typeIndices;
        internal readonly HashSet<byte> typeIndicesSet;
        internal readonly EcsArchetype[] next;
        internal readonly EcsArchetype[] prior;
        
        internal EcsEntityCollection entities;

        internal EcsArchetype(int index, byte[] typeIndices)
        {
            this.index = index;
            this.typeIndices = typeIndices;
            
            typeIndicesSet = new HashSet<byte>(typeIndices);
            entities = new EcsEntityCollection(1024);
            next = new EcsArchetype[EcsTypeManager.typeCount];
            prior = new EcsArchetype[EcsTypeManager.typeCount];
        }

        public void Dispose()
        {
            entities.Dispose();
        }
    }
}