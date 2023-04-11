using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime
{
    internal readonly ref struct EcsTypeIndex<T> where T : struct
    {
        internal static int typeIndex;
        internal static bool isRegister;

        static EcsTypeIndex()
        {
            typeIndex = -1;
        }

        internal static EcsTypeIndex<T> value
        {
            get
            {
                if (!isRegister)
                {
                    throw new InvalidOperationException(nameof(value));
                }

                return new EcsTypeIndex<T>(typeIndex);
            }
        }

        public readonly int index;
        private EcsTypeIndex(int index)
        {
            this.index = index;
        }
    }

    public static class EcsTypeManager
    {
        public static int typeCount => Hashes.Count;
        internal static readonly Dictionary<int, int> Sizes = new Dictionary<int, int>();
        private static readonly Dictionary<int, string> Hashes = new Dictionary<int, string>();
        public static void Register<T>(string key) where T : struct
        {
            int index = GenerateIndex(key);
            if (!Hashes.TryGetValue(index, out string cachedKey))
            {
                EcsTypeIndex<T>.typeIndex = index;
                EcsTypeIndex<T>.isRegister = true;
                Sizes.Add(index, Unsafe.SizeOf<T>());
                Hashes.Add(index, key);
                return;
            }
            if (key == cachedKey) return;
            throw new ArgumentException(nameof(Register));
        }
        
        private const int Lower31BitMask = 0x7FFFFFFF;
        private static int GenerateIndex(string key)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;
                for (int i = 0; i < key.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ key[i];
                    if (i == key.Length - 1) break;
                    hash2 = ((hash2 << 5) + hash2) ^ key[i + 1];
                }
                return (hash1 + hash2 * 1566083941) & Lower31BitMask;
            }
        }
    }
}