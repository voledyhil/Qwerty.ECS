using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Qwerty.ECS.Runtime;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Chunks;
using Qwerty.ECS.Runtime.Components;

// ReSharper disable once CheckNamespace
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
            EcsTypeManager.Register<ComponentA>("comp_a");
            EcsTypeManager.Register<ComponentA3>("comp_a_3");
            
            EcsTypeManager.Register<ComponentB>("comp_b");
            EcsTypeManager.Register<ComponentC>("comp_c");
            EcsTypeManager.Register<ComponentD>("comp_d");
            EcsTypeManager.Register<ComponentE>("comp_e");

            m_world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 32, entitiesCapacity = 32});
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
        public void All_ComponentB_With_Chunks_Test()
        {
            int sumB = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity> { m_abd, m_bd1, m_bd2, m_bc, m_ab, m_b1, m_b2, m_b3, m_b4, m_b5 };

            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor(compBTypeHandle);
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
        public void All_ComponentB_With_Components_Array_Test()
        {
            int sumB = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity> { m_abd, m_bd1, m_bd2, m_bc, m_ab, m_b1, m_b2, m_b3, m_b4, m_b5 };

            EcsComponentDataFromEntity<ComponentB> compsB = m_world.GetComponentDataFromEntityAccessor<ComponentB>();
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumB += compsB[e].value;
                    actualEntities.Add(e);
                }
            }

            Assert.AreEqual(sumB, 130);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }

        [Test]
        public void All_ComponentB_And_ComponentD_With_Chunks_Test()
        {
            int sumB = 0;
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity> { m_abd, m_bd1, m_bd2 };

            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            EcsComponentTypeHandle<ComponentD> compDTypeHandle = m_world.GetComponentTypeHandle<ComponentD>();
            
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB, ComponentD>());
            
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor(compBTypeHandle);
                EcsChunkComponentAccessor<ComponentD> compsD = chunk.GetComponentAccessor(compDTypeHandle);
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
        public void All_ComponentB_And_ComponentD_With_Components_Array_Test()
        {
            int sumB = 0;
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity> { m_abd, m_bd1, m_bd2 };
            
            EcsComponentDataFromEntity<ComponentB> compsB = m_world.GetComponentDataFromEntityAccessor<ComponentB>();
            EcsComponentDataFromEntity<ComponentD> compsD = m_world.GetComponentDataFromEntityAccessor<ComponentD>();
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB, ComponentD>());
            
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumB += compsB[e].value;
                    sumD += compsD[e].value;
                    actualEntities.Add(e);
                }
            }

            Assert.AreEqual(sumB, 17);
            Assert.AreEqual(sumD, 18);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }

        [Test]
        public void Any_ComponentB_With_Chunks_Test()
        {
            int sumB = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd, m_bd1, m_bd2, m_bc, m_ab, m_b1, m_b2, m_b3, m_b4, m_b5 };
        
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AnyOf<ComponentB>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor(compBTypeHandle);
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
        public void Any_ComponentB_With_Components_Array_Test()
        {
            int sumB = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd, m_bd1, m_bd2, m_bc, m_ab, m_b1, m_b2, m_b3, m_b4, m_b5 };
            
            EcsComponentDataFromEntity<ComponentB> compsB = m_world.GetComponentDataFromEntityAccessor<ComponentB>();
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AnyOf<ComponentB>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumB += compsB[e].value;
                    actualEntities.Add(e);
                }
            }

            Assert.AreEqual(sumB, 130);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void Any_ComponentB_Or_ComponentD_With_Chunks_Test()
        {
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd, m_bd1, m_bd2, m_bc, m_ab, m_ad, m_b1, m_b2, m_b3, m_b4, m_b5 };
        
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AnyOf<ComponentB, ComponentD>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    actualEntities.Add(e);
                }
            }
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }

        [Test]
        public void None_ComponentB_And_ComponentD_With_Chunks_Test()
        {
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_ac };
        
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().NoneOf<ComponentB, ComponentD>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    actualEntities.Add(e);
                }
            }
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void None_DoubleComponentB_And_ComponentD_With_Chunks_Test()
        {
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_ac };
        
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().NoneOf<ComponentB, ComponentB, ComponentD>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    actualEntities.Add(e);
                }
            }
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void All_DoubleComponentB_And_ComponentD_AnyFilter_ComponentA_With_Chunks_Test()
        {
            int sumA = 0;
            int sumB = 0;
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd };
            
            EcsComponentTypeHandle<ComponentA> compATypeHandle = m_world.GetComponentTypeHandle<ComponentA>();
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            EcsComponentTypeHandle<ComponentD> compDTypeHandle = m_world.GetComponentTypeHandle<ComponentD>();
        
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB, ComponentB, ComponentD>().AnyOf<ComponentA>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentA> compsA = chunk.GetComponentAccessor(compATypeHandle);
                EcsChunkComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor(compBTypeHandle);
                EcsChunkComponentAccessor<ComponentD> compsD = chunk.GetComponentAccessor(compDTypeHandle);
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
        public void All_DoubleComponentB_And_ComponentD_AnyFilter_ComponentA_With_Components_Array_Test()
        {
            int sumA = 0;
            int sumB = 0;
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd };
        
            EcsComponentDataFromEntity<ComponentA> compsA = m_world.GetComponentDataFromEntityAccessor<ComponentA>();
            EcsComponentDataFromEntity<ComponentB> compsB = m_world.GetComponentDataFromEntityAccessor<ComponentB>();
            EcsComponentDataFromEntity<ComponentD> compsD = m_world.GetComponentDataFromEntityAccessor<ComponentD>();
            
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB, ComponentB, ComponentD>().AnyOf<ComponentA>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumA += compsA[e].value;
                    sumB += compsB[e].value;
                    sumD += compsD[e].value;
                    actualEntities.Add(e);
                }
            }
            
            Assert.AreEqual(sumA, 1);
            Assert.AreEqual(sumB, 2);
            Assert.AreEqual(sumD, 3);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }

        [Test]
        public void All_DoubleComponentD_AnyFilter_ComponentB_DoubleComponentC_With_Chunks_Test()
        {
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd, m_bd1, m_bd2};
        
            EcsComponentTypeHandle<ComponentD> compDTypeHandle = m_world.GetComponentTypeHandle<ComponentD>();
            
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentD, ComponentD>().AnyOf<ComponentB, ComponentC, ComponentC>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentD> compsD = chunk.GetComponentAccessor(compDTypeHandle);
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
        public void All_DoubleComponentD_AnyFilter_ComponentB_DoubleComponentC_With_Components_Array_Test()
        {
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd, m_bd1, m_bd2};
            
            EcsComponentDataFromEntity<ComponentD> compsD = m_world.GetComponentDataFromEntityAccessor<ComponentD>();
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentD, ComponentD>().AnyOf<ComponentB, ComponentC, ComponentC>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumD += compsD[e].value;
                    actualEntities.Add(e);
                }
            }
            Assert.AreEqual(sumD, 18);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }

        [Test]
        public void All_ComponentB_None_ComponentA_With_Chunks_Test()
        {
            int sumB = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_bd1, m_bd2, m_bc, m_b1, m_b2, m_b3, m_b4, m_b5 };
        
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB>().NoneOf<ComponentA>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor(compBTypeHandle);
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumB += compsB[i].value;
                    actualEntities.Add(e);
                }
            }
            
            Assert.AreEqual(sumB, 116);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void All_ComponentB_None_ComponentA_With_Components_Array_Test()
        {
            int sumB = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_bd1, m_bd2, m_bc, m_b1, m_b2, m_b3, m_b4, m_b5 };
        
            EcsComponentDataFromEntity<ComponentB> compsB = m_world.GetComponentDataFromEntityAccessor<ComponentB>();
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB>().NoneOf<ComponentA>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumB += compsB[e].value;
                    actualEntities.Add(e);
                }
            }
            Assert.AreEqual(sumB, 116);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void All_ComponentB_And_ComponentD_None_ComponentA_With_Chunks_Test()
        {
            int sumB = 0;
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_bd1, m_bd2 };
        
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            EcsComponentTypeHandle<ComponentD> compDTypeHandle = m_world.GetComponentTypeHandle<ComponentD>();
            
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB, ComponentD>().NoneOf<ComponentA>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor(compBTypeHandle);
                EcsChunkComponentAccessor<ComponentD> compsD = chunk.GetComponentAccessor(compDTypeHandle);
                
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
        
        [Test]
        public void All_ComponentB_And_ComponentD_None_ComponentA_With_Components_Array_Test()
        {
            int sumB = 0;
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_bd1, m_bd2 };
            
            EcsComponentDataFromEntity<ComponentB> compsB = m_world.GetComponentDataFromEntityAccessor<ComponentB>();
            EcsComponentDataFromEntity<ComponentD> compsD = m_world.GetComponentDataFromEntityAccessor<ComponentD>();
            
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB, ComponentD>().NoneOf<ComponentA>());
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                
                for (int i = 0; i < chunk.count; i++)
                {
                    EcsEntity e = entityAccessor[i];
                    sumB += compsB[e].value;
                    sumD += compsD[e].value;
                    actualEntities.Add(e);
                }
            }
            Assert.AreEqual(sumB, 15);
            Assert.AreEqual(sumD, 15);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
    }
}