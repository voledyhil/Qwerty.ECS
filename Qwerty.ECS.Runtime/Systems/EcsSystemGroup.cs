using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Systems
{
    public class EcsSystemGroup : EcsSystem
    {
        public IReadOnlyList<EcsSystem> Systems => m_systems;
        
        private readonly List<EcsSystem> m_systems = new List<EcsSystem>();
        
        public void AddSystem(EcsSystem system)
        {
            m_systems.Add(system);
        }

        protected override void OnUpdate(float deltaTime, EcsWorld world)
        {
            foreach (EcsSystem system in m_systems)
            {
                system.Update(deltaTime, world);
            }
        }
    }
}