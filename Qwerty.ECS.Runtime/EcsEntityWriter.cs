// ReSharper disable once RedundantUsingDirective
using System;
using Qwerty.ECS.Runtime.Components;


// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public readonly unsafe struct EcsEntityWriter<T0, T1, T2> : IDisposable
        where T0 : struct, IEcsComponent
        where T1 : struct, IEcsComponent
        where T2 : struct, IEcsComponent
    {
        public int length => *m_len;
        
        private readonly int m_capacity;
        private readonly int m_rowSizeInBytes;
        private readonly int m_offsetT0;
        private readonly int m_offsetT1;
        private readonly int m_offsetT2;
        private readonly int m_entitySizeOf;
        
        private readonly IntPtr m_body;
        private readonly int* m_len;

        internal EcsEntityWriter(int capacity, int rowSizeInBytes, int offsetT0, int offsetT1, int offsetT2)
        {
            m_capacity = capacity;
            m_rowSizeInBytes = rowSizeInBytes;
            m_offsetT0 = offsetT0;
            m_offsetT1 = offsetT1;
            m_offsetT2 = offsetT2;
            m_entitySizeOf = MemoryUtil.SizeOf<EcsEntity>();

            m_body = MemoryUtil.Alloc((uint)(capacity * rowSizeInBytes));
            m_len = (int*)MemoryUtil.Alloc<int>();
        }
        
        public void Write(T0 comp0, T1 comp1, T2 comp2)
        {
            int len = (*m_len)++;
            if (len >= m_capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(Write));
            }
            
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT0, comp0);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT1, comp1);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT2, comp2);
        }

        public void CopyRow(int index, IntPtr to, int toIndex)
        {
            void* src = (void*)(m_body + index * m_rowSizeInBytes);
            void* dest = (void*)(to + toIndex * m_rowSizeInBytes);
            int sizeInBytes = m_rowSizeInBytes - m_entitySizeOf;
            Buffer.MemoryCopy(src, dest, sizeInBytes, sizeInBytes);
        }

        public void Dispose()
        {
            MemoryUtil.Free(m_body);
            MemoryUtil.Free((IntPtr)m_len);
        }
    }
    
    public readonly unsafe struct EcsEntityWriter<T0, T1, T2, T3, T4> : IDisposable
        where T0 : struct, IEcsComponent
        where T1 : struct, IEcsComponent
        where T2 : struct, IEcsComponent
        where T3 : struct, IEcsComponent
        where T4 : struct, IEcsComponent
    {
        public int length => *m_len;
        
        private readonly int m_capacity;
        private readonly int m_rowSizeInBytes;
        private readonly int m_offsetT0;
        private readonly int m_offsetT1;
        private readonly int m_offsetT2;
        private readonly int m_offsetT3;
        private readonly int m_offsetT4;
        private readonly int m_entitySizeOf;
        
        private readonly IntPtr m_body;
        private readonly int* m_len;

        internal EcsEntityWriter(int capacity, int rowSizeInBytes, int offsetT0, int offsetT1, int offsetT2, int offsetT3, int offsetT4)
        {
            m_capacity = capacity;
            m_rowSizeInBytes = rowSizeInBytes;
            m_offsetT0 = offsetT0;
            m_offsetT1 = offsetT1;
            m_offsetT2 = offsetT2;
            m_offsetT3 = offsetT3;
            m_offsetT4 = offsetT4;
            m_entitySizeOf = MemoryUtil.SizeOf<EcsEntity>();

            m_body = MemoryUtil.Alloc((uint)(capacity * rowSizeInBytes));
            m_len = (int*)MemoryUtil.Alloc<int>();
        }
        
        public void Write(T0 comp0, T1 comp1, T2 comp2, T3 comp3, T4 comp4)
        {
            int len = (*m_len)++;
            if (len >= m_capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(Write));
            }
            
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT0, comp0);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT1, comp1);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT2, comp2);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT3, comp3);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT4, comp4);
        }

        public void CopyRow(int index, IntPtr to, int toIndex)
        {
            void* src = (void*)(m_body + index * m_rowSizeInBytes);
            void* dest = (void*)(to + toIndex * m_rowSizeInBytes);
            int sizeInBytes = m_rowSizeInBytes - m_entitySizeOf;
            Buffer.MemoryCopy(src, dest, sizeInBytes, sizeInBytes);
        }

        public void Dispose()
        {
            MemoryUtil.Free(m_body);
            MemoryUtil.Free((IntPtr)m_len);
        }
    }
    
    public readonly unsafe struct EcsEntityWriter<T0, T1, T2, T3, T4, T5> : IDisposable
        where T0 : struct, IEcsComponent
        where T1 : struct, IEcsComponent
        where T2 : struct, IEcsComponent
        where T3 : struct, IEcsComponent
        where T4 : struct, IEcsComponent
        where T5 : struct, IEcsComponent
    {
        public int length => *m_len;
        
        private readonly int m_capacity;
        private readonly int m_rowSizeInBytes;
        private readonly int m_offsetT0;
        private readonly int m_offsetT1;
        private readonly int m_offsetT2;
        private readonly int m_offsetT3;
        private readonly int m_offsetT4;
        private readonly int m_offsetT5;
        private readonly int m_entitySizeOf;
        
        private readonly IntPtr m_body;
        private readonly int* m_len;

        internal EcsEntityWriter(int capacity, int rowSizeInBytes, int offsetT0, int offsetT1, int offsetT2, int offsetT3, int offsetT4, int offsetT5)
        {
            m_capacity = capacity;
            m_rowSizeInBytes = rowSizeInBytes;
            m_offsetT0 = offsetT0;
            m_offsetT1 = offsetT1;
            m_offsetT2 = offsetT2;
            m_offsetT3 = offsetT3;
            m_offsetT4 = offsetT4;
            m_offsetT5 = offsetT5;
            m_entitySizeOf = MemoryUtil.SizeOf<EcsEntity>();

            m_body = MemoryUtil.Alloc((uint)(capacity * rowSizeInBytes));
            m_len = (int*)MemoryUtil.Alloc<int>();
        }
        
        public void Write(T0 comp0, T1 comp1, T2 comp2, T3 comp3, T4 comp4, T5 comp5)
        {
            int len = (*m_len)++;
            if (len >= m_capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(Write));
            }
            
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT0, comp0);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT1, comp1);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT2, comp2);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT3, comp3);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT4, comp4);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT5, comp5);
        }

        public void CopyRow(int index, IntPtr to, int toIndex)
        {
            void* src = (void*)(m_body + index * m_rowSizeInBytes);
            void* dest = (void*)(to + toIndex * m_rowSizeInBytes);
            int sizeInBytes = m_rowSizeInBytes - m_entitySizeOf;
            Buffer.MemoryCopy(src, dest, sizeInBytes, sizeInBytes);
        }

        public void Dispose()
        {
            MemoryUtil.Free(m_body);
            MemoryUtil.Free((IntPtr)m_len);
        }
    }
    
    public readonly unsafe struct EcsEntityWriter<T0, T1, T2, T3, T4, T5, T6, T7> : IDisposable
        where T0 : struct, IEcsComponent
        where T1 : struct, IEcsComponent
        where T2 : struct, IEcsComponent
        where T3 : struct, IEcsComponent
        where T4 : struct, IEcsComponent
        where T5 : struct, IEcsComponent
        where T6 : struct, IEcsComponent
        where T7 : struct, IEcsComponent
    {
        public int length => *m_len;
        
        private readonly int m_capacity;
        private readonly int m_rowSizeInBytes;
        private readonly int m_offsetT0;
        private readonly int m_offsetT1;
        private readonly int m_offsetT2;
        private readonly int m_offsetT3;
        private readonly int m_offsetT4;
        private readonly int m_offsetT5;
        private readonly int m_offsetT6;
        private readonly int m_offsetT7;
        private readonly int m_entitySizeOf;
        
        private readonly IntPtr m_body;
        private readonly int* m_len;

        internal EcsEntityWriter(int capacity, int rowSizeInBytes, int offsetT0, int offsetT1, int offsetT2, int offsetT3, int offsetT4, int offsetT5, int offsetT6, int offsetT7)
        {
            m_capacity = capacity;
            m_rowSizeInBytes = rowSizeInBytes;
            m_offsetT0 = offsetT0;
            m_offsetT1 = offsetT1;
            m_offsetT2 = offsetT2;
            m_offsetT3 = offsetT3;
            m_offsetT4 = offsetT4;
            m_offsetT5 = offsetT5;
            m_offsetT6 = offsetT6;
            m_offsetT7 = offsetT7;
            m_entitySizeOf = MemoryUtil.SizeOf<EcsEntity>();

            m_body = MemoryUtil.Alloc((uint)(capacity * rowSizeInBytes));
            m_len = (int*)MemoryUtil.Alloc<int>();
        }
        
        public void Write(T0 comp0, T1 comp1, T2 comp2, T3 comp3, T4 comp4, T5 comp5, T6 comp6, T7 comp7)
        {
            int len = (*m_len)++;
            if (len >= m_capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(Write));
            }
            
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT0, comp0);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT1, comp1);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT2, comp2);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT3, comp3);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT4, comp4);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT5, comp5);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT6, comp6);
            MemoryUtil.Write(m_body, len * m_rowSizeInBytes + m_offsetT7, comp7);
        }

        public void CopyRow(int index, IntPtr to, int toIndex)
        {
            void* src = (void*)(m_body + index * m_rowSizeInBytes);
            void* dest = (void*)(to + toIndex * m_rowSizeInBytes);
            int sizeInBytes = m_rowSizeInBytes - m_entitySizeOf;
            Buffer.MemoryCopy(src, dest, sizeInBytes, sizeInBytes);
        }

        public void Dispose()
        {
            MemoryUtil.Free(m_body);
            MemoryUtil.Free((IntPtr)m_len);
        }
    }
}