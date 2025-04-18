using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConquerServer_v2
{
    public enum SmartQueueStatus
    {
        Started,
        Idle,
        Dequeueing,
        Processing,
        Sleeping
    }

    public abstract class SmartQueue<T> where T : class
    {
        protected Queue<T>[] Queues;
        private Thread[] Threads;
        private SmartQueueStatus[] m_Status;
        protected int m_SmartQueue;

        public int Count
        {
            get
            {
                int num = 0;
                for (int i = 0; i < Queues.Length; i++)
                {
                    num += Queues[i].Count;
                }
                return num;
            }
        }
        public int ProcessorCount { get { return Queues.Length; } }

        protected virtual Queue<T> GetSmartQueue()
        {
            Queue<T> result = Queues[m_SmartQueue];
            m_SmartQueue = (m_SmartQueue + 1) % Queues.Length;
            return result;
        }     
        private void MainThread(object arg)
        {
            try
            {
                int i = (int)arg;
                Queue<T> queue = Queues[i];
                m_Status[i] = SmartQueueStatus.Started;
                while (true)
                {
                    T Data = null;
                    lock (queue)
                    {
                        m_Status[i] = SmartQueueStatus.Dequeueing;
                        if (queue.Count > 0)
                            Data = queue.Dequeue();
                    }
                    if (Data != null)
                    {
                        m_Status[i] = SmartQueueStatus.Processing;
                        OnDequeue(Data);
                    }
                    m_Status[i] = SmartQueueStatus.Sleeping;
                    Thread.Sleep(1);
                }
            }
            catch (ThreadAbortException)
            {
            }
        }
        protected abstract void OnDequeue(T Value);

        public SmartQueue(int Processors)
        {
            Queues = new Queue<T>[Processors];
            m_Status = new SmartQueueStatus[Processors];
            for (int i = 0; i < Queues.Length; i++)
            {
                Queues[i] = new Queue<T>();
                m_Status[i] = SmartQueueStatus.Idle;
            }
        }
        public void Start(ThreadPriority Priority)
        {
            if (Threads == null)
            {
                Threads = new Thread[Queues.Length];
                for (int i = 0; i < Threads.Length; i++)
                {
                    Threads[i] = new Thread(MainThread);
                    Threads[i].Priority = Priority;
                    Threads[i].Start(i);
                }
            }
        }
        public void Stop()
        {
            if (Threads != null)
            {
                for (int i = 0; i < Threads.Length; i++)
                {
                    Threads[i].Abort();
                    m_Status[i] = SmartQueueStatus.Idle;
                }
                Threads = null;
            }
        }
        public SmartQueueStatus Status(int Index)
        {
            return m_Status[Index];
        }
        public virtual void Enqueue(T Value)
        {
            Queue<T> queue = GetSmartQueue();
            lock (queue)
            {
                queue.Enqueue(Value);
            }
        }
        public virtual void Enqueue(T Value, int QueueIndex)
        {
            Queue<T> queue = Queues[QueueIndex];
            lock (queue)
            {
                queue.Enqueue(Value);
            }
        }
        public virtual void Clear()
        {
            for (int i = 0; i < Queues.Length; i++)
            {
                lock (Queues[i])
                {
                    Queues[i].Clear();
                }
            }
        }
    }
}
