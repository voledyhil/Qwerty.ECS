using NUnit.Framework;
using Qwerty.ECS.Runtime;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Tests
{
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
        private EcsEntity m_b1;
        private EcsEntity m_b2;
        private EcsEntity m_b3;
        private EcsEntity m_b4;
        private EcsEntity m_b5;

        [SetUp]
        public void InitWorld()
        {
            EcsComponentType<ComponentA>.Register();
            EcsComponentType<ComponentB>.Register();
            EcsComponentType<ComponentC>.Register();
            EcsComponentType<ComponentD>.Register();
            EcsComponentType<ComponentE>.Register();

            m_world = new EcsWorld(new EcsWorldSetting {archetypeChunkSizeInByte = 32});
            m_abd = m_world.CreateEntity(new ComponentA { value = 1 }, new ComponentB { value = 2 }, new ComponentD { value = 3 });
            m_ac = m_world.CreateEntity(new ComponentA { value = 4 }, new ComponentC { value = 5 });
            m_bd1 = m_world.CreateEntity(new ComponentB { value = 6 }, new ComponentD { value = 7 });
            m_bd2 = m_world.CreateEntity(new ComponentD { value = 8 }, new ComponentB { value = 9 });
            m_bc = m_world.CreateEntity(new ComponentC { value = 10 }, new ComponentB { value = 11 });
            m_ab = m_world.CreateEntity(new ComponentB { value = 12 }, new ComponentA { value = 13 });
            m_ad = m_world.CreateEntity(new ComponentA { value = 14 }, new ComponentD { value = 15 });
            
            m_b1 = m_world.CreateEntity(new ComponentB { value = 16 });
            m_b2 = m_world.CreateEntity(new ComponentB { value = 17 });
            m_b3 = m_world.CreateEntity(new ComponentB { value = 18 });
            m_b4 = m_world.CreateEntity(new ComponentB { value = 19 });
            m_b5 = m_world.CreateEntity(new ComponentB { value = 20 });
        }

        [TearDown]
        public void CleanWorld()
        {
            m_world.Dispose();
        }

        [Test]
        public void FilterAllTest()
        {
            Assert.AreEqual(1, new HashSet<byte> {0}.Intersect(new EcsFilter().AllOf<ComponentA>().all).Count());
            Assert.AreEqual(1, new HashSet<byte> {0}.Intersect(new EcsFilter().AllOf<ComponentA, ComponentA>().all).Count());
            
            Assert.AreEqual(2, new HashSet<byte> {0, 1}.Intersect(new EcsFilter().AllOf<ComponentA, ComponentB>().all).Count());
            Assert.AreEqual(2, new HashSet<byte> {0, 1}.Intersect(new EcsFilter().AllOf<ComponentA, ComponentB, ComponentB>().all).Count());
            
            Assert.AreEqual(3, new HashSet<byte> {0, 1, 2}.Intersect(new EcsFilter().AllOf<ComponentA, ComponentB, ComponentC>().all).Count());
            Assert.AreEqual(3, new HashSet<byte> {0, 1, 2}.Intersect(new EcsFilter().AllOf<ComponentA, ComponentB, ComponentC, ComponentC>().all).Count());
            
            Assert.AreEqual(4, new HashSet<byte> {0, 1, 2, 3}.Intersect(new EcsFilter().AllOf<ComponentA, ComponentB, ComponentC, ComponentD>().all).Count());
            Assert.AreEqual(4, new HashSet<byte> {0, 1, 2, 3}.Intersect(new EcsFilter().AllOf<ComponentA, ComponentB, ComponentC, ComponentD, ComponentD>().all).Count());
            
            Assert.AreEqual(5, new HashSet<byte> {0, 1, 2, 3, 4}.Intersect(new EcsFilter().AllOf<ComponentA, ComponentB, ComponentC, ComponentD, ComponentE>().all).Count());
            Assert.AreEqual(5, new HashSet<byte> {0, 1, 2, 3, 4}.Intersect(new EcsFilter().AllOf<ComponentA, ComponentB, ComponentC, ComponentD, ComponentE, ComponentE>().all).Count());
        }
        
        [Test]
        public void FilterAnyTest()
        {
            Assert.AreEqual(1, new HashSet<byte> {0}.Intersect(new EcsFilter().AnyOf<ComponentA>().any).Count());
            Assert.AreEqual(1, new HashSet<byte> {0}.Intersect(new EcsFilter().AnyOf<ComponentA, ComponentA>().any).Count());
            
            Assert.AreEqual(2, new HashSet<byte> {0, 1}.Intersect(new EcsFilter().AnyOf<ComponentA, ComponentB>().any).Count());
            Assert.AreEqual(2, new HashSet<byte> {0, 1}.Intersect(new EcsFilter().AnyOf<ComponentA, ComponentB, ComponentB>().any).Count());
        }
        
        [Test]
        public void FilterNoneTest()
        {
            Assert.AreEqual(1, new HashSet<byte> {0}.Intersect(new EcsFilter().NoneOf<ComponentA>().none).Count());
            Assert.AreEqual(1, new HashSet<byte> {0}.Intersect(new EcsFilter().NoneOf<ComponentA, ComponentA>().none).Count());
            
            Assert.AreEqual(2, new HashSet<byte> {0, 1}.Intersect(new EcsFilter().NoneOf<ComponentA, ComponentB>().none).Count());
            Assert.AreEqual(2, new HashSet<byte> {0, 1}.Intersect(new EcsFilter().NoneOf<ComponentA, ComponentB, ComponentB>().none).Count());
        }

        [Test]
        public void FilterAllEqualsTest()
        {
            Assert.IsFalse(new EcsFilter().AllOf<ComponentA, ComponentB>().Equals(null));
            Assert.IsFalse(new EcsFilter().AllOf<ComponentA, ComponentB>().Equals(new object()));
            Assert.AreNotEqual(new EcsFilter().AllOf<ComponentA, ComponentB>(), null);
            Assert.AreNotEqual(new EcsFilter().AllOf<ComponentA, ComponentB>(), new object());
            Assert.AreNotEqual(new EcsFilter().AllOf<ComponentA, ComponentB>(), new EcsFilter().AllOf<ComponentA>());
            
            Assert.IsTrue(new EcsFilter().AllOf<ComponentA, ComponentB>().Equals(new EcsFilter().AllOf<ComponentA, ComponentB>()));
            Assert.AreEqual(new EcsFilter().AllOf<ComponentA, ComponentB>(), new EcsFilter().AllOf<ComponentA, ComponentB>());
            Assert.AreEqual(new EcsFilter().AllOf<ComponentA, ComponentB>(), new EcsFilter().AllOf<ComponentA, ComponentB, ComponentB>());
        }
        
        [Test]
        public void FilterAnyEqualsTest()
        {
            Assert.IsFalse(new EcsFilter().AnyOf<ComponentA, ComponentB>().Equals(null));
            Assert.IsFalse(new EcsFilter().AnyOf<ComponentA, ComponentB>().Equals(new object()));
            Assert.AreNotEqual(new EcsFilter().AnyOf<ComponentA, ComponentB>(), null);
            Assert.AreNotEqual(new EcsFilter().AnyOf<ComponentA, ComponentB>(), new object());
            Assert.AreNotEqual(new EcsFilter().AnyOf<ComponentA, ComponentB>(), new EcsFilter().AnyOf<ComponentA>());
            
            Assert.IsTrue(new EcsFilter().AnyOf<ComponentA, ComponentB>().Equals(new EcsFilter().AnyOf<ComponentA, ComponentB>()));
            Assert.AreEqual(new EcsFilter().AnyOf<ComponentA, ComponentB>(), new EcsFilter().AnyOf<ComponentA, ComponentB>());
            Assert.AreEqual(new EcsFilter().AnyOf<ComponentA, ComponentB>(), new EcsFilter().AnyOf<ComponentA, ComponentB, ComponentB>());
        }
        
        [Test]
        public void FilterNoneEqualsTest()
        {
            Assert.IsFalse(new EcsFilter().NoneOf<ComponentA, ComponentB>().Equals(null));
            Assert.IsFalse(new EcsFilter().NoneOf<ComponentA, ComponentB>().Equals(new object()));
            Assert.AreNotEqual(new EcsFilter().NoneOf<ComponentA, ComponentB>(), null);
            Assert.AreNotEqual(new EcsFilter().NoneOf<ComponentA, ComponentB>(), new object());
            Assert.AreNotEqual(new EcsFilter().NoneOf<ComponentA, ComponentB>(), new EcsFilter().NoneOf<ComponentA>());
            
            Assert.IsTrue(new EcsFilter().NoneOf<ComponentA, ComponentB>().Equals(new EcsFilter().NoneOf<ComponentA, ComponentB>()));
            Assert.AreEqual(new EcsFilter().NoneOf<ComponentA, ComponentB>(), new EcsFilter().NoneOf<ComponentA, ComponentB>());
            Assert.AreEqual(new EcsFilter().NoneOf<ComponentA, ComponentB>(), new EcsFilter().NoneOf<ComponentA, ComponentB, ComponentB>());
        }

        [Test]
        public void All_ComponentB_Test()
        {
            int sumB = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity> { m_abd, m_bd1, m_bd2, m_bc, m_ab, m_b1, m_b2, m_b3, m_b4, m_b5 };

            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB>());
            EcsArchetypeAccessor archetypeAccessor = archetypeGroup.GetEntityAccessor();
            foreach (EcsArchetypeChunkAccessor chunk in archetypeAccessor)
            {
                EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsArchetypeComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor<ComponentB>();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumB += compsB[i].value;
                    actualEntities.Add(e);
                }
            }

            Assert.AreEqual(sumB, 130);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }

        [Test]
        public void All_ComponentB_And_ComponentD_Test()
        {
            int sumB = 0;
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity> { m_abd, m_bd1, m_bd2 };

            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB, ComponentD>());

            EcsArchetypeAccessor archetypeAccessor = archetypeGroup.GetEntityAccessor();
            foreach (EcsArchetypeChunkAccessor chunk in archetypeAccessor)
            {
                EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsArchetypeComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor<ComponentB>();
                EcsArchetypeComponentAccessor<ComponentD> compsD = chunk.GetComponentAccessor<ComponentD>();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumB += compsB[i].value;
                    sumD += compsD[i].value;
                    actualEntities.Add(e);
                }
            }

            Assert.AreEqual(sumB, 17);
            Assert.AreEqual(sumD, 18);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }

        [Test]
        public void Any_ComponentB_Test()
        {
            int sumB = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd, m_bd1, m_bd2, m_bc, m_ab, m_b1, m_b2, m_b3, m_b4, m_b5 };
        
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AnyOf<ComponentB>());
            EcsArchetypeAccessor archetypeAccessor = archetypeGroup.GetEntityAccessor();
            foreach (EcsArchetypeChunkAccessor chunk in archetypeAccessor)
            {
                EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsArchetypeComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor<ComponentB>();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumB += compsB[i].value;
                    actualEntities.Add(e);
                }
            }
        
            Assert.AreEqual(sumB, 130);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void Any_ComponentB_Or_ComponentD_Test()
        {
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd, m_bd1, m_bd2, m_bc, m_ab, m_ad, m_b1, m_b2, m_b3, m_b4, m_b5 };
        
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AnyOf<ComponentB, ComponentD>());
            EcsArchetypeAccessor archetypeAccessor = archetypeGroup.GetEntityAccessor();
            foreach (EcsArchetypeChunkAccessor chunk in archetypeAccessor)
            {
                EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    actualEntities.Add(e);
                }
            }
        
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void None_ComponentB_And_ComponentD_Test()
        {
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_ac };
        
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().NoneOf<ComponentB, ComponentD>());
            EcsArchetypeAccessor archetypeAccessor = archetypeGroup.GetEntityAccessor();
            foreach (EcsArchetypeChunkAccessor chunk in archetypeAccessor)
            {
                EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    actualEntities.Add(e);
                }
            }
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void None_DoubleComponentB_And_ComponentD_Test()
        {
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_ac };
        
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().NoneOf<ComponentB, ComponentB, ComponentD>());
            EcsArchetypeAccessor archetypeAccessor = archetypeGroup.GetEntityAccessor();
            foreach (EcsArchetypeChunkAccessor chunk in archetypeAccessor)
            {
                EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    actualEntities.Add(e);
                }
            }
        
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void All_DoubleComponentB_And_ComponentD_AnyFilter_ComponentA_Test()
        {
            int sumA = 0;
            int sumB = 0;
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd };
        
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB, ComponentB, ComponentD>().AnyOf<ComponentA>());
            EcsArchetypeAccessor archetypeAccessor = archetypeGroup.GetEntityAccessor();
            foreach (EcsArchetypeChunkAccessor chunk in archetypeAccessor)
            {
                EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsArchetypeComponentAccessor<ComponentA> compsA = chunk.GetComponentAccessor<ComponentA>();
                EcsArchetypeComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor<ComponentB>();
                EcsArchetypeComponentAccessor<ComponentD> compsD = chunk.GetComponentAccessor<ComponentD>();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumA += compsA[i].value;
                    sumB += compsB[i].value;
                    sumD += compsD[i].value;
                    actualEntities.Add(e);
                }
            }
        
            Assert.AreEqual(sumA, 1);
            Assert.AreEqual(sumB, 2);
            Assert.AreEqual(sumD, 3);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void All_DoubleComponentD_AnyFilter_ComponentB_DoubleComponentC_Test()
        {
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd, m_bd1, m_bd2};
        
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentD, ComponentD>().AnyOf<ComponentB, ComponentC, ComponentC>());
            EcsArchetypeAccessor archetypeAccessor = archetypeGroup.GetEntityAccessor();
            foreach (EcsArchetypeChunkAccessor chunk in archetypeAccessor)
            {
                EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsArchetypeComponentAccessor<ComponentD> compsD = chunk.GetComponentAccessor<ComponentD>();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumD += compsD[i].value;
                    actualEntities.Add(e);
                }
            }
            
            Assert.AreEqual(sumD, 18);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void All_ComponentB_None_ComponentA_Test()
        {
            int sumB = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_bd1, m_bd2, m_bc, m_b1, m_b2, m_b3, m_b4, m_b5 };
        
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB>().NoneOf<ComponentA>());
            EcsArchetypeAccessor archetypeAccessor = archetypeGroup.GetEntityAccessor();
            foreach (EcsArchetypeChunkAccessor chunk in archetypeAccessor)
            {
                EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsArchetypeComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor<ComponentB>();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumB += compsB[i].value;
                    actualEntities.Add(e);
                }
            }
            
            Assert.AreEqual(sumB, 116
            );
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void All_ComponentB_And_ComponentD_None_ComponentA_Test()
        {
            int sumB = 0;
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_bd1, m_bd2 };
        
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB, ComponentD>().NoneOf<ComponentA>());
            EcsArchetypeAccessor archetypeAccessor = archetypeGroup.GetEntityAccessor();
            foreach (EcsArchetypeChunkAccessor chunk in archetypeAccessor)
            {
                EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsArchetypeComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor<ComponentB>();
                EcsArchetypeComponentAccessor<ComponentD> compsD = chunk.GetComponentAccessor<ComponentD>();
                
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumB += compsB[i].value;
                    sumD += compsD[i].value;
                    actualEntities.Add(e);
                }
            }
        
            Assert.AreEqual(sumB, 15);
            Assert.AreEqual(sumD, 15);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
    }
}