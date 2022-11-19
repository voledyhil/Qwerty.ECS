using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public sealed class EcsFilter : IEquatable<EcsFilter>
    {
        public readonly HashSet<byte> any;
        public readonly HashSet<byte> all;
        public readonly HashSet<byte> none;

        public EcsFilter()
        {
            any = new HashSet<byte>();
            all = new HashSet<byte>();
            none = new HashSet<byte>();
        }

        private EcsFilter(EcsFilter other)
        {
            any = new HashSet<byte>(other.any);
            all = new HashSet<byte>(other.all);
            none = new HashSet<byte>(other.none);
        }
        
        public EcsFilter AnyOf<T0>() where T0 : struct, IEcsComponent
        {
            any.Add(EcsComponentType<T0>.index);
            m_hashDirty = false;

            return this;
        }
        
        public EcsFilter AnyOf<T0, T1>() where T0 : struct, IEcsComponent where T1 : struct, IEcsComponent
        {
            any.Add(EcsComponentType<T0>.index);
            any.Add(EcsComponentType<T1>.index);
            m_hashDirty = false;

            return this;
        }
        
        public EcsFilter AnyOf<T0, T1, T2>() where T0 : struct, IEcsComponent where T1 : struct, IEcsComponent where T2 : struct, IEcsComponent
        {
            any.Add(EcsComponentType<T0>.index);
            any.Add(EcsComponentType<T1>.index);
            any.Add(EcsComponentType<T2>.index);
            m_hashDirty = false;

            return this;
        }
        

        
        public EcsFilter AllOf<T0>() where T0 : struct, IEcsComponent
        {
            all.Add(EcsComponentType<T0>.index);
            m_hashDirty = false;

            return this;
        }
        
        public EcsFilter AllOf<T0, T1>() where T0 : struct, IEcsComponent where T1 : struct, IEcsComponent
        {
            all.Add(EcsComponentType<T0>.index);
            all.Add(EcsComponentType<T1>.index);
            m_hashDirty = false;

            return this;
        }
        
        public EcsFilter AllOf<T0, T1, T2>() where T0 : struct, IEcsComponent where T1 : struct, IEcsComponent where T2 : struct, IEcsComponent
        {
            all.Add(EcsComponentType<T0>.index);
            all.Add(EcsComponentType<T1>.index);
            all.Add(EcsComponentType<T2>.index);
            m_hashDirty = false;

            return this;
        }
        
        public EcsFilter AllOf<T0, T1, T2, T3>() where T0 : struct, IEcsComponent where T1 : struct, IEcsComponent where T2 : struct, IEcsComponent where T3 : struct, IEcsComponent
        {
            all.Add(EcsComponentType<T0>.index);
            all.Add(EcsComponentType<T1>.index);
            all.Add(EcsComponentType<T2>.index);
            all.Add(EcsComponentType<T3>.index);
            m_hashDirty = false;

            return this;
        }
        
        public EcsFilter AllOf<T0, T1, T2, T3, T4>() where T0 : struct, IEcsComponent where T1 : struct, IEcsComponent where T2 : struct, IEcsComponent where T3 : struct, IEcsComponent where T4 : struct, IEcsComponent
        {
            all.Add(EcsComponentType<T0>.index);
            all.Add(EcsComponentType<T1>.index);
            all.Add(EcsComponentType<T2>.index);
            all.Add(EcsComponentType<T3>.index);
            all.Add(EcsComponentType<T4>.index);
            m_hashDirty = false;

            return this;
        }
        
        public EcsFilter AllOf<T0, T1, T2, T3, T4, T5>() where T0 : struct, IEcsComponent where T1 : struct, IEcsComponent where T2 : struct, IEcsComponent where T3 : struct, IEcsComponent where T4 : struct, IEcsComponent where T5 : struct, IEcsComponent
        {
            all.Add(EcsComponentType<T0>.index);
            all.Add(EcsComponentType<T1>.index);
            all.Add(EcsComponentType<T2>.index);
            all.Add(EcsComponentType<T3>.index);
            all.Add(EcsComponentType<T4>.index);
            all.Add(EcsComponentType<T5>.index);
            m_hashDirty = false;

            return this;
        }

        public EcsFilter NoneOf<T0>() where T0 : struct, IEcsComponent
        {
            none.Add(EcsComponentType<T0>.index);
            m_hashDirty = false;

            return this;
        }
        
        public EcsFilter NoneOf<T0, T1>() 
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
        {
            none.Add(EcsComponentType<T0>.index);
            none.Add(EcsComponentType<T1>.index);
            m_hashDirty = false;

            return this;
        }
        
        public EcsFilter NoneOf<T0, T1, T2>() 
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
        {
            none.Add(EcsComponentType<T0>.index);
            none.Add(EcsComponentType<T1>.index);
            none.Add(EcsComponentType<T2>.index);
            m_hashDirty = false;

            return this;
        }

        public EcsFilter Clone()
        {
            return new EcsFilter(this);
        }

        public bool Equals(EcsFilter other)
        {
            if (other == null || other.GetHashCode() != GetHashCode())
                return false;
 
            return other.all.SetEquals(all) && other.any.SetEquals(any) && other.none.SetEquals(none);
        }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as EcsFilter);
        }

        private int m_hash;
        private bool m_hashDirty;

        public override int GetHashCode()
        {
            if (m_hashDirty)
            {
                return m_hash;
            }

            int hash = 97;
            hash = CalculateHash(hash, all, 3, 53);
            hash = CalculateHash(hash, any, 307, 367);
            hash = CalculateHash(hash, none, 647, 683);

            m_hash = hash;
            m_hashDirty = true;

            return m_hash;
        }
        
        private static int CalculateHash(int hash, HashSet<byte> indices, int i1, int i2)
        {
            byte[] indicesArray = indices.ToArray();
            Array.Sort(indicesArray);
            foreach (byte index in indicesArray)
            {
                hash ^= index * i1;
            }
            hash ^= indices.Count * i2;
            return hash;
        }
    }
}