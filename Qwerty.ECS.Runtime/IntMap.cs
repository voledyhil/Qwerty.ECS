
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
			m_count = (int*)MemoryUtil.Alloc<int>();
			
			m_entries = (UnsafeArray*)MemoryUtil.Alloc<UnsafeArray>();
			m_entries->Alloc<Entry>(capacity);
			
			m_buckets = (UnsafeArray*)MemoryUtil.Alloc<UnsafeArray>();
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
			return TryGetValue(key, out _);
		}
		
		public int Get(int key)
		{
			if (TryGetValue(key, out int value))
			{
				return value;
			}
			throw new KeyNotFoundException(nameof(Get));
		}
		
		public void Set(int key, int value)
		{
			int index = (*m_count)++;
			int hashCode = key & Lower31BitMask;
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

		private bool TryGetValue(int key, out int value)
		{
			value = 0;
			int hashCode = key & Lower31BitMask;
			int bucketsLen = m_buckets->length;
			int target = hashCode % bucketsLen;
			int index = m_buckets->Get<int>(target);
			while (index >= 0)
			{
				Entry entry = m_entries->Get<Entry>(index);
				if (entry.hashCode != hashCode || entry.key != key)
				{
					index = entry.next;
					continue;
				}
				value = entry.value;
				return true;
			}
			return false;
		}
	}
}