using Qwerty.ECS.Runtime.Archetypes;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    public partial class EcsWorld
    {
        public EcsArchetypeGroup Filter(EcsFilter filter)
        {
            int version = m_arcManager.count;
            if (m_archGroups.TryGetValue(filter, out EcsArchetypeGroup group) && group.Version >= version)
            {
                return group;
            }

            if (group == null)
            {
                group = new EcsArchetypeGroup();
                m_archGroups.Add(filter.Clone(), group);
            }

            int count = m_arcManager.count;
            for (int i = group.Version; i < count; i++)
            {
                EcsArchetype archetype = m_arcManager[i];
                int[] typeIndices = archetype.indices;
                if (None(typeIndices, filter.none) && Any(typeIndices, filter.any) && All(typeIndices, filter.all))
                {
                    group.archetypes.Add(archetype);
                }
            }

            group.ChangeVersion(version);

            return group;
        }

        private static bool All(int[] source, int[] target)
        {
            int cnt = 0, i = 0, j = 0;
            int sourceLength = source.Length;
            int targetLength = target.Length;
            while (i < sourceLength && j < targetLength)
            {
                int s = source[i];
                int t = target[j];

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

        private static bool Any(int[] source, int[] target)
        {
            int i = 0, j = 0;
            int sourceLength = source.Length;
            int targetLength = target.Length;
            while (i < sourceLength && j < targetLength)
            {
                int s = source[i];
                int t = target[j];

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

        private static bool None(int[] source, int[] target)
        {
            int i = 0, j = 0;
            int sourceLength = source.Length;
            int targetLength = target.Length;
            while (i < sourceLength && j < targetLength)
            {
                int s = source[i];
                int t = target[j];

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