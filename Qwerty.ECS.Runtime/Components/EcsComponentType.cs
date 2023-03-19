namespace Qwerty.ECS.Runtime.Components
{
    public static class EcsComponentType<T> where T : struct, IEcsComponent
    {
        public static byte index
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

        private static byte m_index;
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