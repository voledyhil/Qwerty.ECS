using Qwerty.ECS.Runtime.Archetypes;

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
                byte[] typeIndices = archetype.indices;
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