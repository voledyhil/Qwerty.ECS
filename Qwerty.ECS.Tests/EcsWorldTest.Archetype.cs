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
        [Test]
        public void FillArchetypeTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 64, entitiesCapacity = 32});
            EcsEntity entity0 = world.CreateEntity(new ComponentA { value = 0 }, new ComponentB { value = 1 }, new ComponentC {value = 2});
            EcsEntity entity1 = world.CreateEntity(new ComponentA { value = 3 }, new ComponentB { value = 4 }, new ComponentC {value = 5});
            EcsEntity entity2 = world.CreateEntity(new ComponentA { value = 6 }, new ComponentB { value = 7 }, new ComponentC {value = 8});
            EcsEntity entity3 = world.CreateEntity(new ComponentA { value = 9 }, new ComponentB { value = 10 }, new ComponentC {value = 11});
            EcsEntity entity4 = world.CreateEntity(new ComponentA { value = 12 }, new ComponentB { value = 13 }, new ComponentC {value = 14});
            EcsEntity entity5 = world.CreateEntity(new ComponentA { value = 15 }, new ComponentB { value = 16 }, new ComponentC {value = 17});

            EcsFilter filter = new EcsFilter().AllOf<ComponentA, ComponentB, ComponentC>();
            EcsArchetypeGroup group = world.Filter(filter);
            Assert.AreEqual(2, group.CalculateChunksCount());
            Assert.AreEqual(6, group.CalculateEntitiesCount());

            EcsComponentTypeHandle<ComponentA> compATypeHandle = m_world.GetComponentTypeHandle<ComponentA>();
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            EcsComponentTypeHandle<ComponentC> compCTypeHandle = m_world.GetComponentTypeHandle<ComponentC>();

            int chunkCounter = 0;
            foreach (EcsChunkAccessor chunk in group)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentA> compAAccessor = chunk.GetComponentAccessor(compATypeHandle);
                EcsChunkComponentAccessor<ComponentB> compBAccessor = chunk.GetComponentAccessor(compBTypeHandle);
                EcsChunkComponentAccessor<ComponentC> compCAccessor = chunk.GetComponentAccessor(compCTypeHandle);

                if (chunkCounter++ % 2 == 0)
                {
                    Assert.AreEqual(3, chunk.count);
                    Assert.AreEqual(entity3, entityAccessor[0]);
                    Assert.AreEqual(new ComponentA { value = 9 }, compAAccessor[0]);
                    Assert.AreEqual(new ComponentB { value = 10 }, compBAccessor[0]);
                    Assert.AreEqual(new ComponentC { value = 11 }, compCAccessor[0]);
                    Assert.AreEqual(entity4, entityAccessor[1]);
                    Assert.AreEqual(new ComponentA { value = 12 }, compAAccessor[1]);
                    Assert.AreEqual(new ComponentB { value = 13 }, compBAccessor[1]);
                    Assert.AreEqual(new ComponentC { value = 14 }, compCAccessor[1]);
                    Assert.AreEqual(entity5, entityAccessor[2]);
                    Assert.AreEqual(new ComponentA { value = 15 }, compAAccessor[2]);
                    Assert.AreEqual(new ComponentB { value = 16 }, compBAccessor[2]);
                    Assert.AreEqual(new ComponentC { value = 17 }, compCAccessor[2]);
                }
                else
                {
                    Assert.AreEqual(3, chunk.count);
                    Assert.AreEqual(entity0, entityAccessor[0]);
                    Assert.AreEqual(new ComponentA { value = 0 }, compAAccessor[0]);
                    Assert.AreEqual(new ComponentB { value = 1 }, compBAccessor[0]);
                    Assert.AreEqual(new ComponentC { value = 2 }, compCAccessor[0]);
                    Assert.AreEqual(entity1, entityAccessor[1]);
                    Assert.AreEqual(new ComponentA { value = 3 }, compAAccessor[1]);
                    Assert.AreEqual(new ComponentB { value = 4 }, compBAccessor[1]);
                    Assert.AreEqual(new ComponentC { value = 5 }, compCAccessor[1]);
                    Assert.AreEqual(entity2, entityAccessor[2]);
                    Assert.AreEqual(new ComponentA { value = 6 }, compAAccessor[2]);
                    Assert.AreEqual(new ComponentB { value = 7 }, compBAccessor[2]);
                    Assert.AreEqual(new ComponentC { value = 8 }, compCAccessor[2]);
                }
            }
            
            Assert.AreEqual(2, chunkCounter);
            
            world.Dispose();
        }
        
        [Test]
        public void ChangeArchetypeAfterDestroyEntityTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 64, entitiesCapacity = 32});
            EcsEntity entity0 = world.CreateEntity(new ComponentA { value = 0 }, new ComponentB { value = 1 }, new ComponentC {value = 2});
            EcsEntity entity1 = world.CreateEntity(new ComponentA { value = 3 }, new ComponentB { value = 4 }, new ComponentC {value = 5});
            EcsEntity entity2 = world.CreateEntity(new ComponentA { value = 6 }, new ComponentB { value = 7 }, new ComponentC {value = 8});
            EcsEntity entity3 = world.CreateEntity(new ComponentA { value = 9 }, new ComponentB { value = 10 }, new ComponentC {value = 11});
            EcsEntity entity4 = world.CreateEntity(new ComponentA { value = 12 }, new ComponentB { value = 13 }, new ComponentC {value = 14});
            EcsEntity entity5 = world.CreateEntity(new ComponentA { value = 15 }, new ComponentB { value = 16 }, new ComponentC {value = 17});
            
            world.DestroyEntity(entity0);
            world.DestroyEntity(entity4);
            world.DestroyEntity(entity2);
            
            EcsFilter filter = new EcsFilter().AllOf<ComponentA, ComponentB, ComponentC>();
            EcsArchetypeGroup group = world.Filter(filter);

            Assert.AreEqual(1, group.CalculateChunksCount());
            Assert.AreEqual(3, group.CalculateEntitiesCount());
            
            EcsComponentTypeHandle<ComponentA> compATypeHandle = m_world.GetComponentTypeHandle<ComponentA>();
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            EcsComponentTypeHandle<ComponentC> compCTypeHandle = m_world.GetComponentTypeHandle<ComponentC>();
            
            int chunkCounter = 0;
            foreach (EcsChunkAccessor chunk in group)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentA> compAAccessor = chunk.GetComponentAccessor(compATypeHandle);
                EcsChunkComponentAccessor<ComponentB> compBAccessor = chunk.GetComponentAccessor(compBTypeHandle);
                EcsChunkComponentAccessor<ComponentC> compCAccessor = chunk.GetComponentAccessor(compCTypeHandle);
                Assert.AreEqual(3, chunk.count);
                Assert.AreEqual(entity5, entityAccessor[0]);
                Assert.AreEqual(new ComponentA { value = 15 }, compAAccessor[0]);
                Assert.AreEqual(new ComponentB { value = 16 }, compBAccessor[0]);
                Assert.AreEqual(new ComponentC { value = 17 }, compCAccessor[0]);
                Assert.AreEqual(entity1, entityAccessor[1]);
                Assert.AreEqual(new ComponentA { value = 3 }, compAAccessor[1]);
                Assert.AreEqual(new ComponentB { value = 4 }, compBAccessor[1]);
                Assert.AreEqual(new ComponentC { value = 5 }, compCAccessor[1]);
                Assert.AreEqual(entity3, entityAccessor[2]);
                Assert.AreEqual(new ComponentA { value = 9 }, compAAccessor[2]);
                Assert.AreEqual(new ComponentB { value = 10 }, compBAccessor[2]);
                Assert.AreEqual(new ComponentC { value = 11 }, compCAccessor[2]);
                
                chunkCounter++;
            }
            
            Assert.AreEqual(1, chunkCounter);
            
            world.Dispose();
        }
        
        [Test]
        public void ChangeArchetypeAfterRemoveComponentTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 64, entitiesCapacity = 32});
            EcsEntity entity0 = world.CreateEntity(new ComponentA { value = 0 }, new ComponentB { value = 1 }, new ComponentC {value = 2});
            EcsEntity entity1 = world.CreateEntity(new ComponentA { value = 3 }, new ComponentB { value = 4 }, new ComponentC {value = 5});
            EcsEntity entity2 = world.CreateEntity(new ComponentA { value = 6 }, new ComponentB { value = 7 }, new ComponentC {value = 8});
            EcsEntity entity3 = world.CreateEntity(new ComponentA { value = 9 }, new ComponentB { value = 10 }, new ComponentC {value = 11});
            EcsEntity entity4 = world.CreateEntity(new ComponentA { value = 12 }, new ComponentB { value = 13 }, new ComponentC {value = 14});
            EcsEntity entity5 = world.CreateEntity(new ComponentA { value = 15 }, new ComponentB { value = 16 }, new ComponentC {value = 17});
            
            world.RemoveComponent<ComponentA>(entity0);
            world.RemoveComponent<ComponentB>(entity4);
            world.RemoveComponent<ComponentC>(entity2);
            
            EcsFilter filter = new EcsFilter().AllOf<ComponentA, ComponentB, ComponentC>();
            EcsArchetypeGroup group = world.Filter(filter);
            Assert.AreEqual(1, group.CalculateChunksCount());
            Assert.AreEqual(3, group.CalculateEntitiesCount());
            
            EcsComponentTypeHandle<ComponentA> compATypeHandle = m_world.GetComponentTypeHandle<ComponentA>();
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            EcsComponentTypeHandle<ComponentC> compCTypeHandle = m_world.GetComponentTypeHandle<ComponentC>();
            
            
            int chunkCounter = 0;
            foreach (EcsChunkAccessor chunk in group)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentA> compAAccessor = chunk.GetComponentAccessor(compATypeHandle);
                EcsChunkComponentAccessor<ComponentB> compBAccessor = chunk.GetComponentAccessor(compBTypeHandle);
                EcsChunkComponentAccessor<ComponentC> compCAccessor = chunk.GetComponentAccessor(compCTypeHandle);
                Assert.AreEqual(3, chunk.count);
                Assert.AreEqual(entity5, entityAccessor[0]);
                Assert.AreEqual(new ComponentA { value = 15 }, compAAccessor[0]);
                Assert.AreEqual(new ComponentB { value = 16 }, compBAccessor[0]);
                Assert.AreEqual(new ComponentC { value = 17 }, compCAccessor[0]);
                Assert.AreEqual(entity1, entityAccessor[1]);
                Assert.AreEqual(new ComponentA { value = 3 }, compAAccessor[1]);
                Assert.AreEqual(new ComponentB { value = 4 }, compBAccessor[1]);
                Assert.AreEqual(new ComponentC { value = 5 }, compCAccessor[1]);
                Assert.AreEqual(entity3, entityAccessor[2]);
                Assert.AreEqual(new ComponentA { value = 9 }, compAAccessor[2]);
                Assert.AreEqual(new ComponentB { value = 10 }, compBAccessor[2]);
                Assert.AreEqual(new ComponentC { value = 11 }, compCAccessor[2]);
                
                chunkCounter++;
            }
            Assert.AreEqual(1, chunkCounter);

            filter = new EcsFilter().AllOf<ComponentB, ComponentC>().NoneOf<ComponentA>();
            group = world.Filter(filter);
            Assert.AreEqual(1, group.CalculateChunksCount());
            
            chunkCounter = 0;
            foreach (EcsChunkAccessor chunk in group)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentB> compBAccessor = chunk.GetComponentAccessor(compBTypeHandle);
                EcsChunkComponentAccessor<ComponentC> compCAccessor = chunk.GetComponentAccessor(compCTypeHandle);
                Assert.AreEqual(1, chunk.count);
                Assert.AreEqual(entity0, entityAccessor[0]);
                Assert.AreEqual(new ComponentB { value = 1 }, compBAccessor[0]);
                Assert.AreEqual(new ComponentC { value = 2 }, compCAccessor[0]);
                
                chunkCounter++;
            }
            Assert.AreEqual(1, chunkCounter);

            filter = new EcsFilter().AllOf<ComponentA, ComponentC>().NoneOf<ComponentB>();
            group = world.Filter(filter);
            Assert.AreEqual(1, group.CalculateChunksCount());
            
            chunkCounter = 0;
            foreach (EcsChunkAccessor chunk in group)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentA> compAAccessor = chunk.GetComponentAccessor(compATypeHandle);
                EcsChunkComponentAccessor<ComponentC> compCAccessor = chunk.GetComponentAccessor(compCTypeHandle);
                Assert.AreEqual(1, chunk.count);
                Assert.AreEqual(entity4, entityAccessor[0]);
                Assert.AreEqual(new ComponentA { value = 12 }, compAAccessor[0]);
                Assert.AreEqual(new ComponentC { value = 14 }, compCAccessor[0]);
                
                chunkCounter++;
            }
            Assert.AreEqual(1, chunkCounter);

            filter = new EcsFilter().AllOf<ComponentA, ComponentB>().NoneOf<ComponentC>();
            group = world.Filter(filter);
            Assert.AreEqual(1, group.CalculateChunksCount());
            
            chunkCounter = 0;
            foreach (EcsChunkAccessor chunk in group)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentA> compAAccessor = chunk.GetComponentAccessor(compATypeHandle);
                EcsChunkComponentAccessor<ComponentB> compBAccessor = chunk.GetComponentAccessor(compBTypeHandle);
                Assert.AreEqual(1, chunk.count);
                Assert.AreEqual(entity2, entityAccessor[0]);
                Assert.AreEqual(new ComponentA { value = 6 }, compAAccessor[0]);
                Assert.AreEqual(new ComponentB { value = 7 }, compBAccessor[0]);
                
                chunkCounter++;
            }
            Assert.AreEqual(1, chunkCounter);

            world.Dispose();
        }
        
        [Test]
        public void ChangeArchetypeAfterAddComponentTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 64, entitiesCapacity = 32});
            EcsEntity entity0 = world.CreateEntity(new ComponentA { value = 0 }, new ComponentC {value = 2});
            EcsEntity entity1 = world.CreateEntity(new ComponentA { value = 3 }, new ComponentC {value = 5});
            EcsEntity entity2 = world.CreateEntity(new ComponentA { value = 6 }, new ComponentC {value = 8});
            EcsEntity entity3 = world.CreateEntity(new ComponentA { value = 9 }, new ComponentC {value = 11});
            EcsEntity entity4 = world.CreateEntity(new ComponentA { value = 12 }, new ComponentC {value = 14});
            EcsEntity entity5 = world.CreateEntity(new ComponentA { value = 15 }, new ComponentC {value = 17});
            
            EcsFilter filter = new EcsFilter().AllOf<ComponentA, ComponentC>().NoneOf<ComponentB>();
            EcsArchetypeGroup archetypeGroup = world.Filter(filter);
            Assert.AreEqual(2, archetypeGroup.CalculateChunksCount());
            
            world.AddComponent(entity0, new ComponentB { value = 1 });
            world.AddComponent(entity1, new ComponentB { value = 4 });
            world.AddComponent(entity2, new ComponentB { value = 7 });
            world.AddComponent(entity3, new ComponentB { value = 10 });
            world.AddComponent(entity4, new ComponentB { value = 13 });
            world.AddComponent(entity5, new ComponentB { value = 16 });
            
            filter = new EcsFilter().AllOf<ComponentA, ComponentC>().NoneOf<ComponentB>();
            EcsArchetypeGroup group = world.Filter(filter);
            Assert.AreEqual(0, group.CalculateChunksCount());
            Assert.AreEqual(0, group.CalculateEntitiesCount());
            
            filter = new EcsFilter().AllOf<ComponentA, ComponentC, ComponentB>();
            group = world.Filter(filter);
            Assert.AreEqual(2, group.CalculateChunksCount());
            Assert.AreEqual(6, group.CalculateEntitiesCount());
            
            EcsComponentTypeHandle<ComponentA> compATypeHandle = m_world.GetComponentTypeHandle<ComponentA>();
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            EcsComponentTypeHandle<ComponentC> compCTypeHandle = m_world.GetComponentTypeHandle<ComponentC>();
            
            int chunkCounter = 0;
            foreach (EcsChunkAccessor chunk in group)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentA> compAAccessor = chunk.GetComponentAccessor(compATypeHandle);
                EcsChunkComponentAccessor<ComponentB> compBAccessor = chunk.GetComponentAccessor(compBTypeHandle);
                EcsChunkComponentAccessor<ComponentC> compCAccessor = chunk.GetComponentAccessor(compCTypeHandle);

                if (chunkCounter++ % 2 == 0)
                {
                    Assert.AreEqual(3, chunk.count);
                    Assert.AreEqual(entity3, entityAccessor[0]);
                    Assert.AreEqual(new ComponentA { value = 9 }, compAAccessor[0]);
                    Assert.AreEqual(new ComponentB { value = 10 }, compBAccessor[0]);
                    Assert.AreEqual(new ComponentC { value = 11 }, compCAccessor[0]);
                    Assert.AreEqual(entity4, entityAccessor[1]);
                    Assert.AreEqual(new ComponentA { value = 12 }, compAAccessor[1]);
                    Assert.AreEqual(new ComponentB { value = 13 }, compBAccessor[1]);
                    Assert.AreEqual(new ComponentC { value = 14 }, compCAccessor[1]);
                    Assert.AreEqual(entity5, entityAccessor[2]);
                    Assert.AreEqual(new ComponentA { value = 15 }, compAAccessor[2]);
                    Assert.AreEqual(new ComponentB { value = 16 }, compBAccessor[2]);
                    Assert.AreEqual(new ComponentC { value = 17 }, compCAccessor[2]);
                }
                else
                {
                    Assert.AreEqual(3, chunk.count);
                    Assert.AreEqual(entity0, entityAccessor[0]);
                    Assert.AreEqual(new ComponentA { value = 0 }, compAAccessor[0]);
                    Assert.AreEqual(new ComponentB { value = 1 }, compBAccessor[0]);
                    Assert.AreEqual(new ComponentC { value = 2 }, compCAccessor[0]);
                    Assert.AreEqual(entity1, entityAccessor[1]);
                    Assert.AreEqual(new ComponentA { value = 3 }, compAAccessor[1]);
                    Assert.AreEqual(new ComponentB { value = 4 }, compBAccessor[1]);
                    Assert.AreEqual(new ComponentC { value = 5 }, compCAccessor[1]);
                    Assert.AreEqual(entity2, entityAccessor[2]);
                    Assert.AreEqual(new ComponentA { value = 6 }, compAAccessor[2]);
                    Assert.AreEqual(new ComponentB { value = 7 }, compBAccessor[2]);
                    Assert.AreEqual(new ComponentC { value = 8 }, compCAccessor[2]);
                }
            }
            
            Assert.AreEqual(2, chunkCounter);

            world.Dispose();
        }

        // public struct float3
        // {
        //     public float x;
        //     public float y;
        //     public float z;
        // }
        //
        // public struct PhysicsAABB : IEcsComponent
        // {
        //     public float3 min;
        //     public float3 max;
        // }
        //
        // public struct PhysicsPosition : IEcsComponent
        // {
        //     public float3 value;
        // }
        //
        // public struct PhysicsStatic : IEcsComponent
        // {
        //     public bool value;
        // }
        //
        // [Test]
        // public void ChangeArchetypeAfterAddComponentTest5()
        // {
        //     EcsTypeManager.Register<PhysicsStatic>("static"); //2150
        //     EcsTypeManager.Register<PhysicsAABB>("aabb"); //23841
        //     EcsTypeManager.Register<PhysicsPosition>("position"); //24417
        //
        //     EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 1024, entitiesCapacity = 32});
        //
        //     EcsEntity entity = world.CreateEntity(new PhysicsPosition { value = new float3 { x = 1, y = 1, z = 1 } }, new PhysicsAABB());
        //     world.AddComponent(entity, new PhysicsStatic { value = true });
        //
        //     float3 value = world.GetComponent<PhysicsPosition>(entity).value;
        //     Assert.AreEqual(value.x, 1);
        //     Assert.AreEqual(value.y, 1);
        //     Assert.AreEqual(value.z, 1);
        //     
        //     world.Dispose();
        // }
        //
        
        [Test]
        public void ChangeArchetypeAfterAddComponentTest2()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting { archetypeChunkMaxSizeInByte = 1024, entitiesCapacity = 32 });

            EcsEntity entity = world.CreateEntity(new ComponentA3 { x = 1, y = 2, z = 3 }, new ComponentC { value = 3 });
            world.AddComponent(entity, new ComponentB { value = 2 });

            ComponentA3 compA = world.GetComponent<ComponentA3>(entity);
            Assert.AreEqual(1, compA.x);
            Assert.AreEqual(2, compA.y);
            Assert.AreEqual(3, compA.z);

            Assert.AreEqual(2, world.GetComponent<ComponentB>(entity).value);
            Assert.AreEqual(3, world.GetComponent<ComponentC>(entity).value);

            world.Dispose();
        }
        
        [Test]
        public void ChangeComponentsWithChunkAccessorTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 64, entitiesCapacity = 32});
            world.CreateEntity(new ComponentB { value = 1 }, new ComponentC {value = 2});
            world.CreateEntity(new ComponentB { value = 4 }, new ComponentC {value = 5});
            world.CreateEntity(new ComponentA { value = 6 }, new ComponentB { value = 7 });
            world.CreateEntity(new ComponentA { value = 9 }, new ComponentB { value = 10 });
            world.CreateEntity(new ComponentA { value = 12 }, new ComponentB { value = 13 }, new ComponentC {value = 14});
            world.CreateEntity(new ComponentA { value = 15 }, new ComponentB { value = 16 }, new ComponentC {value = 17});
            
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            
            EcsArchetypeGroup group = m_world.Filter(new EcsFilter().AllOf<ComponentB>());
            foreach (EcsChunkAccessor chunk in group)
            {
                EcsChunkComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor(compBTypeHandle);
                for (int i = 0; i < chunk.count; i++)
                {
                    compsB[i] = new ComponentB { value = int.MaxValue };
                }
            }
            
            foreach (EcsChunkAccessor chunk in group)
            {
                EcsChunkComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor<ComponentB>(compBTypeHandle);
                for (int i = 0; i < chunk.count; i++)
                {
                    Assert.AreEqual(int.MaxValue, compsB[i].value);
                }
            }
            
            world.Dispose();
        }
        
        [Test]
        public void ChangeComponentsWithComponentArrayAccessorTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkMaxSizeInByte = 64, entitiesCapacity = 32});
            world.CreateEntity(new ComponentB { value = 1 }, new ComponentC {value = 2});
            world.CreateEntity(new ComponentB { value = 4 }, new ComponentC {value = 5});
            world.CreateEntity(new ComponentA { value = 6 }, new ComponentB { value = 7 });
            world.CreateEntity(new ComponentA { value = 9 }, new ComponentB { value = 10 });
            world.CreateEntity(new ComponentA { value = 12 }, new ComponentB { value = 13 }, new ComponentC {value = 14});
            world.CreateEntity(new ComponentA { value = 15 }, new ComponentB { value = 16 }, new ComponentC {value = 17});
            
            EcsComponentDataFromEntity<ComponentB> compsB = m_world.GetComponentDataFromEntityAccessor<ComponentB>();
            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB>());

            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    compsB[entityAccessor[i]] = new ComponentB { value = int.MaxValue };
                }
            }
            
            foreach (EcsChunkAccessor chunk in archetypeGroup)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    Assert.AreEqual(int.MaxValue, compsB[entityAccessor[i]].value);
                }
            }
            world.Dispose();
        }
    }
}