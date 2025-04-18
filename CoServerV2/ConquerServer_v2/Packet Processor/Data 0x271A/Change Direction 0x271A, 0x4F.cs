using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void ChangeDirection(GameClient Client, DataPacket* lpPacket)
        {
            Client.Entity.Facing = (ConquerAngle)(lpPacket->dwParam1 % 8);
            SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(lpPacket), null);
        }
    }
}