using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void ChangePKMode(GameClient Client, DataPacket* lpPacket)
        {
            Client.PKMode = (PKMode)Math.Min(lpPacket->dwParam1, 3);
            Client.Send(lpPacket);
        }
    }
}