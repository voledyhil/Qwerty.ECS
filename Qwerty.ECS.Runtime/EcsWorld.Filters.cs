using System.Runtime.CompilerServices;
using Qwerty.ECS.Runtime.Archetypes;

namespace Qwerty.ECS.Runtime
{
    public partial class EcsWorld
    {
        public EcsArchetypeGroup Filter(EcsFilter filter)
        {
            int version = m_archetypeManager.archetypeCount - 1;
            if (m_archetypeGroups.TryGetValue(filter, out EcsArchetypeGroup result))
            {
                if (result.Version >= version)
                {
                    return result;
                }
            }

            byte[] all = filter.All?.ToArray();
            byte[] any = filter.Any?.ToArray();
            byte[] none = filter.None?.ToArray();

            if (result != null)
            {
                result.Update(version, GetArchetypes(all, any, none, result.Version));
                return result;
            }

            result = new EcsArchetypeGroup(version, GetArchetypes(all, any, none, 0));
            m_archetypeGroups.Add(filter.Clone(), result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<EcsArchetype> GetArchetypes(byte[] all, byte[] any, byte[] none, int startId)
        {
            HashSet<EcsArchetype> buffer0 = null;
            HashSet<EcsArchetype> buffer1 = null;

            if (all != null || any != null)
            {
                if (all != null)
                {
                    IReadOnlyList<EcsArchetype>[] archetypes = new IReadOnlyList<EcsArchetype>[all.Length];
                    for (int i = 0; i < all.Length; i++)
                    {
                        int len = m_archetypeManager.GetArchetypes(all[i], startId, m_archetypes);
                        EcsArchetype[] items = new EcsArchetype[len];
                        for (int j = 0; j < len; j++)
                        {
                            items[j] = m_archetypes[j];
                        }

                        archetypes[i] = items;
                    }

                    Array.Sort(archetypes, (a, b) => a.Count - b.Count);

                    buffer0 = new HashSet<EcsArchetype>(archetypes[0]);
                    for (int i = 1; i < all.Length; i++)
                    {
                        buffer0.IntersectWith(archetypes[i]);
                    }
                }

                if (any != null)
                {
                    buffer1 = new HashSet<EcsArchetype>();
                    foreach (byte typeIndex in any)
                    {
                        int len = m_archetypeManager.GetArchetypes(typeIndex, startId, m_archetypes);
                        for (int j = 0; j < len; j++)
                        {
                            buffer1.Add(m_archetypes[j]);
                        }
                    }
                }

                if (buffer0 != null && buffer1 != null)
                {
                    buffer0.IntersectWith(buffer1);
                }
                else if (buffer1 != null)
                {
                    buffer0 = buffer1;
                }
            }
            else
            {
                buffer0 = new HashSet<EcsArchetype>();
                int len = m_archetypeManager.GetArchetypes(startId, m_archetypes);
                for (int i = 0; i < len; i++)
                {
                    buffer0.Add(m_archetypes[i]);
                }
            }

            if (none != null)
            {
                foreach (byte type in none)
                {
                    int len = m_archetypeManager.GetArchetypes(type, startId, m_archetypes);
                    for (int i = 0; i < len; i++)
                    {
                        buffer0.Remove(m_archetypes[i]);
                    }
                }
            }

            return buffer0;
        }
    }
}