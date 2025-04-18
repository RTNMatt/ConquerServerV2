using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;

namespace ConquerServer_v2
{
    public unsafe class SendRangePacket
    {
        public class SendRangePacketData
        {
            public IBaseEntity SenderEntity;
            public ushort X, Y;
            public MapID MapID;
            public byte Distance;
            public uint Filter;
            public byte[] Packet;
            public ConquerCallback Callback;
            public TIME Delay;
            public SendRangePacketData(IBaseEntity Sender,
                                       MapID _MapID, ushort _X, ushort _Y,
                                       byte _Distance, uint _Filter,
                                       byte[] _Packet,
                                       ConquerCallback _Callback,
                                       TIME _Delay)
            {
                SenderEntity = Sender;
                MapID = _MapID;
                X = _X;
                Y = _Y;
                Distance = _Distance;
                Filter = _Filter;
                Packet = _Packet;
                Callback = _Callback;
                Delay = _Delay;
            }
        }
        public class RangedPacketQueue : SmartQueue<SendRangePacketData>
        {
            public RangedPacketQueue() :
                base(1)
            {
            }
            protected override void OnDequeue(SendRangePacketData Value)
            {
                SendRangePacket._ProcessMain(Value);
            }
        }
        public static RangedPacketQueue Queue;
        private static int pendingThreads;
        public static int PendingThreads
        {
            get 
            {
                int num = pendingThreads;
                if (Queue != null)
                    num += Queue.Count;
                return num;
            }
        }

        private static void _ProcessSleeper(object obj)
        {
            pendingThreads++;
            SendRangePacketData Data = obj as SendRangePacketData;

            int sleep = (int)(Data.Delay.Time - TIME.Now.Time);
            if (sleep > 0)
                Thread.Sleep(sleep);

            _ProcessMain(Data);
            pendingThreads--;
        }
        private static void _ProcessMain(SendRangePacketData Data)
        {
            bool check;
            foreach (GameClient Client in Kernel.Clients)
            {
                if (Client != null)
                {
                    if ((Client.ServerFlags & ServerFlags.LoggedIn) == ServerFlags.LoggedIn)
                    {
                        if (Client.Entity.MapID.Id == Data.MapID.Id &&
                            Client.Entity.UID != Data.Filter)
                        {
                            if (Kernel.GetDistance(Client.Entity.X, Client.Entity.Y, Data.X, Data.Y) <= Data.Distance)
                            {
                                check = true;
                                if (Data.Callback != null)
                                    check = (Data.Callback(Data.SenderEntity, Client.Entity) == 0);

                                if (check)
                                    Client.Send(Data.Packet);
                            }
                        }
                    }
                }
            }
        }
        private static void _ProcessNewThread(object obj)
        {
            pendingThreads++;
            _ProcessMain(obj as SendRangePacketData);
            pendingThreads--;
        }

        static SendRangePacket()
        {
            pendingThreads = 0;
            //Queue = new RangedPacketQueue();
            //Queue.Start(ThreadPriority.AboveNormal);
        }
        private static void Enqueue(SendRangePacketData Data, int Index)
        {
            if (Data.Delay.Time > TIME.Now.Time)
            {
                Thread sleep = new Thread(_ProcessSleeper);
                sleep.Priority = ThreadPriority.BelowNormal;
                sleep.Start(Data);
            }
            else
            {
                Thread run = new Thread(_ProcessNewThread);
                run.Priority = ThreadPriority.AboveNormal;
                run.Start(Data);
                /*lock (Queue)
                {
                    if (Index > -1)
                        Queue.Enqueue(Data, Index);
                    else
                        Queue.Enqueue(Data);
                }*/
            }
        }
        public static void Add(MapID MapID, ushort X, ushort Y, byte Distance, uint Filter, byte[] Packet, ConquerCallback Callback)
        {
            Enqueue(new SendRangePacketData(null, MapID, X, Y, Distance, Filter, Packet, Callback, new TIME(0)), -1);
        }
        public static void Add(IBaseEntity Entity, byte Distance, uint Filter, byte[] Packet, ConquerCallback Callback)
        {
            Enqueue(new SendRangePacketData(Entity, Entity.MapID, Entity.X, Entity.Y, Distance, Filter, Packet, Callback, new TIME(0)), -1);
        }
        public static void Add(IBaseEntity Entity, byte Distance, uint Filter, byte[] Packet, ConquerCallback Callback, TIME Time)
        {
            Enqueue(new SendRangePacketData(Entity, Entity.MapID, Entity.X, Entity.Y, Distance, Filter, Packet, Callback, Time), -1);
        }
    }
}