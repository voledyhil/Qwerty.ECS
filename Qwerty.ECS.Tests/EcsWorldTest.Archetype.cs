using NUnit.Framework;
using Qwerty.ECS.Runtime;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Tests
{
    public partial class EcsWorldTest
    {
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
        public unsafe void ComponentAccessorTest()
        {
            EcsWorld world = new EcsWorld();
            EcsComponentAccessor<ComponentA> compsA = world.GetComponentAccessor<ComponentA>();
            EcsComponentAccessor<ComponentB> compsB = world.GetComponentAccessor<ComponentB>();
            
            world.CreateEntity(new ComponentA(), new ComponentB { value = 1});
            int sumA = 0;
            int sumB = 0;

            EcsArchetypeGroup archetypeGroup = world.Filter(new EcsFilter().AllOf<ComponentA, ComponentB>());
            EcsArchetypesAccessor accessor = *archetypeGroup.GetArchetypeAccessorPtr();
            for (int i = 0; i < accessor.ArchetypeCount; i++)
            {
                
                EcsEntityCollection entities = accessor.GetEntityArray(i);
                for (int j = 0; j < entities.count; j++)
                {
                    EcsEntity e = entities[j];
                    sumA += compsA[e].value;
                    sumB += compsB[e].value;
                    compsA[e] = new ComponentA { value = 1 };
                }
            }
            
            Assert.AreEqual(0, sumA);
            Assert.AreEqual(1, sumB);
            
            world.CreateEntity(new ComponentA {value = 1}, new ComponentB { value = 1});

            sumA = 0;
            sumB = 0;
            archetypeGroup = world.Filter(new EcsFilter().AllOf<ComponentB>());
            accessor = *archetypeGroup.GetArchetypeAccessorPtr();
            for (int i = 0; i < accessor.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = accessor.GetEntityArray(i);
                for (int j = 0; j < entities.count; j++)
                {
                    EcsEntity e = entities[j];
                    sumA += compsA[e].value;
                    sumB += compsB[e].value;
                }
            }
            Assert.AreEqual(2, sumA);
            Assert.AreEqual(2, sumB);
        }
    }
}