using System.Runtime.CompilerServices;
using Qwerty.Unsafe.Runtime;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Components
{
	public struct EcsComponentAccessor<T> where T : struct, IEcsComponent
	{
		private readonly unsafe UnsafeArray* m_buckets;
		private UnsafeArray m_lastBucket;
		private int m_lastBucketIndex;

		internal unsafe EcsComponentAccessor(UnsafeArray* buckets)
		{
			m_buckets = buckets;
			m_lastBucket = default;
			m_lastBucketIndex = -1;
		}

		public T this[in EcsEntity entity]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				FindBucket(entity.Index);
				return m_lastBucket.Read<T>(entity.Index % EcsComponentPool<T>.BucketSize);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				FindBucket(entity.Index);
				m_lastBucket.Write(entity.Index % EcsComponentPool<T>.BucketSize, value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe void FindBucket(int entityIndex)
		{
			int bucketIndex = entityIndex / EcsComponentPool<T>.BucketSize;
			if (bucketIndex == m_lastBucketIndex)
			{
				return;
			}

			m_lastBucket = m_buckets->Get<UnsafeArray>(bucketIndex);
			m_lastBucketIndex = bucketIndex;
		}
	}
}