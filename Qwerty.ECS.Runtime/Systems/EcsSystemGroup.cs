using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Systems
{
    public class EcsSystemGroup : EcsSystem
    {
        public IReadOnlyList<EcsSystem> Systems => m_systems;
        
        private bool m_dirty;
        private readonly List<EcsSystem> m_systems = new List<EcsSystem>();
        
        public void AddSystem(EcsSystem system)
        {
            AddSystem(GetSystemGroupHierarchy(system.GetType()), system);
        }
        
        private void AddSystem(Stack<Type> groupHierarchy, EcsSystem system)
        {
            if (groupHierarchy.Count > 0)
            {
                Type parentType = groupHierarchy.Pop();
                if (!TryGetSystemGroup(parentType, out EcsSystemGroup systemGroup))
                {
                    systemGroup = (EcsSystemGroup) Activator.CreateInstance(parentType);

                    m_systems.Add(systemGroup);
                    m_dirty = true;
                }
                systemGroup.AddSystem(groupHierarchy, system);
            }
            else
            {
                m_systems.Add(system);
                m_dirty = true;
            }
        }
        
        private static Stack<Type> GetSystemGroupHierarchy(Type type)
        {
            Stack<Type> groupsType = new Stack<Type>();
            EcsUpdateInGroupAttribute attribute = (EcsUpdateInGroupAttribute) Attribute.GetCustomAttribute(type, typeof(EcsUpdateInGroupAttribute));

            while (attribute != null)
            {
                groupsType.Push(attribute.Type);
                attribute = (EcsUpdateInGroupAttribute) Attribute.GetCustomAttribute(attribute.Type, typeof(EcsUpdateInGroupAttribute));
            }

            return groupsType;
        }
        
        private bool TryGetSystemGroup(Type type, out EcsSystemGroup systemGroup)
        {
            systemGroup = null;
            foreach (EcsSystem ecsSystem in m_systems)
            {
                if (ecsSystem.GetType() != type)
                    continue;

                systemGroup = (EcsSystemGroup) ecsSystem;
                return true;
            }

            return false;
        }

        protected override void OnUpdate(float deltaTime, EcsWorld world)
        {
            if (m_dirty)
            {
                EcsSystemSorter.Sort(m_systems);
                m_dirty = false;
            }
            
            foreach (EcsSystem system in m_systems)
            {
                system.Update(deltaTime, world);
            }
        }
    }
}