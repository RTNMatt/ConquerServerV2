using System;
using System.IO;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        private static int GetGemBlessWorth(uint ItemID)
        {
            int high = 1;
            int low = 0;
            if (ItemID % 10 == 1) // refined
            {
                high = 59;
                low = 1;
            }
            else
            {
                switch (ItemID)
                {
                    /* unique */
                        // dragon
                    case 700012: high = 159; low = 100; break;
                        // phoenix, moon, violet
                    case 700002: 
                    case 700062:
                    case 700052: high = 109; low = 60; break;
                        // rainbow
                    case 700032: high = 129; low = 80; break;
                        // tortoise, kylin, fury
                    case 700072:
                    case 700042:
                    case 700022: high = 89; low = 40; break;
                    /* super */
                        // dragon
                    case 700013: high = 255; low = 200; break;
                        // phoenix, tortoise, rainbow
                    case 700003:
                    case 700073:
                    case 700033: high = 229; low = 170; break;
                        // moon, violet
                    case 700063:
                    case 700053: high = 199; low = 140; break;
                        // fury
                    case 700023: high = 149; low = 90; break;
                        // kylin
                    case 700043: high = 119; low = 70; break;
                }
            }
            return Kernel.Random.Next(low, high);
        }

        public static void UpgradeItemEnchant(GameClient Client, ItemUsuagePacket* Packet)
        {
            Item MainItem = Client.Inventory.Search(Packet->UID);
            if (MainItem != null)
            {
                byte SubItemSlot;
                Item SubItem = Client.Inventory.Search(Packet->dwParam1, out SubItemSlot);
                if (SubItem != null)
                {
                    if (SubItem.IsItemType(ItemTypeConst.GemID))
                    {
                        int num = GetGemBlessWorth(SubItem.ID);
                        if (num > MainItem.Enchant)
                            MainItem.Enchant = (byte)num;

                        Client.Inventory.RemoveBySlot(SubItemSlot);
                        MainItem.SendInventoryUpdate(Client);
                    }
                }
            }
        }
    }
}