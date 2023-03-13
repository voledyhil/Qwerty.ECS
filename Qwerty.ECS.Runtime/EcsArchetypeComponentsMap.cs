
// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
	public unsafe struct EcsArchetypeComponentsMap
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
		private int* m_freeList;
		private int* m_freeCount;

		private const int Lower31BitMask = 0x7FFFFFFF;
		
		public void Alloc(int capacity)
		{
			m_freeList = (int*)MemoryUtilities.Alloc<int>(1);
			m_freeCount = (int*)MemoryUtilities.Alloc<int>(1);
			m_count = (int*)MemoryUtilities.Alloc<int>(1);
			*m_freeList = -1;
			
			m_buckets = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
			m_buckets->Realloc<int>(capacity);
			for (int i = 0; i < capacity; i++)
			{
				m_buckets->Write(i, -1);
			}
			
			m_entries = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
			m_entries->Realloc<Entry>(capacity);
		}
		
		public void Dispose()
		{
			MemoryUtilities.Free((IntPtr)m_count);
			MemoryUtilities.Free((IntPtr)m_freeList);
			MemoryUtilities.Free((IntPtr)m_freeCount);
			
			m_buckets->Dispose();
			m_entries->Dispose();
		}
		
		public int Get(int key)
		{
			int index = FindEntry(key);
			return m_entries->Read<Entry>(index).value;
		}

		public void Set(int key, int value)
		{
			int index = FindEntry(key);
			if (index >= 0)
			{
				m_entries->Get<Entry>(index).value = value;
				return;
			}
			
			if (*m_freeCount > 0)
			{
				index = *m_freeList;
				*m_freeList = m_entries->Read<Entry>(index).next;
				--*m_freeCount;
			}
			else
			{
				int entLen = m_entries->Length;
				if (*m_count >= entLen)
				{
					throw new ArgumentOutOfRangeException();
				}
				index = (*m_count)++;
			}

			int hashCode = key.GetHashCode() & Lower31BitMask;
			int bucketsLen = m_buckets->Length;
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
			int bucketsLen = m_buckets->Length;
			int target = hashCode % bucketsLen;
			
			for (int i = m_buckets->Read<int>(target); i >= 0; i = m_entries->Read<Entry>(i).next)
			{
				Entry entry = m_entries->Read<Entry>(i);
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