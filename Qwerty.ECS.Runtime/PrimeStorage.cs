namespace Qwerty.ECS.Runtime
{
    public unsafe struct PrimeStorage
    {
        private const int Capacity = 24;
        private UnsafeArray* m_primes;

        public void Alloc()
        {
            m_primes = (UnsafeArray*)MemoryUtil.Alloc<UnsafeArray>();
            m_primes->Alloc<int>(Capacity);
            m_primes->Write(0, 3);
            m_primes->Write(1, 7);
            m_primes->Write(2, 11);
            m_primes->Write(3, 17);
            m_primes->Write(4, 23);
            m_primes->Write(5, 29);
            m_primes->Write(6, 37);
            m_primes->Write(7, 47);
            m_primes->Write(8, 59);
            m_primes->Write(9, 71);
            m_primes->Write(10, 89);
            m_primes->Write(11, 107);
            m_primes->Write(12, 131);
            m_primes->Write(13, 163);
            m_primes->Write(14, 197);
            m_primes->Write(15, 239);
            m_primes->Write(16, 293);
            m_primes->Write(17, 353);
            m_primes->Write(18, 431);
            m_primes->Write(19, 521);
            m_primes->Write(20, 631);
            m_primes->Write(21, 761);
            m_primes->Write(22, 919);
            m_primes->Write(23, 1103);
        }

        public int GetPrime(int min)
        {
            for (int i = 0; i < Capacity; i++)
            {
                int prime = m_primes->Read<int>(i);
                if (prime >= min)
                {
                    return prime;
                }
            }
            throw new ArgumentException(nameof(GetPrime));
        }

        public void Dispose()
        {
            m_primes->Dispose();
            MemoryUtil.Free((IntPtr)m_primes);
        }
    }
}