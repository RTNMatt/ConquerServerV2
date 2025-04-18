using System;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void RemoveVendingItem(GameClient Client, ItemUsuagePacket* Packet)
        {
            if (Client.IsVendor)
            {
                Client.Vendor.RemoveItem(Packet->UID);
                Client.Send(Packet);
            }
        }
    }
}