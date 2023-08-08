// using System;
// using System.Collections.Generic;
//
// // ReSharper disable once CheckNamespace
// namespace Qwerty.ECS.Runtime.Systems
// {
//     public static class EcsSystemSorter
//     {
//         public static void Sort<T>(List<T> systems)
//         {
//             SystemDependencies<T>[] dependencies = new SystemDependencies<T>[systems.Count];
//             Dictionary<Type, int> systemsDict = new Dictionary<Type, int>();
//
//             for (int i = 0; i < systems.Count; i++)
//             {
//                 T system = systems[i];
//                 dependencies[i] = new SystemDependencies<T>
//                 {
//                     system = system,
//                     before = new List<Type>()
//                 };
//                 systemsDict[system.GetType()] = i;
//             }
//
//             for (int i = 0; i < dependencies.Length; i++)
//             {
//                 Type systemType = dependencies[i].system.GetType();
//                 Attribute[] before = Attribute.GetCustomAttributes(systemType, typeof(EcsUpdateBeforeAttribute));
//                 Attribute[] after = Attribute.GetCustomAttributes(systemType, typeof(EcsUpdateAfterAttribute));
//
//                 foreach (Attribute attr in before)
//                 {
//                     EcsUpdateBeforeAttribute dep = (EcsUpdateBeforeAttribute) attr;
//                     if (dep.Type == systemType)
//                         continue;
//
//                     if (!systemsDict.TryGetValue(dep.Type, out int index))
//                         continue;
//
//                     dependencies[i].before.Add(dep.Type);
//                     dependencies[index].after++;
//                 }
//
//                 foreach (Attribute attr in after)
//                 {
//                     EcsUpdateAfterAttribute dep = (EcsUpdateAfterAttribute) attr;
//                     if (dep.Type == systemType)
//                         continue;
//
//                     if (!systemsDict.TryGetValue(dep.Type, out int index))
//                         continue;
//
//                     dependencies[index].before.Add(systemType);
//                     dependencies[i].after++;
//                 }
//             }
//
//             int size = 0;
//             int[] indices = new int[systems.Count];
//
//             systems.Clear();
//
//             for (int i = 0; i < dependencies.Length; i++)
//             {
//                 if (dependencies[i].after == 0)
//                     indices[size++] = i;
//             }
//
//             while (size > 0)
//             {
//                 int index = indices[0];
//                 indices[0] = indices[--size];
//                 SystemDependencies<T> sd = dependencies[index];
//
//                 systems.Add(sd.system);
//                 foreach (Type type in sd.before)
//                 {
//                     index = systemsDict[type];
//                     dependencies[index].after--;
//                     if (dependencies[index].after == 0)
//                         indices[size++] = index;
//                 }
//             }
//
//             if (systemsDict.Count != systems.Count)
//                 throw new InvalidOperationException("unable to sort systems");
//         }
//
//         private struct SystemDependencies<T>
//         {
//             public T system;
//             public List<Type> before;
//             public int after;
//         }
//
//     }
// }