namespace Qwerty.ECS.Runtime.Archetypes
{
    public class EcsArchetype : IDisposable
    {
        public int Index { get; }
        
        internal readonly byte[] TypeIndices;
        internal readonly HashSet<byte> TypeIndicesSet;
        internal readonly EcsArchetype[] Next;
        internal readonly EcsArchetype[] Prior;
        
        internal EcsEntityCollection Entities;

        internal EcsArchetype(int index, byte[] typeIndices)
        {
            Entities = new EcsEntityCollection(1024);
            
            Next = new EcsArchetype[EcsTypeManager.TypeCount];
            Prior = new EcsArchetype[EcsTypeManager.TypeCount];

            Index = index;
            TypeIndices = typeIndices;
            TypeIndicesSet = new HashSet<byte>(typeIndices);
        }

        public void Dispose()
        {
            Entities.Dispose();
        }
    }
}