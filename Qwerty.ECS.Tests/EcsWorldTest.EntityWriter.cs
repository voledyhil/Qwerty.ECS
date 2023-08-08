using NUnit.Framework;
using Qwerty.ECS.Runtime;
using Qwerty.ECS.Runtime.Archetypes;
using Qwerty.ECS.Runtime.Chunks;
using Qwerty.ECS.Runtime.Components;

namespace Qwerty.ECS.Tests
{
    public partial class EcsWorldTest
    {
        [Test]
        public void EntityWriterTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting());

            EcsEntityWriter<ComponentC, ComponentA3, ComponentB> entityWriter = world.InstantiateEntityWriter<ComponentC, ComponentA3, ComponentB>(3);
            entityWriter.Write(new ComponentC { value = 1 }, new ComponentA3 { x = 2, y = 3, z = 4 }, new ComponentB { value = 5 });
            entityWriter.Write(new ComponentC { value = 6 }, new ComponentA3 { x = 7, y = 8, z = 9 }, new ComponentB { value = 10 });
            entityWriter.Write(new ComponentC { value = 11 }, new ComponentA3 { x = 12, y = 13, z = 14 }, new ComponentB { value = 15 });
            world.Write(entityWriter);
            entityWriter.Dispose();

            EcsArchetypeGroup group = world.Filter(new EcsFilter().AllOf<ComponentC, ComponentA3, ComponentB>());
            EcsArchetypeGroupAccessor groupAccessor = group.GetGroupAccessor();
            EcsComponentTypeHandle<ComponentA3> compA3TypeHandle = world.GetComponentTypeHandle<ComponentA3>();
            EcsComponentTypeHandle<ComponentB> compBTypeHandle = world.GetComponentTypeHandle<ComponentB>();
            EcsComponentTypeHandle<ComponentC> compCTypeHandle = world.GetComponentTypeHandle<ComponentC>();

            int index = 0;
            foreach (EcsChunkAccessor chunkAccessor in groupAccessor)
            {
                EcsChunkEntityAccessor entityAccessor = chunkAccessor.GetEntityAccessor();
                EcsChunkComponentAccessor<ComponentA3> compsA3 = chunkAccessor.GetComponentAccessor(compA3TypeHandle);
                EcsChunkComponentAccessor<ComponentB> compsB = chunkAccessor.GetComponentAccessor(compBTypeHandle);
                EcsChunkComponentAccessor<ComponentC> compsC3 = chunkAccessor.GetComponentAccessor(compCTypeHandle);

                for (int i = 0; i < chunkAccessor.count; i++)
                {
                    switch (index++)
                    {
                        case 0:
                            Assert.AreEqual(new EcsEntity(1, 1), entityAccessor[i]);
                            Assert.AreEqual(1, compsC3[i].value);
                            Assert.AreEqual(2, compsA3[i].x);
                            Assert.AreEqual(3, compsA3[i].y);
                            Assert.AreEqual(4, compsA3[i].z);
                            Assert.AreEqual(5, compsB[i].value);
                            break;
                        case 1:
                            Assert.AreEqual(new EcsEntity(2, 1), entityAccessor[i]);
                            Assert.AreEqual(6, compsC3[i].value);
                            Assert.AreEqual(7, compsA3[i].x);
                            Assert.AreEqual(8, compsA3[i].y);
                            Assert.AreEqual(9, compsA3[i].z);
                            Assert.AreEqual(10, compsB[i].value);
                            break;
                        case 2:
                            Assert.AreEqual(new EcsEntity(3, 1), entityAccessor[i]);
                            Assert.AreEqual(11, compsC3[i].value);
                            Assert.AreEqual(12, compsA3[i].x);
                            Assert.AreEqual(13, compsA3[i].y);
                            Assert.AreEqual(14, compsA3[i].z);
                            Assert.AreEqual(15, compsB[i].value);
                            break;
                    }
                }
            }
        }
    }
}