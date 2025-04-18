using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void EquipItem(GameClient Client, ItemUsuagePacket* lpPacket)
        {
            if (!Client.Entity.Dead)
            {
                Item item = Client.Inventory.Search(lpPacket->UID);
                if (item != null)
                {
                    if (item.ID == ItemTypeConst.GMRobeID && Client.AdminFlag == 0)
                        return;

                    ItemPosition position = (ItemPosition)lpPacket->dwParam1;
                    if (position <= Item.LastSlot && position >= Item.FirstSlot || position == ItemPosition.Inventory)
                    {
                        #region Position Checks
                        switch (position)
                        {
                            case ItemPosition.Inventory:
                                {
                                    if (Client.Entity.MapID.Id == TournamentAI.MapID.Id && TournamentAI.Active)
                                        return;

                                    if (item.IsItemType(ItemTypeConst.ArrowID))
                                    {
                                        Item right = Client.Equipment[ItemPosition.Right];
                                        if (right == null)
                                            return;
                                        else if (!right.IsItemType(ItemTypeConst.BowID))
                                            return;
                                        position = ItemPosition.Left;
                                    }
                                    else
                                    {
                                        TIME Now = TIME.Now;
                                        if (Client.TimeStamps.CanUseItem.Time <= Now.Time)
                                        {
                                            Client.TimeStamps.CanUseItem = Now.AddMilliseconds(500);
                                            ExecuteScriptThread.Add(Client, item);
                                        }
                                        return;
                                    }
                                    break;
                                }
                            case ItemPosition.Right:
                                {
                                    if (item.IsTwoHander())
                                    {
                                        if (Client.Equipment[ItemPosition.Left] != null)
                                            return;
                                    }
                                    break;
                                }
                            case ItemPosition.Left:
                                {
                                    if (item.IsItemType(ItemTypeConst.BackswordID))
                                        return;
                                    else if (item.IsTwoHander())
                                        return;
                                    else
                                    {
                                        Item right = Client.Equipment[ItemPosition.Right];
                                        if (right != null)
                                        {
                                            if (right.IsTwoHander())
                                                return;
                                        }
                                    }
                                    break;
                                }
                            case ItemPosition.Head:
                                {
                                    int small = item.GetSmallItemType();
                                    if (small > 14 || small < 11)
                                        return;
                                    break;
                                }
                            case ItemPosition.Necklace:
                                {
                                    if (item.GetSmallItemType() != 12)
                                        return;
                                    break;
                                }
                            case ItemPosition.Armor:
                                {
                                    if (item.GetSmallItemType() != 13)
                                        return;
                                    break;
                                }
                            case ItemPosition.Ring:
                                {
                                    if (item.GetSmallItemType() != 15)
                                        return;
                                    break;
                                }
                            case ItemPosition.Boots:
                                {
                                    if (item.GetSmallItemType() != 16)
                                        return;
                                    break;
                                }
                        }
                        #endregion
                        #region Specific Item Checks
                        StanderdItemStats std = new StanderdItemStats(item.ID);
                        int Req;
                        if ((Req = std.ReqProfLvl) != 0)
                        {
                            ISkill prof;
                            if (Client.Proficiencies.GetSkill((ushort)item.GetItemType(), out prof))
                            {
                                if (Req > prof.ID)
                                    return;
                            }
                            else
                            {
                                return;
                            }
                        }
                        if (Client.Entity.Level < std.ReqLvl)
                            return;
                        else if (Client.Stats.Strength < std.ReqStr)
                            return;
                        else if (Client.Stats.Agility < std.ReqAgi)
                            return;
                        else
                        {
                            if ((Req = std.ReqSex) != 0)
                            {
                                if (((int)(Client.Entity.Mesh / 1000)) != Req)
                                    return;
                            }
                            if ((Req = std.ReqJob) != 0)
                            {
                                int Req2 = (int)(Req / 10);
                                if (Req2 == 1 || Req2 == 2 || Req2 == 4 || Req2 == 5)
                                {
                                    Req2 = Req % 10;
                                    Req = Client.Job % 10;
                                    if (!(Req <= 5 && Req >= Req2))
                                        return;
                                }
                            }
                        }
                        #endregion
                        if (Client.Equip(item, position, lpPacket))
                        {
                            if (item.IsItemType(ItemTypeConst.ArrowID))
                            {
                                item.Send(Client);
                                item.SendArrows(Client);
                            }
                            Client.DisplayStats();
                        }
                    }
                }
            }
        }
    }
}