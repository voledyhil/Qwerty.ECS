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
            
        private int m_threads;
        public void Execute<T>(T jobData, int arrayLength, int innerLoopBatchCount, IntPtr chunks) where T : struct, IParallelForJobChunk
        {
            m_threads = Math.Min(Math.Max(arrayLength / innerLoopBatchCount, 1), Environment.ProcessorCount);
            
            int threads = m_threads;
            int remainder = arrayLength % threads;
            int slice = arrayLength / threads + (remainder == 0 ? 0 : 1);
            for (int t = 0; t < threads; t++)
            {
                int from = t * slice;
                int to = from + slice;
                if (to > arrayLength)
                    to = arrayLength;

                if (from != to)
                {
                    ThreadPool.QueueUserWorkItem(queueOnThread,
                        new JobState<T> { jobData = jobData, chunks = chunks, from = from, to = to, }, true);
                }
                else
                {
                    Interlocked.Decrement(ref m_threads);
                }
            }
        }

        public void WaitComplete()
        {
            while (m_threads > 0) { }
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

            Interlocked.Decrement(ref m_threads);
        }
    }
}