using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;

namespace ConquerServer_v2
{
    public unsafe class ExecuteScriptThread
    {
        class ExecuteScriptData
        {
            public Item Item;
            public byte OptionID;
            public string Input;
            public GameClient Client;

            public ExecuteScriptData(GameClient client, byte optionid, string input, Item item)
            {
                Client = client;
                OptionID = optionid;
                Input = input;
                Item = item;
            }
        }
        class ExecuteScriptQueue : SmartQueue<ExecuteScriptData>
        {
            public ExecuteScriptQueue()
                : base(2)
            {
            }
            protected override void OnDequeue(ExecuteScriptData data)
            {
                if (data != null)
                {
                    if (data.Client != null)
                    {
                        if (data.Item != null)
                        {
                            if (ConquerScriptEngine.ProcessItem(data.Client, data.Item))
                                data.Client.Inventory.Remove(data.Item.UID);
                        }
                        else
                            ConquerScriptEngine.ProcessNpc(data.Client, data.OptionID, data.Input);
                    }
                }
            }
        }
        private static ExecuteScriptQueue Queue;
        public static int PendingThreads
        {
            get { return Queue.Count; }
        }

        static ExecuteScriptThread()
        {
            Queue = new ExecuteScriptQueue();
            Queue.Start(ThreadPriority.Lowest);
        }
        public static void Add(GameClient Client, Item Item)
        {
            Queue.Enqueue(new ExecuteScriptData(Client, 0, null, Item), 0);
        }
        public static void Add(GameClient Client, byte OptionID, string Input)
        {
            Queue.Enqueue(new ExecuteScriptData(Client, OptionID, Input, null), 1);
        }
    }
}   
