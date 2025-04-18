using System;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void SellItemToNpc(GameClient Client, ItemUsuagePacket* Packet)
        {
            byte Slot;
            Item Item = Client.Inventory.Search(Packet->dwParam1, out Slot);
            if (Item != null)
            {
                Client.Inventory.RemoveBySlot(Slot);
                
                StanderdItemStats stats = new StanderdItemStats(Item.ID);
                int Price = (int)(stats.MoneyPrice / 3);
                Client.Money += Price;
                UpdatePacket Update = UpdatePacket.Create();
                Update.UID = Client.Entity.UID;
                Update.ID = UpdateID.Money;
                Update.Value = (uint)Client.Money;
                Client.Send(&Update);
            }
        }
    }
}