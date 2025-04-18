using System;
using System.Collections.Generic;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void DropGold(GameClient Client, ItemUsuagePacket* Packet)
        {
            int num = (int)Packet->UID;
            if (num > 0)
                Client.DropGold(num);
        }
    }
}