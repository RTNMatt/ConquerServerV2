using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;

namespace ConquerServer_v2
{
    public unsafe class CreateUIDCallback
    {
        private class SendUIDData
        {
            public IBaseEntity Sender;
            public uint[] UIDs;
            public ConquerCallback Callback;

            public SendUIDData(IBaseEntity sender, uint[] uids, ConquerCallback callback)
            {
                Sender = sender;
                UIDs = uids;
                Callback = callback;
            }
        }
        private static int pendingThreads;
        public static int PendingThreads
        {
            get { return pendingThreads; }
        }

        private static void _Process(object obj)
        {
            pendingThreads++;
            SendUIDData Data = obj as SendUIDData;
            foreach (uint UID in Data.UIDs)
            {
                foreach (GameClient Client in Kernel.Clients)
                {
                    if (Client != null)
                    {
                        if (Client.Entity.UID == UID)
                        {
                            Data.Callback(Data.Sender, Client.Entity);
                        }
                    }
                }
            }
            pendingThreads--;
        }
        private static ParameterizedThreadStart Process = new ParameterizedThreadStart(_Process);

        private static void Enqueue(SendUIDData Data)
        {
            new Thread(Process).Start(Data);
        }
        public static void Add(IBaseEntity Entity, uint[] UIDs, ConquerCallback Callback)
        {
            Enqueue(new SendUIDData(Entity, UIDs, Callback));
        }
        public static void Add(IBaseEntity Entity, List<uint> UIDs, ConquerCallback Callback)
        {
            Enqueue(new SendUIDData(Entity, UIDs.ToArray(), Callback));
        }
    }
}