using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;

namespace ConquerServer_v2
{
    public unsafe class SendGlobalPacket
    {
        class GlobalPacketData
        {
            public MapID MapID;
            public byte[] Packet;
            public ConquerCallback Callback;

            public GlobalPacketData(MapID _MapID, byte[] _Packet, ConquerCallback _Callback)
            {
                MapID = _MapID;
                Packet = _Packet;
                Callback = _Callback;
            }
        }

        private static int pendingThreads = 0;
        public static int PendingThreads
        {
            get { return pendingThreads; }
        }
        private static void _internalProcess(object obj)
        {
            pendingThreads++;
            GlobalPacketData Global = obj as GlobalPacketData;
            if (Global != null)
            {
                if (Global.Packet != null)
                {
                    foreach (GameClient Client in Kernel.Clients)
                    {
                        if (Client != null)
                        {
                            if ((Client.ServerFlags & ServerFlags.LoggedIn) == ServerFlags.LoggedIn)
                            {
                                if (Client.Entity.MapID.Id == Global.MapID.Id || Global.MapID.Id == 0)
                                {
                                    Client.Send(Global.Packet);
                                    if (Global.Callback != null)
                                        Global.Callback(null, Client.Entity);
                                }
                            }
                        }
                    }
                }
            }
            pendingThreads--;
        }
        private static ParameterizedThreadStart internalProcess = new ParameterizedThreadStart(_internalProcess);

        public static void Add(byte[] Data)
        {
            new Thread(internalProcess).Start(new GlobalPacketData(0, Data, null));
        }
        public static void Add(byte[] Data, MapID MapID)
        {
            new Thread(internalProcess).Start(new GlobalPacketData(MapID, Data, null));
        }
        public static void Add(byte[] Data, ConquerCallback Callback)
        {
            new Thread(internalProcess).Start(new GlobalPacketData(0, Data, Callback));
        }
        public static void Add(byte[] Data, MapID MapID, ConquerCallback Callback)
        {
            new Thread(internalProcess).Start(new GlobalPacketData(MapID, Data, Callback));
        }
    }
}
