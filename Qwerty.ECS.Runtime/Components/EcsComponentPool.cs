// using System.Runtime.CompilerServices;
//
// // ReSharper disable once CheckNamespace
// namespace Qwerty.ECS.Runtime.Components
// {
// 	internal unsafe class EcsComponentPool<T> : IEcsComponentPool where T : struct, IEcsComponent 
// 	{
// 		public const int BucketSize = 1024;
//
// 		private readonly UnsafeArray* m_buckets;
// 		private EcsComponentAccessor<T> m_accessor;
//
// 		public EcsComponentPool(int capacity)
// 		{
// 			m_buckets = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
// 			m_buckets->Realloc<UnsafeArray>(capacity);
// 			m_accessor = new EcsComponentAccessor<T>(m_buckets);
// 		}
//
// 		public ref EcsComponentAccessor<T> GetAccessor()
// 		{
// 			return ref m_accessor;
// 		}
//
// 		[MethodImpl(MethodImplOptions.AggressiveInlining)]
// 		public T Read(int entityIndex)
// 		{
// 			return m_buckets->Get<UnsafeArray>(entityIndex / BucketSize).Read<T>(entityIndex % BucketSize);
// 		}
//
// 		[MethodImpl(MethodImplOptions.AggressiveInlining)]
// 		public void Write(int entityIndex, in T component)
// 		{
// 			int bucketIndex = entityIndex / BucketSize;
// 			int index = entityIndex % BucketSize;
// 				
// 			if (bucketIndex >= m_buckets->Length)
// 			{
// 				int capacity = 2 * bucketIndex + 1;
// 				m_buckets->Realloc<UnsafeArray>(capacity);
// 			}
//
// 			UnsafeArray bucket = m_buckets->Get<UnsafeArray>(entityIndex / BucketSize);
// 			if (bucket.Ptr == null)
// 			{
// 				UnsafeArray* newBucket = (UnsafeArray*)MemoryUtilities.Alloc<UnsafeArray>(1);
// 				newBucket->Realloc<T>(BucketSize);
//
// 				bucket = *newBucket;
// 				m_buckets->Write<UnsafeArray>(bucketIndex, bucket);
// 			}
// 				
// 			bucket.Write(index, component);
// 		}
//
// 		public void Dispose()
// 		{
// 			int capacity = m_buckets->Length;
// 			for (int i = 0; i < capacity; i++)
// 			{
// 				ref UnsafeArray bucket = ref m_buckets->Get<UnsafeArray>(i);
// 				if (bucket.Ptr == null)
// 				{
// 					continue;
// 				}
// 				bucket.Dispose();
// 			}
// 			m_buckets->Dispose();
// 		}
// 	}
// }