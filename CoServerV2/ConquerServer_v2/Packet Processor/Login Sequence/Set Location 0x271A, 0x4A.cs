using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void SetLocation(GameClient Client, DataPacket* Packet)
        {
            Packet->UID = Client.Entity.UID;
            Packet->dwParam1 = Client.Entity.MapID;
            Packet->wParam1 = Client.Entity.X;
            Packet->wParam2 = Client.Entity.Y;
            Client.Send(Packet);
        }
    }
}
