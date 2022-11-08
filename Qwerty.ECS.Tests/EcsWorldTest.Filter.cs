using NUnit.Framework;
using Qwerty.ECS.Runtime;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Tests
{
    public partial class EcsWorldTest
    {
        [Test]
        public void All_ComponentB_Test()
        {
            int sumB = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity> { m_abd, m_bd1, m_bd2, m_bc, m_ab };

            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB>());
            EcsComponentAccessor<ComponentB> compsB = m_world.GetComponentAccessor<ComponentB>();
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);
                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity e = entities[j];
                    sumB += compsB[e].value;
                    actualEntities.Add(e);
                }
            }

            Assert.AreEqual(sumB, 37);
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
            EcsComponentAccessor<ComponentB> compsB = m_world.GetComponentAccessor<ComponentB>();
            EcsComponentAccessor<ComponentD> compsD = m_world.GetComponentAccessor<ComponentD>();

            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);

                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity e = entities[j];
                    sumB += compsB[e].value;
                    sumD += compsD[e].value;
                    actualEntities.Add(e);
                }
            }

            Assert.AreEqual(sumB, 16);
            Assert.AreEqual(sumD, 18);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }

        [Test]
        public void Any_ComponentB_Test()
        {
            int sumB = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd, m_bd1, m_bd2, m_bc, m_ab };

            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AnyOf<ComponentB>());
            EcsComponentAccessor<ComponentB> compsB = m_world.GetComponentAccessor<ComponentB>();
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);
                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity e = entities[j];
                    sumB += compsB[e].value;
                    actualEntities.Add(e);
                }
            }

            Assert.AreEqual(sumB, 37);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }

        [Test]
        public void Any_ComponentB_Or_ComponentD_Test()
        {
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd, m_bd1, m_bd2, m_bc, m_ab, m_ad };

            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AnyOf<ComponentB, ComponentD>());
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
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_ac };

            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().NoneOf<ComponentB, ComponentD>());
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
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_ac };

            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().NoneOf<ComponentB, ComponentB, ComponentD>());
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
            int sumA = 0;
            int sumB = 0;
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd };

            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB, ComponentB, ComponentD>().AnyOf<ComponentA>());
            EcsComponentAccessor<ComponentA> compsA = m_world.GetComponentAccessor<ComponentA>();
            EcsComponentAccessor<ComponentB> compsB = m_world.GetComponentAccessor<ComponentB>();
            EcsComponentAccessor<ComponentD> compsD = m_world.GetComponentAccessor<ComponentD>();

            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);
                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity e = entities[j];

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
        public void All_DoubleComponentD_AnyFilter_ComponentB_DoubleComponentC_Test()
        {
            int sumD = 0;
            HashSet<EcsEntity> actualEntities = new HashSet<EcsEntity>();
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_abd, m_bd1, m_bd2 };

            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentD, ComponentD>().AnyOf<ComponentB, ComponentC, ComponentC>());
            EcsComponentAccessor<ComponentD> compsD = m_world.GetComponentAccessor<ComponentD>();
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);
                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity e = entities[j];
                    sumD += compsD[e].value;
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
            HashSet<EcsEntity> expectedEntities = new HashSet<EcsEntity>() { m_bd1, m_bd2, m_bc };

            EcsArchetypeGroup archetypeGroup = m_world.Filter(new EcsFilter().AllOf<ComponentB>().NoneOf<ComponentA>());
            EcsComponentAccessor<ComponentB> compsB = m_world.GetComponentAccessor<ComponentB>();
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);

                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity e = entities[j];
                    sumB += compsB[e].value;
                    actualEntities.Add(e);
                }
            }

            Assert.AreEqual(sumB, 24);
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
            EcsComponentAccessor<ComponentB> compsB = m_world.GetComponentAccessor<ComponentB>();
            EcsComponentAccessor<ComponentD> compsD = m_world.GetComponentAccessor<ComponentD>();
            EcsArchetypesAccessor chunks = archetypeGroup.GetArchetypeAccessor();
            for (int i = 0; i < chunks.ArchetypeCount; i++)
            {
                EcsEntityCollection entities = chunks.GetEntityArray(i);

                int count = entities.count;
                for (int j = 0; j < count; j++)
                {
                    EcsEntity e = entities[j];
                    sumB += compsB[e].value;
                    sumD += compsD[e].value;
                    actualEntities.Add(e);
                }
            }

            Assert.AreEqual(sumB, 14);
            Assert.AreEqual(sumD, 15);
            Assert.IsTrue(expectedEntities.SetEquals(actualEntities));
        }
    }
}