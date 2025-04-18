using System;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void BuyVendingItem(GameClient Client, ItemUsuagePacket* Packet)
        {
            if (Client.Inventory.ItemCount < 40)
            {
                GameClient vClient = ClientVendor.FindVendorClient(Packet->dwParam1);
                if (vClient != null)
                {
                    if (vClient.IsVendor)
                    {
                        VendingItem vItem = vClient.Vendor.SelectItem(Packet->UID);
                        if (vItem != null)
                        {
                            bool Purchased = false;
                            byte ItemSlot;
                            if (vClient.Inventory.Search(vItem.UID, out ItemSlot) != null)
                            {
                                if (vItem.Mode == VendMode.VendByGold)
                                {
                                    if (Purchased = (Client.Money >= vItem.Price))
                                    {
                                        Client.Money -= vItem.Price;
                                        vClient.Money += vItem.Price;
                                    }
                                }
                                else if (vItem.Mode == VendMode.VendByConquerPoints)
                                {
                                    if (Purchased = (Client.ConquerPoints >= vItem.Price))
                                    {
                                        Client.ConquerPoints -= vItem.Price;
                                        vClient.ConquerPoints += vItem.Price;
                                    }
                                }
                                if (Purchased)
                                {
                                    BigUpdatePacket big = new BigUpdatePacket(2);
                                    big.UID = Client.Entity.UID;
                                    big.Append(0, UpdateID.Money, Client.Money);
                                    big.Append(1, UpdateID.ConquerPoints, Client.ConquerPoints);
                                    Client.Send(big);
                                    big.UID = vClient.Entity.UID;
                                    big.Append(0, UpdateID.Money, Client.Money);
                                    big.Append(1, UpdateID.ConquerPoints, vClient.ConquerPoints);
                                    vClient.Send(big);

                                    Client.Send(Packet);
                                    Client.Inventory.Add(vItem.ToItem());

                                    Packet->ID = ItemUsuageID.RemoveVendingItem;
                                    vClient.Send(Packet);
                                    vClient.Vendor.RemoveItem(vItem.UID);
                                    vClient.Inventory.RemoveBySlot(ItemSlot);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}