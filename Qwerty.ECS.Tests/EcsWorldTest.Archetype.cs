using NUnit.Framework;
using Qwerty.ECS.Runtime;
using Qwerty.ECS.Runtime.Archetypes;

namespace Qwerty.ECS.Tests
{
    public partial class EcsWorldTest
    {
        [Test]
        public void FillArchetypeTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkSizeInByte = 64});
            EcsEntity entity0 = world.CreateEntity(new ComponentA { value = 0 }, new ComponentB { value = 1 }, new ComponentC {value = 2});
            EcsEntity entity1 = world.CreateEntity(new ComponentA { value = 3 }, new ComponentB { value = 4 }, new ComponentC {value = 5});
            EcsEntity entity2 = world.CreateEntity(new ComponentA { value = 6 }, new ComponentB { value = 7 }, new ComponentC {value = 8});
            EcsEntity entity3 = world.CreateEntity(new ComponentA { value = 9 }, new ComponentB { value = 10 }, new ComponentC {value = 11});
            EcsEntity entity4 = world.CreateEntity(new ComponentA { value = 12 }, new ComponentB { value = 13 }, new ComponentC {value = 14});
            EcsEntity entity5 = world.CreateEntity(new ComponentA { value = 15 }, new ComponentB { value = 16 }, new ComponentC {value = 17});
            
            EcsArchetype archetype = world.GetArchetype<ComponentA, ComponentB, ComponentC>();
            Assert.AreEqual(2, archetype.chunksCount);
            
            EcsArchetypeChunkAccessor chunk = archetype.GetChunk(0);
            EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
            EcsArchetypeComponentAccessor<ComponentA> compAAccessor = chunk.GetComponentAccessor<ComponentA>();
            EcsArchetypeComponentAccessor<ComponentB> compBAccessor = chunk.GetComponentAccessor<ComponentB>();
            EcsArchetypeComponentAccessor<ComponentC> compCAccessor = chunk.GetComponentAccessor<ComponentC>();
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
            
            chunk = archetype.GetChunk(1);
            entityAccessor = chunk.GetEntityAccessor();
            compAAccessor = chunk.GetComponentAccessor<ComponentA>();
            compBAccessor = chunk.GetComponentAccessor<ComponentB>();
            compCAccessor = chunk.GetComponentAccessor<ComponentC>();
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
            
            world.Dispose();
        }
        
        [Test]
        public void ChangeArchetypeAfterDestroyEntityTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkSizeInByte = 64});
            EcsEntity entity0 = world.CreateEntity(new ComponentA { value = 0 }, new ComponentB { value = 1 }, new ComponentC {value = 2});
            EcsEntity entity1 = world.CreateEntity(new ComponentA { value = 3 }, new ComponentB { value = 4 }, new ComponentC {value = 5});
            EcsEntity entity2 = world.CreateEntity(new ComponentA { value = 6 }, new ComponentB { value = 7 }, new ComponentC {value = 8});
            EcsEntity entity3 = world.CreateEntity(new ComponentA { value = 9 }, new ComponentB { value = 10 }, new ComponentC {value = 11});
            EcsEntity entity4 = world.CreateEntity(new ComponentA { value = 12 }, new ComponentB { value = 13 }, new ComponentC {value = 14});
            EcsEntity entity5 = world.CreateEntity(new ComponentA { value = 15 }, new ComponentB { value = 16 }, new ComponentC {value = 17});
            
            world.DestroyEntity(entity0);
            world.DestroyEntity(entity4);
            world.DestroyEntity(entity2);

            EcsArchetype archetype = world.GetArchetype<ComponentA, ComponentB, ComponentC>();
            Assert.AreEqual(1, archetype.chunksCount);
            
            EcsArchetypeChunkAccessor chunk = archetype.GetChunk(0);
            EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
            EcsArchetypeComponentAccessor<ComponentA> compAAccessor = chunk.GetComponentAccessor<ComponentA>();
            EcsArchetypeComponentAccessor<ComponentB> compBAccessor = chunk.GetComponentAccessor<ComponentB>();
            EcsArchetypeComponentAccessor<ComponentC> compCAccessor = chunk.GetComponentAccessor<ComponentC>();
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
            
            world.Dispose();
        }
        
        [Test]
        public void ChangeArchetypeAfterRemoveComponentTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkSizeInByte = 64});
            EcsEntity entity0 = world.CreateEntity(new ComponentA { value = 0 }, new ComponentB { value = 1 }, new ComponentC {value = 2});
            EcsEntity entity1 = world.CreateEntity(new ComponentA { value = 3 }, new ComponentB { value = 4 }, new ComponentC {value = 5});
            EcsEntity entity2 = world.CreateEntity(new ComponentA { value = 6 }, new ComponentB { value = 7 }, new ComponentC {value = 8});
            EcsEntity entity3 = world.CreateEntity(new ComponentA { value = 9 }, new ComponentB { value = 10 }, new ComponentC {value = 11});
            EcsEntity entity4 = world.CreateEntity(new ComponentA { value = 12 }, new ComponentB { value = 13 }, new ComponentC {value = 14});
            EcsEntity entity5 = world.CreateEntity(new ComponentA { value = 15 }, new ComponentB { value = 16 }, new ComponentC {value = 17});
            
            world.RemoveComponent<ComponentA>(entity0);
            world.RemoveComponent<ComponentB>(entity4);
            world.RemoveComponent<ComponentC>(entity2);

            EcsArchetype archetype = world.GetArchetype<ComponentA, ComponentB, ComponentC>();
            Assert.AreEqual(1, archetype.chunksCount);
            
            EcsArchetypeChunkAccessor chunk = archetype.GetChunk(0);
            EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
            EcsArchetypeComponentAccessor<ComponentA> compAAccessor = chunk.GetComponentAccessor<ComponentA>();
            EcsArchetypeComponentAccessor<ComponentB> compBAccessor = chunk.GetComponentAccessor<ComponentB>();
            EcsArchetypeComponentAccessor<ComponentC> compCAccessor = chunk.GetComponentAccessor<ComponentC>();
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
            
            
            archetype = world.GetArchetype<ComponentB, ComponentC>();
            Assert.AreEqual(1, archetype.chunksCount);
            
            chunk = archetype.GetChunk(0);
            entityAccessor = chunk.GetEntityAccessor();
            compBAccessor = chunk.GetComponentAccessor<ComponentB>();
            compCAccessor = chunk.GetComponentAccessor<ComponentC>();
            Assert.AreEqual(1, chunk.count);
            Assert.AreEqual(entity0, entityAccessor[0]);
            Assert.AreEqual(new ComponentB { value = 1 }, compBAccessor[0]);
            Assert.AreEqual(new ComponentC { value = 2 }, compCAccessor[0]);
            
            
            archetype = world.GetArchetype<ComponentA, ComponentC>();
            Assert.AreEqual(1, archetype.chunksCount);
            chunk = archetype.GetChunk(0);
            entityAccessor = chunk.GetEntityAccessor();
            compAAccessor = chunk.GetComponentAccessor<ComponentA>();
            compCAccessor = chunk.GetComponentAccessor<ComponentC>();
            Assert.AreEqual(1, chunk.count);
            Assert.AreEqual(entity4, entityAccessor[0]);
            Assert.AreEqual(new ComponentA { value = 12 }, compAAccessor[0]);
            Assert.AreEqual(new ComponentC { value = 14 }, compCAccessor[0]);

            
            archetype = world.GetArchetype<ComponentA, ComponentB>();
            Assert.AreEqual(1, archetype.chunksCount);
            chunk = archetype.GetChunk(0);
            entityAccessor = chunk.GetEntityAccessor();
            compAAccessor = chunk.GetComponentAccessor<ComponentA>();
            compBAccessor = chunk.GetComponentAccessor<ComponentB>();
            Assert.AreEqual(1, chunk.count);
            Assert.AreEqual(entity2, entityAccessor[0]);
            Assert.AreEqual(new ComponentA { value = 6 }, compAAccessor[0]);
            Assert.AreEqual(new ComponentB { value = 7 }, compBAccessor[0]);
            
            world.Dispose();
        }
        
        [Test]
        public void ChangeArchetypeAfterAddComponentTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting {archetypeChunkSizeInByte = 64});
            EcsEntity entity0 = world.CreateEntity(new ComponentA { value = 0 }, new ComponentC {value = 2});
            EcsEntity entity1 = world.CreateEntity(new ComponentA { value = 3 }, new ComponentC {value = 5});
            EcsEntity entity2 = world.CreateEntity(new ComponentA { value = 6 }, new ComponentC {value = 8});
            EcsEntity entity3 = world.CreateEntity(new ComponentA { value = 9 }, new ComponentC {value = 11});
            EcsEntity entity4 = world.CreateEntity(new ComponentA { value = 12 }, new ComponentC {value = 14});
            EcsEntity entity5 = world.CreateEntity(new ComponentA { value = 15 }, new ComponentC {value = 17});

            EcsArchetype archetype = world.GetArchetype<ComponentA, ComponentC>();
            Assert.AreEqual(2, archetype.chunksCount);
            Assert.AreEqual(4, archetype.GetChunk(0).count);
            Assert.AreEqual(2, archetype.GetChunk(1).count);
            
            world.AddComponent(entity0, new ComponentB { value = 1 });
            world.AddComponent(entity1, new ComponentB { value = 4 });
            world.AddComponent(entity2, new ComponentB { value = 7 });
            world.AddComponent(entity3, new ComponentB { value = 10 });
            world.AddComponent(entity4, new ComponentB { value = 13 });
            world.AddComponent(entity5, new ComponentB { value = 16 });
            
            archetype = world.GetArchetype<ComponentA, ComponentC>();
            Assert.AreEqual(1, archetype.chunksCount);
            Assert.AreEqual(0, archetype.GetChunk(0).count);


            archetype = world.GetArchetype<ComponentA, ComponentB, ComponentC>();
            Assert.AreEqual(2, archetype.chunksCount);
            EcsArchetypeChunkAccessor chunk = archetype.GetChunk(0);
            EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
            EcsArchetypeComponentAccessor<ComponentA> compAAccessor = chunk.GetComponentAccessor<ComponentA>();
            EcsArchetypeComponentAccessor<ComponentB> compBAccessor = chunk.GetComponentAccessor<ComponentB>();
            EcsArchetypeComponentAccessor<ComponentC> compCAccessor = chunk.GetComponentAccessor<ComponentC>();
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
            
            chunk = archetype.GetChunk(1);
            entityAccessor = chunk.GetEntityAccessor();
            compAAccessor = chunk.GetComponentAccessor<ComponentA>();
            compBAccessor = chunk.GetComponentAccessor<ComponentB>();
            compCAccessor = chunk.GetComponentAccessor<ComponentC>();
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
            
            world.Dispose();
        }
        
        // [Test]
        // public void ChangeArchetypeByAddRemoveComponentsTest()
        // {
        //     EcsWorld world = new EcsWorld();
        //     Assert.AreEqual(1, world.archetypeCount); // empty
        //
        //     EcsEntity entity0 = world.CreateEntity(new ComponentA { value = 1 }, new ComponentB { value = 5 });
        //     EcsEntity entity1 = world.CreateEntity(new ComponentA { value = 2 }, new ComponentB { value = 6 });
        //     EcsEntity entity2 = world.CreateEntity(new ComponentA { value = 3 }, new ComponentB { value = 7 });
        //     EcsEntity entity3 = world.CreateEntity(new ComponentA { value = 4 }, new ComponentB { value = 8 });
        //     /* [AB], [AB], [AB], [AB] */
        //     
        //     Assert.AreEqual(3, world.archetypeCount); // empty, A, AB
        //     Assert.AreEqual(4, world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>()).CalculateCount());
        //     
        //     EcsArchetype archetype = world.GetArchetype<ComponentA, ComponentB>();
        //     Assert.AreEqual(2, archetype.chunksCount);
        //     
        //     EcsArchetypeChunkAccessor chunk = archetype.GetChunk(0);
        //     EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
        //     EcsArchetypeComponentAccessor<ComponentA> compAAccessor = chunk.GetComponentAccessor<ComponentA>();
        //     EcsArchetypeComponentAccessor<ComponentB> compBAccessor = chunk.GetComponentAccessor<ComponentB>();
        //     Assert.AreEqual(2, chunk.count);
        //     Assert.AreEqual(entity0, entityAccessor[0]);
        //     Assert.AreEqual(new ComponentA { value = 1 }, compAAccessor[0]);
        //     Assert.AreEqual(new ComponentB { value = 5 }, compBAccessor[0]);
        //     Assert.AreEqual(entity1, entityAccessor[1]);
        //     Assert.AreEqual(new ComponentA { value = 2 }, compAAccessor[1]);
        //     Assert.AreEqual(new ComponentB { value = 6 }, compBAccessor[1]);
        //     
        //     chunk = archetype.GetChunk(1);
        //     entityAccessor = chunk.GetEntityAccessor();
        //     compAAccessor = chunk.GetComponentAccessor<ComponentA>();
        //     compBAccessor = chunk.GetComponentAccessor<ComponentB>();
        //     Assert.AreEqual(2, chunk.count);
        //     Assert.AreEqual(entity2, entityAccessor[0]);
        //     Assert.AreEqual(new ComponentA { value = 3 }, compAAccessor[0]);
        //     Assert.AreEqual(new ComponentB { value = 7 }, compBAccessor[0]);
        //     Assert.AreEqual(entity3, entityAccessor[1]);
        //     Assert.AreEqual(new ComponentA { value = 4 }, compAAccessor[1]);
        //     Assert.AreEqual(new ComponentB { value = 8 }, compBAccessor[1]);
        //
        //     
        //     
        //     world.RemoveComponent<ComponentA>(entity1);            
        //     world.RemoveComponent<ComponentA>(entity2);
        //     Assert.AreEqual(5, world.GetComponent<ComponentB>(entity0).value);
        //     /* [AB], [B], [B], [AB] */
        //     
        //     
        //     archetype = world.GetArchetype<ComponentA, ComponentB>();
        //     Assert.AreEqual(1, archetype.chunksCount);
        //     chunk = archetype.GetChunk(0);
        //     entityAccessor = chunk.GetEntityAccessor();
        //     compAAccessor = chunk.GetComponentAccessor<ComponentA>();
        //     compBAccessor = chunk.GetComponentAccessor<ComponentB>();
        //     Assert.AreEqual(2, chunk.count);
        //     Assert.AreEqual(entity0, entityAccessor[0]);
        //     Assert.AreEqual(new ComponentA { value = 1 }, compAAccessor[0]);
        //     Assert.AreEqual(new ComponentB { value = 5 }, compBAccessor[0]);
        //     // Assert.AreEqual(entity3, entityAccessor[1]);
        //     // Assert.AreEqual(new ComponentA { value = 4 }, compAAccessor[1]);
        //     // Assert.AreEqual(new ComponentB { value = 8 }, compBAccessor[1]);
        //     
        //     
        //     
        //     
        //     archetype = world.GetArchetype<ComponentB>();
        //     Assert.AreEqual(1, archetype.chunksCount);
        //     
        //     chunk = archetype.GetChunk(0);
        //     entityAccessor = chunk.GetEntityAccessor();
        //     compBAccessor = chunk.GetComponentAccessor<ComponentB>();
        //     Assert.AreEqual(2, chunk.count);
        //     Assert.AreEqual(entity1, entityAccessor[0]);
        //     Assert.AreEqual(new ComponentB { value = 6 }, compBAccessor[0]);
        //     
        //     // chunk = archetype.GetChunk(1);
        //     // entityAccessor = chunk.GetEntityAccessor();
        //     // compAAccessor = chunk.GetComponentAccessor<ComponentA>();
        //     // compBAccessor = chunk.GetComponentAccessor<ComponentB>();
        //     // Assert.AreEqual(2, chunk.count);
        //     // Assert.AreEqual(entity2, entityAccessor[0]);
        //     // Assert.AreEqual(new ComponentA { value = 3 }, compAAccessor[0]);
        //     // Assert.AreEqual(new ComponentB { value = 7 }, compBAccessor[0]);
        //     // Assert.AreEqual(entity3, entityAccessor[1]);
        //     // Assert.AreEqual(new ComponentA { value = 4 }, compAAccessor[1]);
        //     // Assert.AreEqual(new ComponentB { value = 8 }, compBAccessor[1]);
        //     //
        //     
        //     
        //     // Assert.AreEqual(5, world.GetComponent<ComponentB>(entity0).value);
        //     // Assert.AreEqual(6, world.GetComponent<ComponentB>(entity1).value);
        //     // Assert.AreEqual(7, world.GetComponent<ComponentB>(entity2).value);
        //     // Assert.AreEqual(8, world.GetComponent<ComponentB>(entity3).value);
        //     //
        //     Assert.AreEqual(4, world.archetypeCount); // empty, A, B, AB
        //     Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>()).CalculateCount());
        //     Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
        //     Assert.AreEqual(4, world.Filter(new EcsFilter().AllOf<ComponentB>()).CalculateCount());
        //     
        //     world.RemoveComponent<ComponentA>(entity0);
        //     /* [B], [B], [B], [AB] */
        //     
        //     Assert.AreEqual(4, world.archetypeCount); // empty, A, B, AB
        //     Assert.AreEqual(1, world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>()).CalculateCount());
        //     Assert.AreEqual(1, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
        //     Assert.AreEqual(4, world.Filter(new EcsFilter().AllOf<ComponentB>()).CalculateCount());
        //     
        //     world.AddComponent(entity1, new ComponentA()); 
        //     world.AddComponent(entity2, new ComponentA());
        //     /* [B], [AB], [AB], [AB] */
        //     
        //     Assert.AreEqual(4, world.archetypeCount); // empty, A, B, AB
        //     Assert.AreEqual(3, world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>()).CalculateCount());
        //     Assert.AreEqual(3, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
        //     Assert.AreEqual(4, world.Filter(new EcsFilter().AllOf<ComponentB>()).CalculateCount());
        //     
        //     world.RemoveComponent<ComponentA>(entity3);
        //     world.RemoveComponent<ComponentB>(entity3);
        //     /* [B], [AB], [], [AB] */
        //     
        //     Assert.AreEqual(4, world.archetypeCount); // empty, A, B, AB
        //     Assert.AreEqual(1, world.Filter(new EcsFilter().NoneOf<ComponentA, ComponentB>()).CalculateCount());
        //     Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>()).CalculateCount());
        //     Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
        //     Assert.AreEqual(3, world.Filter(new EcsFilter().AllOf<ComponentB>()).CalculateCount());
        //     
        //     world.AddComponent(entity3, new ComponentC());
        //     /* [B], [AB], [C], [AB] */
        //     
        //     Assert.AreEqual(5, world.archetypeCount); // empty, A, B, AB, C
        //     Assert.AreEqual(1, world.Filter(new EcsFilter().NoneOf<ComponentA, ComponentB>()).CalculateCount());
        //     Assert.AreEqual(1, world.Filter(new EcsFilter().AllOf<ComponentC>()).CalculateCount());
        //     Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>()).CalculateCount());
        //     Assert.AreEqual(2, world.Filter(new EcsFilter().AllOf<ComponentA>()).CalculateCount());
        //     Assert.AreEqual(3, world.Filter(new EcsFilter().AllOf<ComponentB>()).CalculateCount());
        //     
        //     world.Dispose();
        // }

        // [Test]
        // public unsafe void ComponentAccessorTest()
        // {
        //     EcsWorld world = new EcsWorld();
        //     EcsComponentAccessor<ComponentA> compsA = world.GetComponentAccessor<ComponentA>();
        //     EcsComponentAccessor<ComponentB> compsB = world.GetComponentAccessor<ComponentB>();
        //     
        //     world.CreateEntity(new ComponentA(), new ComponentB { value = 1});
        //     int sumA = 0;
        //     int sumB = 0;
        //
        //     EcsArchetypeGroup archetypeGroup = world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>());
        //     EcsArchetypeAccessor archetypeAccessor = *archetypeGroup.GetEntityAccessorPtr();
        //     foreach (EcsArchetypeChunkAccessor chunk in archetypeAccessor)
        //     {
        //         EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
        //         for (int i = 0; i < chunk.count; i++)
        //         {
        //             EcsEntity e = entityAccessor[i];
        //             sumA += compsA[e].value;
        //             sumB += compsB[e].value;
        //             compsA[e] = new ComponentA { value = 1 };
        //         }
        //     }
        //
        //     Assert.AreEqual(0, sumA);
        //     Assert.AreEqual(1, sumB);
        //     
        //     world.CreateEntity(new ComponentA {value = 1}, new ComponentB { value = 1});
        //
        //     sumA = 0;
        //     sumB = 0;
        //     archetypeGroup = world.Filter(new EcsFilter().AllOf<ComponentB>());
        //     archetypeAccessor = *archetypeGroup.GetEntityAccessorPtr();
        //     foreach (EcsArchetypeChunkAccessor chunk in archetypeAccessor)
        //     {
        //         EcsArchetypeEntityAccessor entityAccessor = chunk.GetEntityAccessor();
        //         for (int i = 0; i < chunk.count; i++)
        //         {
        //             EcsEntity e = entityAccessor[i];
        //             sumA += compsA[e].value;
        //             sumB += compsB[e].value;
        //         }
        //     }
        //
        //     Assert.AreEqual(2, sumA);
        //     Assert.AreEqual(2, sumB);
        //     
        //     world.Dispose();
        // }
    }
}