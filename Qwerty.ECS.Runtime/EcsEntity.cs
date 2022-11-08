using System;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public readonly struct EcsEntity : IEquatable<EcsEntity>
    {
        public static EcsEntity Null = new EcsEntity(0, 0);
        public readonly int Index;
        public readonly int Version;

        public EcsEntity(int index, int version)
        {
            Index = index;
            Version = version;
        }

        public bool Equals(EcsEntity other)
        {
            return Index == other.Index && Version == other.Version;
        }

        public override bool Equals(object obj)
        {
            return obj is EcsEntity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Index;
        }

        public static bool operator ==(EcsEntity lhs, EcsEntity rhs)
        {
            return lhs.Index == rhs.Index && lhs.Version == rhs.Version;
        }

        public static bool operator !=(EcsEntity lhs, EcsEntity rhs)
        {
            return lhs.Index != rhs.Index || lhs.Version != rhs.Version;
        }
    }
}