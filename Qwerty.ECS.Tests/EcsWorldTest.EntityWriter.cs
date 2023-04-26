using NUnit.Framework;
using Qwerty.ECS.Runtime;

namespace Qwerty.ECS.Tests
{
    public partial class EcsWorldTest
    {
        [Test]
        public void EntityWriterTest()
        {
            EcsWorld world = new EcsWorld(new EcsWorldSetting());
            
            EcsEntityWriter<ComponentC, ComponentA3, ComponentB> entityWriter = world.InstantiateEntityWriter<ComponentC, ComponentA3, ComponentB>(5);
            entityWriter.Write(new ComponentC {value = 1}, new ComponentA3 { x = 2, y = 3, z = 4}, new ComponentB {value = 5});
            entityWriter.Write(new ComponentC {value = 6}, new ComponentA3 { x = 7, y = 8, z = 9}, new ComponentB {value = 10});
            entityWriter.Write(new ComponentC {value = 11}, new ComponentA3 { x = 12, y = 13, z = 14}, new ComponentB {value = 15});
            world.Write(entityWriter);
            entityWriter.Dispose();

            EcsEntity entity11 = new EcsEntity(1, 1);
            EcsEntity entity21 = new EcsEntity(2, 1);
            EcsEntity entity31 = new EcsEntity(3, 1);
            
            Assert.AreEqual(1, world.GetComponent<ComponentC>(entity11).value);
            Assert.AreEqual(2, world.GetComponent<ComponentA3>(entity11).x);
            Assert.AreEqual(3, world.GetComponent<ComponentA3>(entity11).y);
            Assert.AreEqual(4, world.GetComponent<ComponentA3>(entity11).z);
            Assert.AreEqual(5, world.GetComponent<ComponentB>(entity11).value);
            
            Assert.AreEqual(6, world.GetComponent<ComponentC>(entity21).value);
            Assert.AreEqual(7, world.GetComponent<ComponentA3>(entity21).x);
            Assert.AreEqual(8, world.GetComponent<ComponentA3>(entity21).y);
            Assert.AreEqual(9, world.GetComponent<ComponentA3>(entity21).z);
            Assert.AreEqual(10, world.GetComponent<ComponentB>(entity21).value);
            
            Assert.AreEqual(11, world.GetComponent<ComponentC>(entity31).value);
            Assert.AreEqual(12, world.GetComponent<ComponentA3>(entity31).x);
            Assert.AreEqual(13, world.GetComponent<ComponentA3>(entity31).y);
            Assert.AreEqual(14, world.GetComponent<ComponentA3>(entity31).z);
            Assert.AreEqual(15, world.GetComponent<ComponentB>(entity31).value);
        }
    }
}