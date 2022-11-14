using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    /// <summary>
    /// Query defines the set of component types that an archetype must contain in
    /// order for its entities to be included in the view.
    /// You can also exclude archetypes that contain specific types of components.
    /// </summary>
    public class EcsFilter : IEquatable<EcsFilter>
    {
        public HashSet<byte> Any;
        public HashSet<byte> All;
        public HashSet<byte> None;

        /// <summary>
        /// At least one of the component types in this array must exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        public EcsFilter AnyOf<T0>() where T0 : struct, IEcsComponent
        {
            return AnyOf(EcsComponentType<T0>.index);
        }

        /// <summary>
        /// At least one of the component types in this array must exist in the archetype
        /// </summary>
        /// <returns></returns>
        public EcsFilter AnyOf<T0, T1>() 
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
        {
            return AnyOf(EcsComponentType<T0>.index, EcsComponentType<T1>.index);
        }

        /// <summary>
        /// At least one of the component types in this array must exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        public EcsFilter AnyOf<T0, T1, T2>() 
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
        {
            return AnyOf(EcsComponentType<T0>.index, EcsComponentType<T1>.index, EcsComponentType<T2>.index);
        }


        /// <summary>
        /// At least one of the component types in this array must exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        private EcsFilter AnyOf(params byte[] types)
        {
            Any ??= new HashSet<byte>();
            foreach (byte type in types)
            {
                Any.Add(type);
                m_isCached = false;
            }

            return this;
        }

        /// <summary>
        /// All component types in this array must exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        public EcsFilter AllOf<T0>() where T0 : struct, IEcsComponent
        {
            return AllOf(EcsComponentType<T0>.index);
        }

        /// <summary>
        /// All component types in this array must exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        public EcsFilter AllOf<T0, T1>() 
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
        {
            return AllOf(EcsComponentType<T0>.index, EcsComponentType<T1>.index);
        }

        /// <summary>
        /// All component types in this array must exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        public EcsFilter AllOf<T0, T1, T2>()
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
        {
            return AllOf(EcsComponentType<T0>.index, EcsComponentType<T1>.index, EcsComponentType<T2>.index);
        }

        /// <summary>
        /// All component types in this array must exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        public EcsFilter AllOf<T0, T1, T2, T3>() where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
        {
            return AllOf(EcsComponentType<T0>.index, EcsComponentType<T1>.index, EcsComponentType<T2>.index, EcsComponentType<T3>.index);
        }
        
        /// <summary>
        /// All component types in this array must exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        public EcsFilter AllOf<T0, T1, T2, T3, T4>() 
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
            where T4 : struct, IEcsComponent
        {
            return AllOf(EcsComponentType<T0>.index, EcsComponentType<T1>.index, EcsComponentType<T2>.index, EcsComponentType<T3>.index, EcsComponentType<T4>.index);
        }
        
        /// <summary>
        /// All component types in this array must exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        public EcsFilter AllOf<T0, T1, T2, T3, T4, T5>() 
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
            where T3 : struct, IEcsComponent
            where T4 : struct, IEcsComponent
            where T5 : struct, IEcsComponent
        {
            return AllOf(EcsComponentType<T0>.index, EcsComponentType<T1>.index, EcsComponentType<T2>.index, EcsComponentType<T3>.index, EcsComponentType<T4>.index, EcsComponentType<T5>.index);
        }


        /// <summary>
        /// All component types in this array must exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        private EcsFilter AllOf(params byte[] types)
        {
            All ??= new HashSet<byte>();
            foreach (byte type in types)
            {
                All.Add(type);
                m_isCached = false;
            }

            return this;
        }

        /// <summary>
        /// None of the component types in this array can exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        public EcsFilter NoneOf<T0>() where T0 : struct, IEcsComponent
        {
            return NoneOf(EcsComponentType<T0>.index);
        }

        /// <summary>
        /// None of the component types in this array can exist in the archetype 
        /// </summary>
        /// <returns>Current filter</returns>
        public EcsFilter NoneOf<T0, T1>() 
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
        {
            return NoneOf(EcsComponentType<T0>.index, EcsComponentType<T1>.index);
        }

        /// <summary>
        /// None of the component types in this array can exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        public EcsFilter NoneOf<T0, T1, T2>() 
            where T0 : struct, IEcsComponent
            where T1 : struct, IEcsComponent
            where T2 : struct, IEcsComponent
        {
            return NoneOf(EcsComponentType<T0>.index, EcsComponentType<T1>.index, EcsComponentType<T2>.index);
        }

        /// <summary>
        /// None of the component types in this array can exist in the archetype
        /// </summary>
        /// <returns>Current filter</returns>
        private EcsFilter NoneOf(params byte[] types)
        {
            None ??= new HashSet<byte>();
            foreach (byte type in types)
            {
                None.Add(type);
                m_isCached = false;
            }

            return this;
        }

        /// <summary>
        /// Creates a new filter with the same set of parameters
        /// </summary>
        /// <returns>New Filter</returns>
        public EcsFilter Clone()
        {
            EcsFilter filter = new EcsFilter();

            if (Any != null)
                filter.Any = new HashSet<byte>(Any);

            if (All != null)
                filter.All = new HashSet<byte>(All);

            if (None != null)
                filter.None = new HashSet<byte>(None);

            return filter;
        }

        public bool Equals(EcsFilter other)
        {
            if (other == null || other.GetHashCode() != GetHashCode() || other.GetType() != GetType())
                return false;

            if (other.All != null && All != null && !other.All.SetEquals(All))
                return false;

            if (other.Any != null && Any != null && !other.Any.SetEquals(Any))
                return false;

            return other.None == null || None == null || other.None.SetEquals(None);
        }

        private int m_hash;
        private bool m_isCached;

        public override int GetHashCode()
        {
            if (m_isCached)
                return m_hash;

            int hash = GetType().GetHashCode();
            hash = CalculateHash(hash, All, 3, 53);
            hash = CalculateHash(hash, Any, 307, 367);
            hash = CalculateHash(hash, None, 647, 683);

            m_hash = hash;
            m_isCached = true;

            return m_hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CalculateHash(int hash, HashSet<byte> indices, int i1, int i2)
        {
            if (indices == null)
                return hash;

            byte[] indicesArray = indices.ToArray();
            Array.Sort(indicesArray);

            hash = indicesArray.Aggregate(hash, (current, index) => current ^ index * i1);
            hash ^= indices.Count * i2;
            return hash;
        }
    }
}