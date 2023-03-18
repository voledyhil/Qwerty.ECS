using System.Text;
using NUnit.Framework;
using Qwerty.ECS.Runtime;
using Qwerty.ECS.Runtime.Components;

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

    [TestFixture]
    public partial class EcsWorldTest
    {
        
        /*
            // x and y are ints
            long l = x;
            l = (l << 32) | y;
            And I'm getting the int values like so:

            x = (int) l >> 32;
            y = (int) l & 0xffffffff;
         */

        // private static long ToInt(int x, int y)
        // {
        //     
        // }


        [Test]
        public void ParseLongTest()
        {
            const int x = 1234;
            const int y = 5678;
            long hash = ToLong(x, y);

            FromLong(hash, out int xv, out int yv);
            
            Assert.AreEqual(x, xv);
            Assert.AreEqual(y, yv);
        }
        
        [Test]
        public void ParseIntTest()
        {
            const short x = 1234;
            const short y = 5678;
            int hash = ToInt(x, y);
            
            Assert.AreEqual(x, FromIntX(hash));
            Assert.AreEqual(y, FromIntY(hash));
            
        }

        private static long ToLong(int x, int y)
        {
            return ((long)x << 32) | (y & 0xFFFFFFFFL);
        }
        
        private static void FromLong(long value, out int x, out int y)
        {
            x = (int)(value >> 32);
            y = (int)(value & 0xFFFFFFFFL);
        }

        //[Test]
        public void IntMapTest()
        {
            Random r = new Random(100);
            
            const int componentCount = 20;
            Dictionary<int, int> s = new Dictionary<int, int>();
            for (int i = 1; i <= componentCount; i++)
            {
                s[i] = 0;
            }
            
            for (int j = 0; j < 10000; j++)
            {
                Dictionary<int, int> d = new Dictionary<int, int>(componentCount);
                for (int i = 0; i < componentCount;)
                {
                    int key = (int)(r.NextDouble() * (short.MaxValue - 1));
                    int value = (int)(r.NextDouble() * (short.MaxValue - 1));
        
                    if (d.ContainsKey(key))
                    {
                        continue;
                    }
        
                    d.Add(key, value);
                    i++;
                }
                
                
                int[] storage = new int[Length];
                foreach (KeyValuePair<int, int> item in d)
                {
                    Add(storage, (short)item.Key, (short)item.Value);
                }
                
                foreach (KeyValuePair<int, int> item in d)
                {
                    Assert.AreEqual(item.Value, Get(storage, (short)item.Key, out int iteration));
                    s[iteration]++;
                }
            }
            
            
        
            int sum = 0;
            foreach (KeyValuePair<int, int> item in s)
            {
                sum += item.Value;
            }
        
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<int, int> item in s)
            {
                builder.Append($"{item.Key}->{Math.Round(1.0 * item.Value / sum, 2)}; ");
            }
            Assert.AreEqual(0, componentCount, builder.ToString());
            
            
        }
        
        private const byte Length = 47;
        private static void Add(int[] storage, short key, short value)
        {
            short hash = (short)((key + 1) % Length);
            short curKey = FromIntX(storage[hash]);
            while (curKey > 0)
            {
                hash = (short)((hash + 1) % Length);
                curKey = FromIntX(storage[hash]);
            }
            storage[hash] = ToInt((short)(key + 1), value);
        }
        
        private int Get(int[] storage, short key, out int iteration)
        {
            iteration = 0;
            short hash = (short)((key + 1) % Length);
            short curKey = FromIntX(storage[hash]);
            while (curKey > 0)
            {
                iteration++;
                
                if (curKey == key + 1) return FromIntY(storage[hash]);
                hash = (short)((hash + 1) % Length);
                curKey = FromIntX(storage[hash]);
            }
            throw new ArgumentException(nameof(Get));
        }
        
        private static int ToInt(short x, short y)
        {
            return (x << 16) | (y & 0xFFFF);
        }
        
        private static void FromInt(int value, out short x, out short y)
        {
            x = (short)(value >> 16);
            y = (short)(value & 0xFFFF);
        }
        
        private static short FromIntX(int value)
        {
            return (short)(value >> 16);
        }
        
        private static short FromIntY(int value)
        {
            return (short)(value & 0xFFFF);
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