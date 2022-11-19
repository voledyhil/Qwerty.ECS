using Qwerty.ECS.Runtime.Archetypes;

namespace Qwerty.ECS.Runtime
{
    public partial class EcsWorld
    {
        public EcsArchetypeGroup Filter(EcsFilter filter)
        {
            int version = m_archetypeManager.archetypeCount;
            if (m_archetypeGroups.TryGetValue(filter, out EcsArchetypeGroup group))
            {
                if (group.Version >= version)
                {
                    return group;
                }
            }
            
            if (group == null)
            {
                group = new EcsArchetypeGroup();
                m_archetypeGroups.Add(filter.Clone(), group);
            }
            
            byte[] all = filter.all.ToArray();
            byte[] any = filter.any.ToArray();
            byte[] none = filter.none.ToArray();
            
            Array.Sort(all);
            Array.Sort(any);
            Array.Sort(none);
            
            for (int i = group.Version; i < m_archetypeManager.archetypeCount; i++)
            {
                EcsArchetype archetype = m_archetypeManager[i];
                byte[] typeIndices = archetype.TypeIndices;
                
                if (None(typeIndices, none) && Any(typeIndices, any) && All(typeIndices, all))
                {
                    group.archetypes.Add(archetype);
                }
            }
            group.ChangeVersion(version);
            
            return group;
        }
        
        private static bool All(byte[] source, byte[] target)
        {
            int cnt = 0, i = 0, j = 0;
            int sourceLength = source.Length;
            int targetLength = target.Length;
            while (i < sourceLength && j < targetLength)
            {
                if (source[i] < target[j])
                {
                    i++;
                }
                else if (target[j] < source[i])
                {
                    j++;
                }
                else
                {
                    cnt++;
                    i++;
                }
            }
            return cnt == targetLength;
        }
        
        private static bool Any(byte[] source, byte[] target)
        {
            int i = 0, j = 0;
            int sourceLength = source.Length;
            int targetLength = target.Length;
            while (i < sourceLength && j < targetLength)
            {
                if (source[i] < target[j])
                {
                    i++;
                }
                else if (target[j] < source[i])
                {
                    j++;
                }
                else
                {
                    return true;
                }
            }
            return targetLength == 0;
        }
        
        private static bool None(byte[] source, byte[] target)
        {
            int i = 0, j = 0;
            int sourceLength = source.Length;
            int targetLength = target.Length;
            while (i < sourceLength && j < targetLength)
            {
                if (source[i] < target[j])
                {
                    i++;
                }
                else if (target[j] < source[i])
                {
                    j++;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}