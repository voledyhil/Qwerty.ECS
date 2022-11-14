using Qwerty.Unsafe.Runtime;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Components
{
	public readonly struct EcsComponentAccessor<T> where T : struct, IEcsComponent
	{
		private readonly unsafe UnsafeArray* m_buckets;
		internal unsafe EcsComponentAccessor(UnsafeArray* buckets)
		{
			m_buckets = buckets;
		}

		public unsafe T this[in EcsEntity entity]
		{
			get => m_buckets->Get<UnsafeArray>(entity.Index / EcsComponentPool<T>.BucketSize)
				.Read<T>(entity.Index % EcsComponentPool<T>.BucketSize);
			set => m_buckets->Get<UnsafeArray>(entity.Index / EcsComponentPool<T>.BucketSize)
				.Write(entity.Index % EcsComponentPool<T>.BucketSize, value);
		}
	}
}