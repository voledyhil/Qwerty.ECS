using System.Collections.Generic;
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
            EcsChunkCollection chunkCollection = group.GetChunks();
            Assert.AreEqual(2, group.CalculateChunksCount());
            Assert.AreEqual(6, group.CalculateEntitiesCount());

            EcsComponentTypeHandle<ComponentA> compATypeHandle = m_world.GetComponentTypeHandle<ComponentA>();
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            EcsComponentTypeHandle<ComponentC> compCTypeHandle = m_world.GetComponentTypeHandle<ComponentC>();
            
            EcsChunkAccessor chunk = chunkCollection.GetChunk(1);
            EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
            EcsChunkComponentAccessor<ComponentA> compAAccessor = chunk.GetComponentAccessor(compATypeHandle);
            EcsChunkComponentAccessor<ComponentB> compBAccessor = chunk.GetComponentAccessor(compBTypeHandle);
            EcsChunkComponentAccessor<ComponentC> compCAccessor = chunk.GetComponentAccessor(compCTypeHandle);
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

            chunk = chunkCollection.GetChunk(0);
            entityAccessor = chunk.GetEntityAccessor();
            compAAccessor = chunk.GetComponentAccessor(compATypeHandle);
            compBAccessor = chunk.GetComponentAccessor(compBTypeHandle);
            compCAccessor = chunk.GetComponentAccessor(compCTypeHandle);
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
            
            chunkCollection.Dispose();
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
            EcsChunkCollection groupAccessor = group.GetChunks();
            Assert.AreEqual(1, group.CalculateChunksCount());
            Assert.AreEqual(3, group.CalculateEntitiesCount());
            
            EcsComponentTypeHandle<ComponentA> compATypeHandle = m_world.GetComponentTypeHandle<ComponentA>();
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            EcsComponentTypeHandle<ComponentC> compCTypeHandle = m_world.GetComponentTypeHandle<ComponentC>();
            
            EcsChunkAccessor chunk = groupAccessor.GetChunk(0);
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
            
            groupAccessor.Dispose();
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
            EcsChunkCollection chunkCollection = group.GetChunks();
            Assert.AreEqual(1, group.CalculateChunksCount());
            Assert.AreEqual(3, group.CalculateEntitiesCount());
            
            EcsComponentTypeHandle<ComponentA> compATypeHandle = m_world.GetComponentTypeHandle<ComponentA>();
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            EcsComponentTypeHandle<ComponentC> compCTypeHandle = m_world.GetComponentTypeHandle<ComponentC>();

            
            EcsChunkAccessor chunk = chunkCollection.GetChunk(0);
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
            chunkCollection.Dispose();
            
            filter = new EcsFilter().AllOf<ComponentB, ComponentC>().NoneOf<ComponentA>();
            EcsArchetypeGroup archetypeGroup = world.Filter(filter);
            chunkCollection = archetypeGroup.GetChunks();
            Assert.AreEqual(1, archetypeGroup.CalculateChunksCount());
            
            chunk = chunkCollection.GetChunk(0);
            entityAccessor = chunk.GetEntityAccessor();
            compBAccessor = chunk.GetComponentAccessor(compBTypeHandle);
            compCAccessor = chunk.GetComponentAccessor(compCTypeHandle);
            Assert.AreEqual(1, chunk.count);
            Assert.AreEqual(entity0, entityAccessor[0]);
            Assert.AreEqual(new ComponentB { value = 1 }, compBAccessor[0]);
            Assert.AreEqual(new ComponentC { value = 2 }, compCAccessor[0]);
            chunkCollection.Dispose();
            
            filter = new EcsFilter().AllOf<ComponentA, ComponentC>().NoneOf<ComponentB>();
            archetypeGroup = world.Filter(filter);
            chunkCollection = archetypeGroup.GetChunks();
            Assert.AreEqual(1, archetypeGroup.CalculateChunksCount());
            
            chunk = chunkCollection.GetChunk(0);
            entityAccessor = chunk.GetEntityAccessor();
            compAAccessor = chunk.GetComponentAccessor(compATypeHandle);
            compCAccessor = chunk.GetComponentAccessor(compCTypeHandle);
            Assert.AreEqual(1, chunk.count);
            Assert.AreEqual(entity4, entityAccessor[0]);
            Assert.AreEqual(new ComponentA { value = 12 }, compAAccessor[0]);
            Assert.AreEqual(new ComponentC { value = 14 }, compCAccessor[0]);
            chunkCollection.Dispose();

            filter = new EcsFilter().AllOf<ComponentA, ComponentB>().NoneOf<ComponentC>();
            archetypeGroup = world.Filter(filter);
            chunkCollection = archetypeGroup.GetChunks();
            Assert.AreEqual(1, archetypeGroup.CalculateChunksCount());
            
            chunk = chunkCollection.GetChunk(0);
            entityAccessor = chunk.GetEntityAccessor();
            compAAccessor = chunk.GetComponentAccessor(compATypeHandle);
            compBAccessor = chunk.GetComponentAccessor(compBTypeHandle);
            Assert.AreEqual(1, chunk.count);
            Assert.AreEqual(entity2, entityAccessor[0]);
            Assert.AreEqual(new ComponentA { value = 6 }, compAAccessor[0]);
            Assert.AreEqual(new ComponentB { value = 7 }, compBAccessor[0]);
            chunkCollection.Dispose();
            
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
            EcsChunkCollection groupAccessor = group.GetChunks();
            Assert.AreEqual(2, group.CalculateChunksCount());
            Assert.AreEqual(6, group.CalculateEntitiesCount());
            
            EcsComponentTypeHandle<ComponentA> compATypeHandle = m_world.GetComponentTypeHandle<ComponentA>();
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = m_world.GetComponentTypeHandle<ComponentB>();
            EcsComponentTypeHandle<ComponentC> compCTypeHandle = m_world.GetComponentTypeHandle<ComponentC>();
            
            EcsChunkAccessor chunk = groupAccessor.GetChunk(1);
            EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
            EcsChunkComponentAccessor<ComponentA> compAAccessor = chunk.GetComponentAccessor(compATypeHandle);
            EcsChunkComponentAccessor<ComponentB> compBAccessor = chunk.GetComponentAccessor(compBTypeHandle);
            EcsChunkComponentAccessor<ComponentC> compCAccessor = chunk.GetComponentAccessor(compCTypeHandle);
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

            chunk = groupAccessor.GetChunk(0);
            entityAccessor = chunk.GetEntityAccessor();
            compAAccessor = chunk.GetComponentAccessor(compATypeHandle);
            compBAccessor = chunk.GetComponentAccessor(compBTypeHandle);
            compCAccessor = chunk.GetComponentAccessor(compCTypeHandle);
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
            groupAccessor.Dispose();
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

            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB>());
            EcsChunkCollection chunks = archetypeGroup.GetChunks();
            
            foreach (EcsChunkAccessor chunk in chunks)
            {
                EcsChunkComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor(compBTypeHandle);
                for (int i = 0; i < chunk.count; i++)
                {
                    compsB[i] = new ComponentB { value = int.MaxValue };
                }
            }
            
            foreach (EcsChunkAccessor chunk in chunks)
            {
                EcsChunkComponentAccessor<ComponentB> compsB = chunk.GetComponentAccessor<ComponentB>(compBTypeHandle);
                for (int i = 0; i < chunk.count; i++)
                {
                    Assert.AreEqual(int.MaxValue, compsB[i].value);
                }
            }
            
            chunks.Dispose();
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
            EcsChunkCollection chunks = archetypeGroup.GetChunks();
            
            foreach (EcsChunkAccessor chunk in chunks)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    compsB[entityAccessor[i]] = new ComponentB { value = int.MaxValue };
                }
            }
            
            foreach (EcsChunkAccessor chunk in chunks)
            {
                EcsChunkEntityAccessor entityAccessor = chunk.GetEntityAccessor();
                for (int i = 0; i < chunk.count; i++)
                {
                    Assert.AreEqual(int.MaxValue, compsB[entityAccessor[i]].value);
                }
            }
            
            chunks.Dispose();
        }
    }
}