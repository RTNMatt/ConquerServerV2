using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void RepairItem(GameClient Client, ItemUsuagePacket* lpPacket)
        {
            foreach (IMapObject obj in Client.Screen.Objects)
            {
                if (obj.MapObjType == MapObjectType.Npc)
                {
                    NpcEntity npc = obj as NpcEntity;
                    if (npc.Interaction == 1)
                    {
                        Item item = Client.Inventory.Search(lpPacket->UID);
                        if (item != null)
                        {
                            if (item.MaxDurability > 0)
                            {
                                StanderdItemStats std = new StanderdItemStats(item.ID);
                                double Repair = item.MaxDurability - item.Durability;
                                if (Repair > 0)
                                {
                                    Repair = (Repair / item.MaxDurability) * 0.75 * std.MoneyPrice;
                                    int nRepairCost = (int)Math.Round(Repair);
                                    if (Client.Money >= nRepairCost)
                                    {
                                        Client.Money -= nRepairCost;
                                        UpdatePacket update = UpdatePacket.Create();
                                        update.ID = UpdateID.Money;
                                        update.UID = Client.Entity.UID;
                                        update.Value = (uint)Client.Money;
                                        Client.Send(&update);

                                        item.Durability = item.MaxDurability;
                                        item.SendInventoryUpdate(Client);
                                    }
                                    else
                                    {
                                        Client.Send(MessageConst.CANNOT_AFFORD);
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }
    }
}