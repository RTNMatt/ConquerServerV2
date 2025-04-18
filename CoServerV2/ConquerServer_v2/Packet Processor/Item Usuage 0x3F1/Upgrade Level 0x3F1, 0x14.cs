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
        public static void UpgradeItemLevel(GameClient Client, ItemUsuagePacket* Packet)
        {
            Item MainItem = Client.Inventory.Search(Packet->UID);
            if (MainItem != null)
            {
                byte SubItemSlot;
                Item SubItem = Client.Inventory.Search(Packet->dwParam1, out SubItemSlot);
                if (SubItem != null)
                {
                    if (SubItem.ID == 1088001)
                    {
                        int JMP = 0;
                        uint itemID = MainItem.ID;
                        int itemType = MainItem.GetSmallItemType();
                    CheckValidID:
                        switch (itemType)
                        {
                            case 11: // head
                            case 12: // neck
                            case 15: // ring
                            case 13: // armor
                            case 16: // boots
                                itemID += 10;
                                break;
                            default:
                                {
                                    if (itemType >= 40 && itemType <= 61) // weapons
                                        itemID += 10;
                                    break;
                                }
                        }
                        if (itemID != MainItem.ID)
                        {
                            if (ServerDatabase.ValidItemID(itemID))
                            {
                                StanderdItemStats std1 = new StanderdItemStats(MainItem.ID);
                                StanderdItemStats std2 = new StanderdItemStats(itemID);
                                if (std2.ReqLvl > std1.ReqLvl)
                                {
                                    Client.Inventory.RemoveBySlot(SubItemSlot);
                                    if (Kernel.Random.Next(1000) % 100 <= 40)
                                    {
                                        MainItem.ID = itemID;
                                        MainItem.SendInventoryUpdate(Client);
                                        }
                                    return;
                                }
                            }
                            else
                            {
                                if (JMP < 3)
                                {
                                    JMP++;
                                    goto CheckValidID;
                                }
                            }
                        }
                        Client.Send(MessageConst.CANNOT_UPGRADE_LEVEL);
                    }
                }
            }
        }
    }
}