using System;
using System.Collections.Generic;
using System.Linq;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public sealed class EcsFilter : IEquatable<EcsFilter>
    {
        public short[] any
        {
            get
            {
                Build();
                return m_anyArray;
            }
        }

        public short[] all
        {
            get
            {
                Build();
                return m_allArray;
            }
        }

        public short[] none
        {
            get
            {
                Build();
                return m_noneArray;
            }
        }

        private readonly HashSet<short> m_any;
        private readonly HashSet<short> m_all;
        private readonly HashSet<short> m_none;

        private short[] m_anyArray;
        private short[] m_allArray;
        private short[] m_noneArray;

        private int m_hash;
        private bool m_dirty = true;

        public EcsFilter()
        {
            m_any = new HashSet<short>();
            m_all = new HashSet<short>();
            m_none = new HashSet<short>();
        }

        private EcsFilter(EcsFilter other)
        {
            m_any = new HashSet<short>(other.m_any);
            m_all = new HashSet<short>(other.m_all);
            m_none = new HashSet<short>(other.m_none);
        }

        public EcsFilter AnyOf<T0>() where T0 : struct, IEcsComponent
        {
            m_any.Add(EcsComponentType<T0>.index);
            m_dirty = true;

            return this;
        }

        public EcsFilter AnyOf<T0, T1>() where T0 : struct, IEcsComponent where T1 : struct, IEcsComponent
        {
            m_any.Add(EcsComponentType<T0>.index);
            m_any.Add(EcsComponentType<T1>.index);
            m_dirty = true;

            return this;
        }

        public EcsFilter AnyOf<T0, T1, T2>() where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
        {
            m_any.Add(EcsComponentType<T0>.index);
            m_any.Add(EcsComponentType<T1>.index);
            m_any.Add(EcsComponentType<T2>.index);
            m_dirty = true;

            return this;
        }



        public EcsFilter AllOf<T0>() where T0 : struct, IEcsComponent
        {
            m_all.Add(EcsComponentType<T0>.index);
            m_dirty = true;

            return this;
        }

        public EcsFilter AllOf<T0, T1>() where T0 : struct, IEcsComponent where T1 : struct, IEcsComponent
        {
            m_all.Add(EcsComponentType<T0>.index);
            m_all.Add(EcsComponentType<T1>.index);
            m_dirty = true;

            return this;
        }

        public EcsFilter AllOf<T0, T1, T2>() where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
        {
            m_all.Add(EcsComponentType<T0>.index);
            m_all.Add(EcsComponentType<T1>.index);
            m_all.Add(EcsComponentType<T2>.index);
            m_dirty = true;

            return this;
        }

        public EcsFilter AllOf<T0, T1, T2, T3>() where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
        {
            m_all.Add(EcsComponentType<T0>.index);
            m_all.Add(EcsComponentType<T1>.index);
            m_all.Add(EcsComponentType<T2>.index);
            m_all.Add(EcsComponentType<T3>.index);
            m_dirty = true;

            return this;
        }

        public EcsFilter AllOf<T0, T1, T2, T3, T4>() where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
            where T4 : struct, IEcsComponent
        {
            m_all.Add(EcsComponentType<T0>.index);
            m_all.Add(EcsComponentType<T1>.index);
            m_all.Add(EcsComponentType<T2>.index);
            m_all.Add(EcsComponentType<T3>.index);
            m_all.Add(EcsComponentType<T4>.index);
            m_dirty = true;

            return this;
        }

        public EcsFilter AllOf<T0, T1, T2, T3, T4, T5>() where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
            where T4 : struct, IEcsComponent
            where T5 : struct, IEcsComponent
        {
            m_all.Add(EcsComponentType<T0>.index);
            m_all.Add(EcsComponentType<T1>.index);
            m_all.Add(EcsComponentType<T2>.index);
            m_all.Add(EcsComponentType<T3>.index);
            m_all.Add(EcsComponentType<T4>.index);
            m_all.Add(EcsComponentType<T5>.index);
            m_dirty = true;

            return this;
        }

        public EcsFilter NoneOf<T0>() where T0 : struct, IEcsComponent
        {
            m_none.Add(EcsComponentType<T0>.index);
            m_dirty = true;

            return this;
        }

        public EcsFilter NoneOf<T0, T1>()
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
        {
            m_none.Add(EcsComponentType<T0>.index);
            m_none.Add(EcsComponentType<T1>.index);
            m_dirty = true;

            return this;
        }

        public EcsFilter NoneOf<T0, T1, T2>()
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
        {
            m_none.Add(EcsComponentType<T0>.index);
            m_none.Add(EcsComponentType<T1>.index);
            m_none.Add(EcsComponentType<T2>.index);
            m_dirty = true;

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

            return other.m_all.SetEquals(m_all) && other.m_any.SetEquals(m_any) && other.m_none.SetEquals(m_none);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EcsFilter);
        }

        public override int GetHashCode()
        {
            if (!m_dirty)
            {
                return m_hash;
            }

            Build();

            return m_hash;
        }

        private void Build()
        {
            if (!m_dirty)
            {
                return;
            }

            m_anyArray = m_any.ToArray();
            m_allArray = m_all.ToArray();
            m_noneArray = m_none.ToArray();

            Array.Sort(m_anyArray);
            Array.Sort(m_allArray);
            Array.Sort(m_noneArray);

            int hash = 97;
            hash = CalculateHash(hash, m_allArray, 3, 53);
            hash = CalculateHash(hash, m_anyArray, 307, 367);
            hash = CalculateHash(hash, m_noneArray, 647, 683);

            m_hash = hash;
            m_dirty = false;
        }

        private static int CalculateHash(int hash, short[] indices, int i1, int i2)
        {
            foreach (short index in indices)
            {
                hash ^= index * i1;
            }

            hash ^= indices.Length * i2;
            return hash;
        }
    }
}