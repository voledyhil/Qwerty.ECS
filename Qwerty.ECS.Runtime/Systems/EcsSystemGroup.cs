namespace Qwerty.ECS.Runtime.Systems
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for systems or other groups of systems.
    /// Using the attributes <see cref="T:MiniEcs.Core.Systems.EcsUpdateAfterAttribute" /> and <see cref="T:MiniEcs.Core.Systems.EcsUpdateAfterAttribute" />,
    /// the order of updating the nodes in the group is indicated.
    /// Using the attribute <see cref="T:MiniEcs.Core.Systems.EcsUpdateInGroupAttribute" /> can be added to the parent group.
    /// </summary>
    public class EcsSystemGroup : IEcsSystem
    {
        public IEnumerable<IEcsSystem> Systems => m_systems;

        /// <summary>
        /// Indicates that after updating the list, nodes should be sorted.
        /// </summary>
        private bool _dirty;
        
        private readonly List<IEcsSystem> m_systems = new List<IEcsSystem>();
        public void AddSystem(IEcsSystem system)
        {
            AddSystem(GetSystemGroupHierarchy(system.GetType()), system);
        }

        /// <summary>
        /// Adds a system or group of systems to the list
        /// </summary>
        /// <param name="groupHierarchy">Parent group hierarchy</param>
        /// <param name="system">System or group of systems</param>
        private void AddSystem(Stack<Type> groupHierarchy, IEcsSystem system)
        {
            if (groupHierarchy.Count > 0)
            {
                Type parentType = groupHierarchy.Pop();

                if (!TryGetSystemGroup(parentType, out EcsSystemGroup systemGroup))
                {
                    systemGroup = (EcsSystemGroup) Activator.CreateInstance(parentType);

                    m_systems.Add(systemGroup);
                    _dirty = true;
                }

                systemGroup.AddSystem(groupHierarchy, system);
            }
            else
            {
                m_systems.Add(system);
                _dirty = true;
            }
        }

        /// <summary>
        /// Gets the parent group hierarchy
        /// </summary>
        /// <param name="type">Node type</param>
        /// <returns>Stack of parent nodes</returns>
        private static Stack<Type> GetSystemGroupHierarchy(Type type)
        {
            Stack<Type> groupsType = new Stack<Type>();
            EcsUpdateInGroupAttribute attribute =
                (EcsUpdateInGroupAttribute) Attribute.GetCustomAttribute(type, typeof(EcsUpdateInGroupAttribute));

            while (attribute != null)
            {
                groupsType.Push(attribute.Type);
                attribute = (EcsUpdateInGroupAttribute) Attribute.GetCustomAttribute(attribute.Type,
                    typeof(EcsUpdateInGroupAttribute));
            }

            return groupsType;
        }

        /// <summary>
        /// Trying to find a node of the specified type
        /// </summary>
        /// <param name="type">Node type</param>
        /// <param name="systemGroup">found node</param>
        /// <returns>true - the node exists. false - node not found</returns>
        private bool TryGetSystemGroup(Type type, out EcsSystemGroup systemGroup)
        {
            systemGroup = null;
            foreach (IEcsSystem ecsSystem in m_systems)
            {
                if (ecsSystem.GetType() != type)
                    continue;

                systemGroup = (EcsSystemGroup) ecsSystem;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the list of systems and system groups.
        /// Before updating, sorts the items in the list, if necessary.
        /// </summary>
        /// <param name="deltaTime">Elapsed time since last update</param>
        /// <param name="world">Entity Manager <see cref="EcsWorld"/></param>
        public void Update(float deltaTime, EcsWorld world)
        {
            if (_dirty)
            {
                EcsSystemSorter.Sort(m_systems);
                _dirty = false;
            }

            foreach (IEcsSystem system in m_systems)
            {
                system.Update(deltaTime, world);
            }
        }
    }
}