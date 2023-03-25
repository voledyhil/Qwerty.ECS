// ReSharper disable RedundantUsingDirective
using System;
using System.Threading;
using Qwerty.ECS.Runtime.Chunks;

// ReSharper disable once CheckNamespace
namespace Qwerty.ECS.Runtime.Jobs
{
    internal class ThreadPoolWorker
    {
        private struct JobState<T> where T : struct
        {
            public T jobData;
            public IntPtr chunks;
            public int from;
            public int to;
        }
            
        private readonly int m_tasks;
        private readonly int m_length;
        private int m_taskCompleted;
        
        public ThreadPoolWorker(int length)
        {
            m_length = length;
            m_tasks = Environment.ProcessorCount;
        }

        public void Execute<T>(T jobData, IntPtr chunks) where T : struct, IParallelForJobChunk
        {
            int remainder = m_length % m_tasks;
            int slice = m_length / m_tasks + (remainder == 0 ? 0 : 1);
            for (int t = 0; t < m_tasks; t++)
            {
                int from = t * slice;
                int to = from + slice;
                if (to > m_length)
                    to = m_length;

                if (from != to)
                {
                    JobState<T> state = new JobState<T> { jobData = jobData, chunks = chunks, from = from, to = to, };
                    ThreadPool.QueueUserWorkItem(queueOnThread, state, true);
                }
                else
                {
                    Interlocked.Increment(ref m_taskCompleted);
                }
            }
            while (m_taskCompleted < m_tasks) { }
        }

        private unsafe void queueOnThread<T>(JobState<T> state) where T : struct, IParallelForJobChunk
        {
            IntPtr chunks = state.chunks;
            T jobData = state.jobData;

            int from = state.from;
            int to = state.to;
            
            for (int index = from; index < to; index++)
            {
                EcsChunk* chunk = (EcsChunk*)MemoryUtil.Read<IntPtr>(chunks, index * sizeof(IntPtr));
                jobData.Execute(new EcsChunkAccessor(chunk));
            }

            Interlocked.Increment(ref m_taskCompleted);
        }
    }
}