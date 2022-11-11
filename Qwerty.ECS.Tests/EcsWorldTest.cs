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

    [TestFixture]
    public partial class EcsWorldTest
    {
        private EcsWorld m_world;
        private EcsEntity m_ab;
        private EcsEntity m_abd;
        private EcsEntity m_ac;
        private EcsEntity m_ad;
        private EcsEntity m_bc;
        private EcsEntity m_bd1;
        private EcsEntity m_bd2;

        [SetUp]
        public void InitWorld()
        {
            EcsComponentType<ComponentA>.Register();
            EcsComponentType<ComponentB>.Register();
            EcsComponentType<ComponentC>.Register();
            EcsComponentType<ComponentD>.Register();

            m_world = new EcsWorld();
            m_abd = m_world.CreateEntity(new ComponentA { value = 1 }, new ComponentB { value = 2 }, new ComponentD { value = 3 });
            m_ac = m_world.CreateEntity(new ComponentA { value = 4 }, new ComponentC { value = 5 });
            m_bd1 = m_world.CreateEntity(new ComponentB { value = 6 }, new ComponentD { value = 7 });
            m_bd2 = m_world.CreateEntity(new ComponentD { value = 8 }, new ComponentB { value = 8 });
            m_bc = m_world.CreateEntity(new ComponentC { value = 9 }, new ComponentB { value = 10 });
            m_ab = m_world.CreateEntity(new ComponentB { value = 11 }, new ComponentA { value = 12 });
            m_ad = m_world.CreateEntity(new ComponentA { value = 13 }, new ComponentD { value = 14 });
        }

        [TearDown]
        public void CleanWorld()
        {
            m_world.Dispose();
        }

        [Test]
        public void EntityVersionTest()
        {
            EcsWorld world = new EcsWorld();
            
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
        }

        [Test]
        public void CreateEntityTest()
        {
            EcsWorld world = new EcsWorld();
            EcsEntity e = world.CreateEntity(new ComponentA(), new ComponentB(), new ComponentC(), new ComponentD());
            
            Assert.IsTrue(world.HasComponent<ComponentA>(e));
            Assert.IsTrue(world.HasComponent<ComponentB>(e));
            Assert.IsTrue(world.HasComponent<ComponentC>(e));
            Assert.IsTrue(world.HasComponent<ComponentD>(e));
        }
        
        [Test]
        public void RemoveComponentTest()
        {
            EcsWorld world = new EcsWorld();
            EcsEntity e = world.CreateEntity();
            
            Assert.IsFalse(world.HasComponent<ComponentA>(e));
            
            world.AddComponent(e, new ComponentA());
            Assert.IsTrue(world.HasComponent<ComponentA>(e));
            
            world.RemoveComponent<ComponentA>(e);
            Assert.IsFalse(world.HasComponent<ComponentA>(e));
        }
        
        [Test]
        public void GetComponentThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld();
            EcsEntity e  = world.CreateEntity();
            Assert.That(() => world.GetComponent<ComponentA>(e), Throws.InvalidOperationException);
        }

        [Test]
        public void AddComponentThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld();
            EcsEntity e  = world.CreateEntity();
            world.AddComponent(e, new ComponentA());

            Assert.That(() => world.AddComponent(e, new ComponentA()), Throws.InvalidOperationException);
        }

        [Test]
        public void RemoveComponentThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld();
            EcsEntity e = world.CreateEntity();

            Assert.That(() => world.RemoveComponent<ComponentA>(e), Throws.InvalidOperationException);
        }

        [Test]
        public void SetComponentTest()
        {
            EcsWorld world = new EcsWorld();
            EcsEntity e = world.CreateEntity();
            
            world.AddComponent(e, new ComponentA());
            world.SetComponent(e, new ComponentA { value = 1 });
            
            Assert.AreEqual(1, world.GetComponent<ComponentA>(e).value);
        }
        
        [Test]
        public void SetComponentThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld();
            EcsEntity e = world.CreateEntity();
            Assert.That(() => world.SetComponent(e, new ComponentA()), Throws.InvalidOperationException);
        }
        
        [Test]
        public void DestroyEntityThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld();
            EcsEntity e = world.CreateEntity();
            world.DestroyEntity(e);

            Assert.That(() => world.DestroyEntity(e), Throws.InvalidOperationException);
        }
        
    }
}