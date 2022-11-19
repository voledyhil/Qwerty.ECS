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
                
                if (None(typeIndices, typeIndices.Length, none, none.Length) &&
                    Any(typeIndices, typeIndices.Length, any, any.Length) &&
                    All(typeIndices, typeIndices.Length, all, all.Length))
                {
                    group.archetypes.Add(archetype);
                }
            }
            group.ChangeVersion(version);
            
            return group;
        }
        
        private static bool All(byte[] source, int sourceLen, byte[] target, int targetLen)
        {
            int cnt = 0, i = 0, j = 0;
            while (i < sourceLen && j < targetLen)
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
            return cnt == targetLen;
        }
        
        private static bool Any(byte[] source, int sourceLen, byte[] target, int targetLen)
        {
            int i = 0, j = 0;
            while (i < sourceLen && j < targetLen)
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
            return targetLen == 0;
        }
        
        private static bool None(byte[] source, int sourceLen, byte[] target, int targetLen)
        {
            int i = 0, j = 0;
            while (i < sourceLen && j < targetLen)
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