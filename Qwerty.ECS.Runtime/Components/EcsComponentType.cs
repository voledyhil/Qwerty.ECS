using System;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Components
{
    public static class EcsComponentType<T> where T : struct, IEcsComponent
    {
        public static short index
        {
            get
            {
                if (!m_isRegister)
                {
                    throw new InvalidOperationException($"'{typeof(T)}' is not register");
                }
                return m_index;
            }
        }

        private static short m_index;
        private static bool m_isRegister;

        public static void Register()
        {
            if (m_isRegister)
            {
                return;
            }
            m_index = EcsTypeManager.Register<T>();
            m_isRegister = true;
        }
    }
}