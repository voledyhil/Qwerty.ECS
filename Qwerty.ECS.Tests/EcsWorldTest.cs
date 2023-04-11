using NUnit.Framework;
using Qwerty.ECS.Runtime;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Tests
{
    public struct ComponentA : IEcsComponent
    {
        public int value;
    }

    public struct ComponentB : IEcsComponent
    {
        public int value;
    }

    public struct ComponentC : IEcsComponent
    {
        public int value;
    }

    public struct ComponentD : IEcsComponent
    {
        public int value;
    }

    public struct ComponentE : IEcsComponent
    {
        public int value;
    }
    
    public struct ComponentF : IEcsComponent
    {
        public int value;
    }


    
    public readonly ref struct TypeIndex<T> where T : struct
    {
        internal static short typeIndex;
        internal static bool isRegister;
        static TypeIndex()
        {
            typeIndex = -1;
        }
            
        internal static TypeIndex<T> value
        {
            get
            {
                if (!isRegister)
                {
                    throw new InvalidOperationException(nameof(value));
                }
                return new TypeIndex<T>(typeIndex);
            }
        }

        public readonly short index;
        private TypeIndex(short index)
        {
            this.index = index;
        }
    }
    
    public static class TypeManager
    {
        private static readonly HashSet<short> Hashes = new HashSet<short>();
        public static void Register<T>(string key) where T : struct
        {
            short hash = GenerateHash(key);
            if (Hashes.Contains(hash))
            {
                throw new ArgumentException(nameof(Register));
            }
            Hashes.Add(hash);
            TypeIndex<T>.typeIndex = hash;
            TypeIndex<T>.isRegister = true;
        }

        public static short GetIndex<T>() where T : struct
        {
            return TypeIndex<T>.value.index;
        }
        
        private const int Lower31BitMask = 0x7FFFFFFF;
        private static short GenerateHash(string key)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < key.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ key[i];
                    if (i == key.Length - 1) break;
                    hash2 = ((hash2 << 5) + hash2) ^ key[i + 1];
                }
                return (short)(((hash1 + hash2 * 1566083941) & Lower31BitMask) % short.MaxValue);
            }
        }
    }

    
    public struct T1
    {
        
    }

    public struct T2
    {
        
    }

    [TestFixture]
    public partial class EcsWorldTest
    {
        [Test]
        public void Test()
        {
            TypeManager.Register<T1>("t1");
            TypeManager.Register<T2>("t2");
            
            Assert.AreNotEqual(TypeManager.GetIndex<T2>(), -1);
            Assert.AreNotEqual(TypeManager.GetIndex<T1>(), -1);
        }
        
        [Test]
        public void RegisterComponentThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 32, entitiesCapacity = 32});
            Assert.That(() => world.CreateEntity(new ComponentF()), Throws.InvalidOperationException);
            world.Dispose();
        }

        [Test]
        public void EntityEqualsTest()
        {
            Assert.AreEqual(EcsEntity.Null, EcsEntity.Null);
            Assert.IsTrue(EcsEntity.Null == EcsEntity.Null);
            Assert.IsTrue(EcsEntity.Null.Equals(EcsEntity.Null));
            Assert.IsTrue(EcsEntity.Null.Equals((object)EcsEntity.Null));
            
            Assert.AreNotEqual(EcsEntity.Null, new EcsEntity(1, 1));
            Assert.AreNotEqual(EcsEntity.Null, new EcsEntity(0, 1));
            Assert.AreNotEqual(EcsEntity.Null, new EcsEntity(1, 0));
            Assert.IsTrue(EcsEntity.Null != new EcsEntity(1, 1));
            Assert.IsTrue(EcsEntity.Null != new EcsEntity(0, 1));
            Assert.IsTrue(EcsEntity.Null != new EcsEntity(1, 0));
            
            Assert.IsFalse(EcsEntity.Null.Equals(new object()));
        }
        
        [Test]
        public void EntityIncVersionTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 32, entitiesCapacity = 32});
            
            EcsEntity e1 = world.CreateEntity();
            Assert.AreEqual(e1.Index, 1);
            Assert.AreEqual(e1.Version, 1);
            
            EcsEntity e2 = world.CreateEntity();
            Assert.AreEqual(e2.Index, 2);
            Assert.AreEqual(e2.Version, 1);
            
            EcsEntity e3 = world.CreateEntity();
            Assert.AreEqual(e3.Index, 3);
            Assert.AreEqual(e3.Version, 1);
            
            world.DestroyEntity(e1);
            e1 = world.CreateEntity();
            Assert.AreEqual(e1.Index, 1);
            Assert.AreEqual(e1.Version, 2);
            
            world.Dispose();
        }

        [Test]
        public void CreateEntityTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 32, entitiesCapacity = 32});
            EcsEntity e = world.CreateEntity(new ComponentA(), new ComponentB(), new ComponentC(), new ComponentD());
            
            Assert.IsTrue(world.HasComponent<ComponentA>(e));
            Assert.IsTrue(world.HasComponent<ComponentB>(e));
            Assert.IsTrue(world.HasComponent<ComponentC>(e));
            Assert.IsTrue(world.HasComponent<ComponentD>(e));
            
            world.Dispose();
        }

        [Test]
        public void CreateEntityThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 32, entitiesCapacity = 32});
            Assert.That(() => world.CreateEntity(new ComponentA(), new ComponentA()), Throws.ArgumentException);
            Assert.That(() => world.CreateEntity(new ComponentA(), new ComponentB(), new ComponentB()), Throws.ArgumentException);
            Assert.That(() => world.CreateEntity(new ComponentA(), new ComponentB(), new ComponentC(), new ComponentC()), Throws.ArgumentException);
            world.Dispose();
        }
        
        [Test]
        public void RemoveComponentTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 32, entitiesCapacity = 32});
            EcsEntity e = world.CreateEntity();
            
            Assert.IsFalse(world.HasComponent<ComponentA>(e));
            
            world.AddComponent(e, new ComponentA());
            Assert.IsTrue(world.HasComponent<ComponentA>(e));
            
            world.RemoveComponent<ComponentA>(e);
            Assert.IsFalse(world.HasComponent<ComponentA>(e));
            
            world.Dispose();
        }
        
        [Test]
        public void GetComponentThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 32, entitiesCapacity = 32});
            EcsEntity e  = world.CreateEntity();
            Assert.That(() => world.GetComponent<ComponentA>(e), Throws.InvalidOperationException);
            
            world.Dispose();
        }

        [Test]
        public void AddComponentThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 32, entitiesCapacity = 32});
            EcsEntity e  = world.CreateEntity();
            world.AddComponent(e, new ComponentA());

            Assert.That(() => world.AddComponent(e, new ComponentA()), Throws.InvalidOperationException);
            
            world.Dispose();
        }

        [Test]
        public void RemoveComponentThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 32, entitiesCapacity = 32});
            EcsEntity e = world.CreateEntity();

            Assert.That(() => world.RemoveComponent<ComponentA>(e), Throws.InvalidOperationException);
            
            world.Dispose();
        }

        [Test]
        public void SetComponentTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 32, entitiesCapacity = 32});
            EcsEntity e = world.CreateEntity();
            
            world.AddComponent(e, new ComponentA());
            world.SetComponent(e, new ComponentA { value = 1 });
            
            Assert.AreEqual(1, world.GetComponent<ComponentA>(e).value);
            
            world.Dispose();
        }
        
        [Test]
        public void SetComponentThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 32, entitiesCapacity = 32});
            EcsEntity e = world.CreateEntity();
            Assert.That(() => world.SetComponent(e, new ComponentA()), Throws.InvalidOperationException);
            
            world.Dispose();
        }
        
        [Test]
        public void DestroyEntityThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 32, entitiesCapacity = 32});
            EcsEntity e = world.CreateEntity();
            world.DestroyEntity(e);

            Assert.That(() => world.DestroyEntity(e), Throws.InvalidOperationException);
            
            world.Dispose();
        }
        
    }
}