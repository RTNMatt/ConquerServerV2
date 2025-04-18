using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void UpgradeItemQuality(GameClient Client, ItemUsuagePacket* Packet)
        {
            Item MainItem = Client.Inventory.Search(Packet->UID);
            if (MainItem != null)
            {
                byte SubItemSlot;
                Item SubItem = Client.Inventory.Search(Packet->dwParam1, out SubItemSlot);
                if (SubItem != null)
                {
                    if (SubItem.ID == 1088000)
                    {
                        byte itemQuality = MainItem.GetQuality();
                        if (itemQuality >= 3 && itemQuality < 9)
                        {   
                            Client.Inventory.RemoveBySlot(SubItemSlot);
                            bool Lucky = false;
                            if (itemQuality < 6)
                                Lucky = Kernel.Random.Next(1000) % 100 <= 80;
                            else if (itemQuality == 6)
                                Lucky = Kernel.Random.Next(1000) % 100 <= 50;
                            else if (itemQuality == 7)
                                Lucky = Kernel.Random.Next(1000) % 100 <= 20;
                            else if (itemQuality == 8)
                                Lucky = Kernel.Random.Next(1000) % 100 <= 8;
                            if (Lucky)
                            {
                                MainItem.ID++;
                                MainItem.SendInventoryUpdate(Client);
                            }
                        }
                        else
                        {
                            Client.Send(MessageConst.CANNOT_UPGRADE_QUALITY);
                        }
                    }
                }
            }
        }
    }
}