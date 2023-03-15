
// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
	internal unsafe struct IntMap
	{
		private struct Entry 
		{
			public int hashCode;    // Lower 31 bits of hash code, -1 if unused
			public int next;        // Index of next entry, -1 if last
			public int key;        // Key of entry
			public int value;    // Value of entry
		}
		
		private UnsafeArray* m_buckets;
		private UnsafeArray* m_entries;
		private int* m_count;
		private const int Lower31BitMask = 0x7FFFFFFF;
		
		public void Alloc(int capacity)
		{
			m_count = (int*)MemoryUtil.Alloc<int>(1);
			
			m_entries = (UnsafeArray*)MemoryUtil.Alloc<UnsafeArray>(1);
			m_entries->Alloc<Entry>(capacity);
			
			m_buckets = (UnsafeArray*)MemoryUtil.Alloc<UnsafeArray>(1);
			m_buckets->Alloc<int>(capacity);
			
			for (int i = 0; i < capacity; i++)
			{
				m_buckets->Write(i, -1);
			}
		}
		
		public void Dispose()
		{
			m_buckets->Dispose();
			m_entries->Dispose();
			
			MemoryUtil.Free((IntPtr)m_count);
			MemoryUtil.Free((IntPtr)m_buckets);
			MemoryUtil.Free((IntPtr)m_entries);
		}
		
		public bool Contains(int key)
		{
			return FindEntry(key) >= 0;
		}
		
		public int Get(int key)
		{
			return m_entries->Get<Entry>(FindEntry(key)).value;
		}
		
		public void Set(int key, int value)
		{
			int index = (*m_count)++;
			int hashCode = key.GetHashCode() & Lower31BitMask;
			int bucketsLen = m_buckets->length;
			int target = hashCode % bucketsLen;
			
			Entry entry = m_entries->Read<Entry>(index);
			entry.hashCode = hashCode;
			entry.next = m_buckets->Read<int>(target);
			entry.key = key;
			entry.value = value;
			
			m_entries->Write(index, entry);
			m_buckets->Write(target, index);
		}

		private int FindEntry(int key) 
		{
			int hashCode = key.GetHashCode() & Lower31BitMask;
			int bucketsLen = m_buckets->length;
			int target = hashCode % bucketsLen;
			
			for (int i = m_buckets->Get<int>(target); i >= 0; i = m_entries->Get<Entry>(i).next)
			{
				Entry entry = m_entries->Get<Entry>(i);
				if (entry.hashCode != hashCode || !entry.key.Equals(key))
				{
					continue;
				}
				return i;
			}
			return -1;
		}
	}
}