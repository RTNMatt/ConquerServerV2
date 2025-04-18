using System;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void BuyItemFromNpc(GameClient Client, ItemUsuagePacket* Packet)
        {
            if (ServerDatabase.NpcDistanceCheck(Packet->UID, Client.CurrentDMap, Client.Entity.X, Client.Entity.Y))
            {
                string ShopHeader = Packet->UID.ToString();
                uint ItemID;
                bool Valid = false;
                byte Amount = ServerDatabase.Shop.ReadByte(ShopHeader, "ItemAmount", 0);

                for (byte i = 0; i < Amount; i++)
                {
                    ItemID = ServerDatabase.Shop.ReadUInt32(ShopHeader, "Item" + i.ToString(), 0);
                    if ((Valid = (ItemID == Packet->dwParam1)))
                        break;
                }
                if (Valid)
                {
                    Packet->dwParam2 = Math.Max(Packet->dwParam2, 1);
                    byte MoneyType = ServerDatabase.Shop.ReadByte(ShopHeader, "MoneyType", 0);
                    if (Client.Inventory.ItemCount <= 40 - Packet->dwParam2)
                    {
                        int UserAmount = (MoneyType == 0) ? Client.Money : Client.ConquerPoints;
                        int AskingAmount;
                        StanderdItemStats stats = new StanderdItemStats(Packet->dwParam1);
                        if (MoneyType == 0)
                            AskingAmount = stats.MoneyPrice;
                        else
                        {
                            AskingAmount = stats.ConquerPointsPrice;
                            AskingAmount *= (int)Packet->dwParam2;
                        }

                        if (UserAmount >= AskingAmount)
                        {
                            UserAmount -= AskingAmount;
                            UpdatePacket Update = UpdatePacket.Create();
                            Update.UID = Client.Entity.UID;
                            Update.Value = (uint)UserAmount;
                            if (MoneyType == 0)
                            {
                                Client.Money = UserAmount;
                                Update.ID = UpdateID.Money;
                            }
                            else
                            {
                                Client.ConquerPoints = UserAmount;
                                Update.ID = UpdateID.ConquerPoints;
                            }
                            Client.Send(&Update);

                            Item Item = new Item();
                            Item.ID = Packet->dwParam1;
                            Item.Durability = stats.Durability;
                            Item.MaxDurability = stats.Durability;
                            Item.Color = 3;
                            Client.Inventory.Add(Item);
                                for (uint i = 1; i < Packet->dwParam2; i++)
                                {
                                    Item = new Item();
                                    Item.ID = Packet->dwParam1;
                                    Item.Durability = stats.Durability;
                                    Item.Durability = stats.Durability;
                                    Client.Inventory.Add(Item);
                                }
                        }
                        else
                        {
                            Client.Send(MessageConst.CANNOT_AFFORD);
                        }
                    }
                    else
                    {
                        Client.Send(MessageConst.NOT_ENOUGH_ROOM_INVENTORY);
                    }
                }
            }
        }
    }
}