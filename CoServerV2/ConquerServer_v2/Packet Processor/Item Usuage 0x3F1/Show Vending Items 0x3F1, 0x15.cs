using System;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void ShowVendingItems(GameClient Client, ItemUsuagePacket* Packet)
        {
            GameClient vClient = ClientVendor.FindVendorClient(Packet->UID);
            if (vClient != null)
            {
                foreach (VendingItem vItem in vClient.Vendor.Items)
                    vItem.Send(Client);
            }
        }
    }
}