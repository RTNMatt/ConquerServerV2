using System;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void AddVendingItemGold(GameClient Client, ItemUsuagePacket* Packet)
        {
            if (Client.IsVendor)
            {
                Item item = Client.Inventory.Search(Packet->UID);
                if (item != null)
                {
                    Client.Vendor.AddItem(item, (int)Packet->dwParam1, true);
                    Client.Send(Packet);
                }
            }
        }
    }
}