using NUnit.Framework;
using Qwerty.ECS.Runtime;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Tests
{
    [TestFixture]
    public class EcsWorldTest
    {
        private EcsWorld _world;
        private EcsEntity _entityAB;
        private EcsEntity _entityABD;
        private EcsEntity _entityAC;
        private EcsEntity _entityAD;
        private EcsEntity _entityBC;
        private EcsEntity _entityBD0;
        private EcsEntity _entityBD1;
        
        [SetUp]
        public void InitWorld()
        {
            EcsComponentType<ComponentA>.Register();
            EcsComponentType<ComponentB>.Register();
            EcsComponentType<ComponentC>.Register();
            EcsComponentType<ComponentD>.Register();

            _world = new EcsWorld();

            _entityABD = _world.CreateEntity(new ComponentA {Value = 1}, new ComponentB {Value = 2}, new ComponentD {Value = 3});
            _entityAC = _world.CreateEntity(new ComponentA {Value = 4}, new ComponentC {Value = 5});
            _entityBD0 = _world.CreateEntity(new ComponentB {Value = 6}, new ComponentD {Value = 7});
            _entityBD1 = _world.CreateEntity(new ComponentD {Value = 8}, new ComponentB {Value = 8});
            _entityBC = _world.CreateEntity(new ComponentC {Value = 9}, new ComponentB {Value = 10});
            _entityAB = _world.CreateEntity(new ComponentB {Value = 11}, new ComponentA {Value = 12});
            _entityAD = _world.CreateEntity(new ComponentA {Value = 13}, new ComponentD {Value = 14});
        }

        [TearDown]
        public void CleanWorld()
        {
            _world.Dispose();
        }

        [Test]
        public void GetSetRemoveComponentTest()
        {
            EcsWorld world = new EcsWorld();
            
            ComponentB componentB = new ComponentB();
            EcsEntity entity = world.CreateEntity();
            world.AddComponent(entity, new ComponentA());
            world.AddComponent(entity, componentB);
            world.AddComponent(entity, new ComponentC());

            Assert.IsNotNull(world.GetComponent<ComponentA>(entity));
            Assert.IsNotNull(world.GetComponent<ComponentC>(entity));
            Assert.AreEqual(componentB, world.GetComponent<ComponentB>(entity));
            Assert.IsFalse(world.HasComponent<ComponentD>(entity));

            world.RemoveComponent<ComponentB>(entity);
            Assert.IsFalse(world.HasComponent<ComponentB>(entity));
        }

        [Test]
        public void All_ComponentB_Test()
        {
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>
            {
                _entityABD, _entityBD0, _entityBD1, _entityBC, _entityAB
            };
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            
            EcsArchetypeGroup archetypeGroup = _world.Filter(new EcsFilter().AllOf<ComponentB>());
            EcsComponentAccessor<ComponentB> componentsB = _world.GetComponentAccessor<ComponentB>();

            int sumB = 0;
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);
                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity entity = entities[j];
                    sumB += componentsB[entity].Value;
                    actualEntities.Add(entity);
                }
            }
            
            Assert.AreEqual(sumB, 37);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void All_ComponentB_And_ComponentD_Test()
        {
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>
            {
                _entityABD, _entityBD0, _entityBD1
            };
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            
            EcsArchetypeGroup archetypeGroup = _world.Filter(new EcsFilter().AllOf<ComponentB, ComponentD>());
            EcsComponentAccessor<ComponentB> componentsB = _world.GetComponentAccessor<ComponentB>();
            EcsComponentAccessor<ComponentD> componentsD = _world.GetComponentAccessor<ComponentD>();
            
            int sumB = 0;
            int sumD = 0;
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);
  
                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity entity = entities[j];
                    
                    sumB += componentsB[entity].Value;
                    sumD += componentsD[entity].Value;

                    actualEntities.Add(entity);
                }
            }
            
            Assert.AreEqual(sumB, 16);
            Assert.AreEqual(sumD, 18);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }

        [Test]
        public void Any_ComponentB_Test()
        {
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>()
            {
                _entityABD, _entityBD0, _entityBD1, _entityBC, _entityAB
            };
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            
            EcsArchetypeGroup archetypeGroup = _world.Filter(new EcsFilter().AnyOf<ComponentB>());
            EcsComponentAccessor<ComponentB> componentsB = _world.GetComponentAccessor<ComponentB>();
            
            int sumB = 0;
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);
        
                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity entity = entities[j];
                    sumB += componentsB[entity].Value;
                    actualEntities.Add(entity);
                }
            }
            
            Assert.AreEqual(sumB, 37);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void Any_ComponentB_Or_ComponentD_Test()
        {
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>()
            {
                _entityABD, _entityBD0, _entityBD1, _entityBC, _entityAB, _entityAD
            };
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            
            EcsArchetypeGroup archetypeGroup = _world.Filter(new EcsFilter().AnyOf<ComponentB, ComponentD>());
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);

                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    actualEntities.Add(entities[j]);
                }
            }
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void None_ComponentB_And_ComponentD_Test()
        {
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>()
            {
                _entityAC
            };
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            
            EcsArchetypeGroup archetypeGroup = _world.Filter(new EcsFilter().NoneOf<ComponentB, ComponentD>());
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);

                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    actualEntities.Add(entities[j]);
                }
            }
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void None_DoubleComponentB_And_ComponentD_Test()
        {
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>()
            {
                _entityAC
            };
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            
            EcsArchetypeGroup archetypeGroup = _world.Filter(new EcsFilter().NoneOf<ComponentB, ComponentB, ComponentD>());
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);

                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    actualEntities.Add(entities[j]);
                }
            }
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }

        [Test]
        public void All_DoubleComponentB_And_ComponentD_AnyFilter_ComponentA_Test()
        {
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>()
            {
                _entityABD
            };
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            
            EcsArchetypeGroup archetypeGroup = _world.Filter(new EcsFilter().AllOf<ComponentB, ComponentB, ComponentD>().AnyOf<ComponentA>());
            EcsComponentAccessor<ComponentA> componentsA = _world.GetComponentAccessor<ComponentA>();
            EcsComponentAccessor<ComponentB> componentsB = _world.GetComponentAccessor<ComponentB>();
            EcsComponentAccessor<ComponentD> componentsD = _world.GetComponentAccessor<ComponentD>();
        
            int sumA = 0;
            int sumB = 0;
            int sumD = 0;
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);
        
                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity entity = entities[j];
                    
                    sumA += componentsA[entity].Value;
                    sumB += componentsB[entity].Value;
                    sumD += componentsD[entity].Value;

                    actualEntities.Add(entity);
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
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>()
            {
                _entityABD, _entityBD0, _entityBD1
            };
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            
            EcsArchetypeGroup archetypeGroup = _world.Filter(new EcsFilter().AllOf<ComponentD, ComponentD>().AnyOf<ComponentB, ComponentC, ComponentC>());
            EcsComponentAccessor<ComponentD> componentsD = _world.GetComponentAccessor<ComponentD>();
        
            int sumD = 0;
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);
 
                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity entity = entities[j];
                    sumD += componentsD[entity].Value;
                    actualEntities.Add(entity);
                }
            }
            
            Assert.AreEqual(sumD, 18);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void All_ComponentB_None_ComponentA_Test()
        {
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>()
            {
                _entityBD0, _entityBD1, _entityBC
            };
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            
            EcsArchetypeGroup archetypeGroup = _world.Filter(new EcsFilter().AllOf<ComponentB>().NoneOf<ComponentA>());
            EcsComponentAccessor<ComponentB> componentsB = _world.GetComponentAccessor<ComponentB>();
        
            int sumB = 0;
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);
        
                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity entity = entities[j];
                    sumB += componentsB[entity].Value;
                    actualEntities.Add(entity);
                }
            }
            
            Assert.AreEqual(sumB, 24);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void All_ComponentB_And_ComponentD_None_ComponentA_Test()
        {
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>()
            {
                _entityBD0, _entityBD1
            };
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            
            EcsArchetypeGroup archetypeGroup = _world.Filter(new EcsFilter().AllOf<ComponentB, ComponentD>().NoneOf<ComponentA>());
            EcsComponentAccessor<ComponentB> componentsB = _world.GetComponentAccessor<ComponentB>();
            EcsComponentAccessor<ComponentD> componentsD = _world.GetComponentAccessor<ComponentD>();
            
            int sumB = 0;
            int sumD = 0;
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);
        
                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity entity = entities[j];
                    
                    sumB += componentsB[entity].Value;
                    sumD += componentsD[entity].Value;
                    
                    actualEntities.Add(entity);
                }
            }
            
            Assert.AreEqual(sumB, 14);
            Assert.AreEqual(sumD, 15);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
        
        [Test]
        public void All_ComponentB_None_ComponentA_ChangeValues_Test()
        {
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>()
            {
                _entityBD0, _entityBD1, _entityBC
            };
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            
            EcsArchetypeGroup archetypeGroup = _world.Filter(new EcsFilter().AllOf<ComponentB>().NoneOf<ComponentA>());
            EcsComponentAccessor<ComponentB> componentsB = _world.GetComponentAccessor<ComponentB>();
            
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);
        
                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity entity = entities[j];
                    componentsB[entity] = new ComponentB {Value = byte.MaxValue};
                    actualEntities.Add(entity);
                }
            }
            
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
            
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);
        
                int count = entities.count;
                
                for (int j = 0; j < count; j++)
                {
                    Assert.AreEqual(byte.MaxValue, componentsB[entities[j]].Value);
                }
            }
        }

        [Test]
        public void AddComponentThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld();
            EcsEntity entity = world.CreateEntity();
            world.AddComponent(entity, new ComponentA());
            
            Assert.That(()=> world.AddComponent(entity, new ComponentA()), Throws.InvalidOperationException);
        }
        
        [Test]
        public void DestroyEntityThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld();
            EcsEntity entity = world.CreateEntity();
            world.DestroyEntity(entity);
            
            Assert.That(()=> world.DestroyEntity(entity), Throws.InvalidOperationException);
        }
        
        [Test]
        public void RemoveComponentThrowExceptionTest()
        {
            EcsWorld world = new EcsWorld();
            EcsEntity entity = world.CreateEntity();
            
            Assert.That(()=> world.RemoveComponent<ComponentA>(entity), Throws.InvalidOperationException);
        }

        [Test]
        public void ChangeArchetypeByAddRemoveComponentsTest()
        {
            EcsWorld world = new EcsWorld();
            Assert.AreEqual(1, world.ArchetypeCount); // empty
            
            EcsEntity entity0 = world.CreateEntity(new ComponentA(), new ComponentB());
            EcsEntity entity1 = world.CreateEntity(new ComponentA(), new ComponentB());
            EcsEntity entity2 = world.CreateEntity(new ComponentA(), new ComponentB());
            EcsEntity entity3 = world.CreateEntity(new ComponentA(), new ComponentB());
            
            Assert.AreEqual(3, world.ArchetypeCount); // empty, A, AB
            Assert.AreEqual(4, world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>()).CalculateCount());
            
            world.RemoveComponent<ComponentA>(entity1);
            world.RemoveComponent<ComponentA>(entity2);
            Assert.AreEqual(4, world.ArchetypeCount); // empty, A, B, AB
            Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>()).CalculateCount());
            Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
            Assert.AreEqual(4, world.Filter(new EcsFilter().AllOf<ComponentB>()).CalculateCount());
            
            world.RemoveComponent<ComponentA>(entity0);
            Assert.AreEqual(4, world.ArchetypeCount); // empty, A, B, AB
            Assert.AreEqual(1, world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>()).CalculateCount());
            Assert.AreEqual(1, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
            Assert.AreEqual(4, world.Filter(new EcsFilter().AllOf<ComponentB>()).CalculateCount());
            
            world.AddComponent(entity1, new ComponentA()); 
            world.AddComponent(entity2, new ComponentA());
            Assert.AreEqual(4, world.ArchetypeCount); // empty, A, B, AB
            Assert.AreEqual(3, world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>()).CalculateCount());
            Assert.AreEqual(3, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
            Assert.AreEqual(4, world.Filter(new EcsFilter().AllOf<ComponentB>()).CalculateCount());
            
            world.RemoveComponent<ComponentA>(entity3);
            world.RemoveComponent<ComponentB>(entity3);
            Assert.AreEqual(4, world.ArchetypeCount); // empty, A, B, AB
            Assert.AreEqual(1, world.Filter(new EcsFilter().NoneOf<ComponentA, ComponentB>()).CalculateCount());
            Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>()).CalculateCount());
            Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
            Assert.AreEqual(3, world.Filter(new EcsFilter().AllOf<ComponentB>()).CalculateCount());
            
            world.AddComponent(entity3, new ComponentC());
            Assert.AreEqual(5, world.ArchetypeCount); // empty, A, B, AB, C
            Assert.AreEqual(2, world.Filter(new EcsFilter().NoneOf<ComponentA, ComponentB>()).CalculateCount());
            Assert.AreEqual(1, world.Filter(new EcsFilter().AllOf<ComponentC>()).CalculateCount());
            Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>()).CalculateCount());
            Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
            Assert.AreEqual(3, world.Filter(new EcsFilter().AllOf<ComponentB>()).CalculateCount());
        }

        [Test]
        public unsafe void ChangeArchetypeAndReadFromAccessorPtrTest()
        {
            EcsWorld world = new EcsWorld();
            EcsComponentAccessor<ComponentB> componentsB = world.GetComponentAccessor<ComponentB>();
            
            world.CreateEntity(new ComponentB { Value = 1});
            int sumB = 0;
            
            EcsArchetypeGroup archetypeGroup = world.Filter(new EcsFilter().AllOf<ComponentB>());
            EcsArchetypesAccessor accessor = *archetypeGroup.GetArchetypeAccessorPtr();
            for (int i = 0; i < accessor.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = accessor.GetEntityArray(i);
                for (int j = 0; j < entities.count; j++)
                {
                    EcsEntity entity = entities[j];
                    sumB += componentsB[entity].Value;
                }
            }
            Assert.AreEqual(1, sumB);
            
            world.CreateEntity(new ComponentB { Value = 1});
            
            sumB = 0;
            archetypeGroup = world.Filter(new EcsFilter().AllOf<ComponentB>());
            accessor = *archetypeGroup.GetArchetypeAccessorPtr();
            for (int i = 0; i < accessor.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = accessor.GetEntityArray(i);
                for (int j = 0; j < entities.count; j++)
                {
                    EcsEntity entity = entities[j];
                    sumB += componentsB[entity].Value;
                }
            }
            Assert.AreEqual(2, sumB);
        } 

        [Test]
        public void ChangeArchetypeByCreateDestroyEntitiesTest()
        {
            EcsWorld world = new EcsWorld();
            Assert.AreEqual(1, world.ArchetypeCount); // empty

            EcsEntity entity1 = world.CreateEntity(new ComponentA());
            EcsEntity entity2 = world.CreateEntity(new ComponentA());
            Assert.AreEqual(2, world.ArchetypeCount); // empty, A
            Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
            
            world.DestroyEntity(entity1);
            world.DestroyEntity(entity2);
            Assert.AreEqual(2, world.ArchetypeCount); // empty, A
            Assert.AreEqual(0, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
            
            entity1 = world.CreateEntity(new ComponentA());
            entity2 = world.CreateEntity(new ComponentA());
            Assert.AreEqual(2, world.ArchetypeCount); // empty, A
            Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
            
            world.DestroyEntity(entity1);
            world.DestroyEntity(entity2);
            Assert.AreEqual(2, world.ArchetypeCount); // empty, A
            Assert.AreEqual(0, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
        }
    }
}