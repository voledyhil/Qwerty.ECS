using Qwerty.ECS.Runtime.Archetypes;

namespace Qwerty.ECS.Runtime
{
    public partial class EcsWorld
    {
        public EcsArchetypeGroup Filter(EcsFilter filter)
        {
            int version = m_archetypeManager.archetypeCounter;
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

            for (int i = group.Version; i < m_archetypeManager.archetypeCounter; i++)
            {
                EcsArchetype archetype = m_archetypeManager[i];
                byte[] typeIndices = archetype.typeIndices;

                if (None(typeIndices, filter.none) && Any(typeIndices, filter.any) && All(typeIndices, filter.all))
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
                byte s = source[i];
                byte t = target[j];

                if (s < t)
                {
                    i++;
                    continue;
                }

                if (t < s)
                {
                    j++;
                    continue;
                }

                cnt++;
                i++;
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
                byte s = source[i];
                byte t = target[j];

                if (s < t)
                {
                    i++;
                    continue;
                }

                if (t < s)
                {
                    j++;
                    continue;
                }

                return true;
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
                byte s = source[i];
                byte t = target[j];

                if (s < t)
                {
                    i++;
                    continue;
                }

                if (t < s)
                {
                    j++;
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}