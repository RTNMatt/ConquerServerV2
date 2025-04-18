using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void PickupDroppedItem(GameClient Client, DroppedItemPacket* Packet)
        {
            if (Client.CurrentDMap != null)
            {
                DictionaryV2<uint, IDroppedItem> DroppedItems = Client.CurrentDMap.DroppedItems;
                IDroppedItem DroppedItem;
                if (DroppedItems.TryGetValue(Packet->UID, out DroppedItem))
                {
                    TIME now = TIME.Now;
                    bool remove = false;
                    if (DroppedItem.RemoveTime.Time > now.Time && 
                       (DroppedItem.ProtectionTime.Time <= now.Time || Client.Entity.UID == DroppedItem.KillerUID))
                    {
                        if (Client.Entity.X == DroppedItem.X && Client.Entity.Y == DroppedItem.Y)
                        {
                            remove = DroppedItem.IsGold();
                            if (remove)
                            {
                                Client.Money += DroppedItem.Gold;
                                UpdatePacket gold = UpdatePacket.Create();
                                gold.ID = UpdateID.Money;
                                gold.UID = Client.Entity.UID;
                                gold.Value = (uint)Client.Money;
                                Client.Send(&gold);
                                Client.Send(new MessagePacket("You have picked up " + DroppedItem.Gold.ToString() + " silvers.", 0x00FF0000, ChatID.TopLeft));
                            }
                            else
                            {
                                remove = (Client.Inventory.ItemCount < 40);
                                if (remove)
                                    Client.Inventory.Add(DroppedItem.GetItem());
                                else
                                    Client.Send(MessageConst.NOT_ENOUGH_ROOM_INVENTORY);
                            }
                        }
                    }
                    else // Item Expired
                    {
                        remove = true;
                    }

                    if (remove)
                    {
                        DataMap DMap = Client.CurrentDMap;
                        DroppedItemPacket dItem = (DroppedItemPacket)DroppedItem;
                        dItem.DropType = DropID.Remove;
                        SendRangePacket.Add(DroppedItem.MapID, DroppedItem.X, DroppedItem.Y,
                                Kernel.ViewDistance, 0, Kernel.ToBytes(&dItem), null);
                        DMap.SetItemOnTile(DroppedItem.X, DroppedItem.Y, false);
                        DroppedItems.Remove(DroppedItem.UID);
                    }
                }
                else // Item doesn't exist (already removed)
                {
                    Packet->DropType = DropID.Remove;
                    SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(Packet), null);
                }
            }
        }
    }
}