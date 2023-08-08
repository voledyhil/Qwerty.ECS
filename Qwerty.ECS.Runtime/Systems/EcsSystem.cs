using System;
using System.Collections.Generic;
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
}