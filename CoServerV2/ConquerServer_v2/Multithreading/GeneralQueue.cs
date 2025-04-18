using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConquerServer_v2
{
    public class GeneralQueue
    {
        class GeneralQueueData
        {
            public object Parameter;
            public Func<object, int> Callback;
        }
        class InternalQueue : SmartQueue<GeneralQueueData>
        {
            public InternalQueue() :
                base(1)
            {
            }
            protected override void OnDequeue(GeneralQueueData Data)
            {
                if (Data != null)
                {
                    Data.Callback(Data.Parameter);
                }
            }
        }
        private InternalQueue Queue;
        public int Count { get { return Queue.Count; } }
        public GeneralQueue()
        {
            Queue = new InternalQueue();
        }
        public void Start(ThreadPriority Priority)
        {
            Queue.Start(Priority);
        }
        public void Stop()
        {
            Queue.Stop();
        }
        public void Add(Func<object, int> Callback, object Argument)
        {
            GeneralQueueData data = new GeneralQueueData();
            data.Callback = Callback;
            data.Parameter = Argument;
            Queue.Enqueue(data);
        }
    }
}
