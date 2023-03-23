using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Systems
{
    public abstract class EcsSystem
    {
        public double elapsedTotalMilliseconds { get; private set; }
        
        public void Update(float deltaTime, EcsWorld world)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            OnUpdate(deltaTime, world);
            
            stopwatch.Stop();
            elapsedTotalMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
        }

        protected abstract void OnUpdate(float deltaTime, EcsWorld world);
    }
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EcsUpdateAfterAttribute : Attribute
    {
        public Type Type { get; }
        public EcsUpdateAfterAttribute(Type type)
        {
            Type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EcsUpdateBeforeAttribute : Attribute
    {
        public Type Type { get; }
        public EcsUpdateBeforeAttribute(Type type)
        {
            Type = type;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class EcsUpdateInGroupAttribute : Attribute
    {
        public Type Type { get; }
        public EcsUpdateInGroupAttribute(Type type)
        {
            Type = type;
        }
    }
}