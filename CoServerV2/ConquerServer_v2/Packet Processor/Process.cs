using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void Process(GameClient Client, byte* Packet, byte[] SafePacket, ushort Type)
        {
            Client.PacketCount++;
            // Used to assist debugging, please assign this field with the sub-id of any packets
            // that have sub-ids.
            int SubID = 0;

            try
            {
                switch (Type)
                {
                    case 0x3EC: ProcessMessage(Client, Packet, SafePacket); break;
                    case 0x3E9: CreationThread.CharacterCreate(Client, (CreateCharacterPacket*)Packet); break;
                    case 0x3F1:
                        {
                            ItemUsuagePacket* iuPtr = (ItemUsuagePacket*)Packet;
                            SubID = (int)iuPtr->ID;
                            switch (iuPtr->ID)
                            {
                                case ItemUsuageID.Ping: ReplyPing(Client, iuPtr); break;
                                
                                case ItemUsuageID.Unequip: UnequipItem(Client, iuPtr); break;
                                case ItemUsuageID.Equip: EquipItem(Client, iuPtr); break;
                                case ItemUsuageID.BuyItem: BuyItemFromNpc(Client, iuPtr); break;
                                case ItemUsuageID.SellItem: SellItemToNpc(Client, iuPtr); break;
                                case ItemUsuageID.DropItem: DropItem(Client, iuPtr); break;
                                case ItemUsuageID.RepairItem: RepairItem(Client, iuPtr); break;
                                case ItemUsuageID.DropGold: DropGold(Client, iuPtr); break;

                                case ItemUsuageID.UpgradeDragonball: UpgradeItemQuality(Client, iuPtr); break;
                                case ItemUsuageID.UpgradeMeteor: UpgradeItemLevel(Client, iuPtr); break;
                                case ItemUsuageID.UpdateEnchant: UpgradeItemEnchant(Client, iuPtr); break;

                                case ItemUsuageID.ShowWarehouseMoney: OpenWarehouse(Client, iuPtr); break;
                                case ItemUsuageID.DepositWarehouse: DepositWarehouseMoney(Client, iuPtr); break;
                                case ItemUsuageID.WithdrawWarehouse: WithdrawWarehouseMoney(Client, iuPtr); break;

                                case ItemUsuageID.AddVendingItemConquerPts: AddVendingItemCPs(Client, iuPtr); break;
                                case ItemUsuageID.AddVendingItemGold: AddVendingItemGold(Client, iuPtr); break;
                                case ItemUsuageID.ShowVendingList: ShowVendingItems(Client, iuPtr); break;
                                case ItemUsuageID.BuyVendingItem: BuyVendingItem(Client, iuPtr); break;
                                case ItemUsuageID.RemoveVendingItem: RemoveVendingItem(Client, iuPtr); break;
                            }
                            break;
                        }
                    case 0x3F7:
                        {
                            StringPacket strPacket = Packet;
                            SubID = (int)strPacket.ID;
                            switch (strPacket.ID)
                            {
                                case StringID.ViewEquipment: ViewEquipment(Client, strPacket); break;
                                case StringID.GuildMemberList: GuildMemberList(Client, strPacket); break;
                            }
                            break;
                        }
                    case 0x3FB:
                        {
                            AssociatePacket* aPtr = (AssociatePacket*)Packet;
                            switch (aPtr->ID)
                            {
                                case AssociationID.RequestFriend: RequestFriend(Client, aPtr); break;
                                case AssociationID.RemoveFriend: RemoveFriend(Client, aPtr); break;
                                // TO-DO:
                                // enemies
                            }
                            break;
                        }
                    case 0x3FE:
                        {
                            RequestAttackPacket* atkPtr = (RequestAttackPacket*)Packet;
                            SubID = (int)atkPtr->AtkType;
                            if (atkPtr->AtkType == AttackID.Magic)
                            {
                                atkPtr->Decrypt(Client.Entity.UID, Client.SpellCrypt);
                            }

                            if (!Client.Entity.Dead)
                            {
                                switch (atkPtr->AtkType)
                                {
                                    case AttackID.Archer:
                                    case AttackID.Physical:
                                        {
                                            if (atkPtr->UID == Client.Entity.UID)
                                            {
                                                RequestAttackPacket.MoveData(Client.AutoAttackPtr.Addr, atkPtr);
                                                RequestAttackPacket* AtkPtr = (RequestAttackPacket*)Client.AutoAttackPtr.Addr;
                                                AtkPtr->AttackerX = Client.Entity.X;
                                                AtkPtr->AttackerY = Client.Entity.Y;
                                                AtkPtr->Aggressive = true;

                                                PhysialAttack(Client);
                                            }
                                            else
                                            {
                                                if (Client.Pet != null)
                                                {
                                                    Client.Pet.Target = Client.Screen.FindObject(atkPtr->OpponentUID) as IBaseEntity;
                                                    Client.Pet.Attack();
                                                }
                                            }
                                            break;
                                        }
                                    case AttackID.Magic:
                                        {
                                            RequestAttackPacket.MoveData(Client.AutoAttackPtr.Addr, atkPtr);
                                            RequestAttackPacket* AtkPtr = (RequestAttackPacket*)Client.AutoAttackPtr.Addr;
                                            AtkPtr->AttackerX = Client.Entity.X;
                                            AtkPtr->AttackerY = Client.Entity.Y;
                                            AtkPtr->Aggressive = true;

                                            MagicAttack(Client, AtkPtr->SpellID);
                                            break;
                                        }
                                    case AttackID.AcceptMarriage: AcceptMarriage(Client, atkPtr); break;
                                    case AttackID.RequestMarriage: ProposeMarriage(Client, atkPtr); break;
                                }
                            }
                            break;
                        }
                    case 0x3FF:
                        {
                            TeamActionPacket* taPtr = (TeamActionPacket*)Packet;
                            SubID = (int)taPtr->ID;
                            switch (taPtr->ID)
                            {
                                case TeamActionID.Create: CreateTeam(Client, taPtr); break;
                                case TeamActionID.Dismiss: DismissTeam(Client, taPtr); break;
                                case TeamActionID.Kick: KickFromTeam(Client, taPtr); break;
                                case TeamActionID.LeaveTeam: LeaveTeam(Client, taPtr); break;
                                case TeamActionID.RequestInvite: InviteJoinTeam(Client, taPtr); break;
                                case TeamActionID.RequestJoin: RequestJoinTeam(Client, taPtr); break;
                                case TeamActionID.AcceptInvite: AcceptInviteTeam(Client, taPtr); break;
                                case TeamActionID.AcceptJoin: AcceptJoinTeam(Client, taPtr); break;
                            }
                            break;
                        }
                    case 0x400: DistributeStatPoints(Client, (DistributeStatPacket*)Packet); break;
                    case 1052: LoginStart(Client, Packet); break;
                    case 0x403: SocketGem(Client, (GemSocketPacket*)Packet); break;
                    case 0x420:
                        {
                            TradePacket* tPtr = (TradePacket*)Packet;
                            SubID = (int)tPtr->ID;
                            switch (tPtr->ID)
                            {
                                case TradeID.RequestNewTrade: NewTrade(Client, tPtr); break;
                                case TradeID.RequestCloseTrade: CloseTrade(Client, tPtr); break;
                                case TradeID.RequestCompleteTrade: CompleteTrade(Client, tPtr); break;
                                case TradeID.RequestAddMoneyToTrade: AddMoneyToTrade(Client, tPtr); break;
                                case TradeID.RequestAddConquerPointsToTrade: AddCPsToTrade(Client, tPtr); break;
                                case TradeID.RequestAddItemToTrade: AddItemToTrade(Client, tPtr); break;
                            }
                            break;
                        }
                    case 0x44E:
                        {
                            WarehousePacket* whPtr = (WarehousePacket*)Packet;
                            switch (whPtr->Action)
                            {
                                case WarehouseActionID.Show: ShowWarehouseItems(Client); break;
                                case WarehouseActionID.DepositItem: DepositWarehouseItem(Client, whPtr); break;
                                case WarehouseActionID.WithdrawItem: WithdrawWarehouseItem(Client, whPtr); break;
                            }
                            break;
                        }
                    case 0x44D: PickupDroppedItem(Client, (DroppedItemPacket*)Packet); break;
                    case 0x453:
                        {
                            GuildRequestPacket* grPtr = (GuildRequestPacket*)Packet;
                            SubID = (int)grPtr->ID;
                            switch (grPtr->ID)
                            {
                                case GuildRequestID.RequestJoin: RequestJoinGuild(Client, grPtr); break;
                                case GuildRequestID.AcceptJoin: AcceptJoinGuild(Client, grPtr); break;
                                case GuildRequestID.Donate: DonateToGuild(Client, grPtr); break;
                                case GuildRequestID.Quit: QuitGuild(Client); break;
                                case GuildRequestID.RequestInfo: GetGuildInfo(Client); break;
                            }
                            break;
                        }
                    case 0x458: AppendGuildMemberInfo(Client, (GuildMemberInfoPacket*)Packet); break; 
                    case 0x7EF: NpcStartup(Client, (NpcClickPacket*)Packet); break;
                    case 0x7F0: NpcContinue(Client, (NpcClickPacket*)Packet); break;
                    case 0x7F4: ComposeItems(Client, (ComposeItemPacket*)Packet); break;
                    case 0x810:
                        {
                            NobilityRankType rankType = NobilityRankPacket.ID(Packet);
                            SubID = (int)rankType;
                            switch (rankType)
                            {
                                case NobilityRankType.Listings: ShowNobilityRankings(Client, Packet); break;
                                case NobilityRankType.Donate: DonateNobility(Client, Packet); break;
                            }
                            break;
                        }
                    case 0x2715: Walk(Client, (MovementPacket*)Packet); break;
                    case 0x271A:
                        {
                            DataPacket* dPtr = (DataPacket*)Packet;
                            SubID = (int)dPtr->ID;
                            dPtr->UID = Client.Entity.UID;
                            switch (dPtr->ID)
                            {
                                case DataID.SetLocation: SetLocation(Client, dPtr); break;
                                case DataID.Hotkeys: HotkeysAndInventory(Client, dPtr); break;
                                case DataID.ConfirmSpells: SendSpells(Client, dPtr); break;
                                case DataID.ConfirmProficiencies: SendProficiencies(Client, dPtr); break;
                                case DataID.ConfirmGuild: SendGuild(Client, dPtr); break;
                                case DataID.ConfirmAssociates: SendAssociates(Client, dPtr); break;
                                case DataID.Login: CompleteLogin(Client, dPtr); break;

                                case DataID.GetSurroundings: LoadScreen(Client, dPtr); break;
                                case DataID.Jump: AppendJump(Client, dPtr); break;
                                case DataID.GuardJump: AppendGuardJump(Client, dPtr); break;
                                case DataID.RequestEntity: RequestEntity(Client, dPtr); break;
                                case DataID.ChangeAction: ChangeAction(Client, dPtr); break;
                                case DataID.ChangeDirection: ChangeDirection(Client, dPtr); break;
                                case DataID.ChangePkMode: ChangePKMode(Client, dPtr); break;
                                case DataID.Revive: Revive(Client); break;
                                case DataID.EndTransform: Untransform(Client); break;
                                case DataID.EnterPortal: EnterPortal(Client, dPtr); break;
                                case DataID.RequestFriendInfo: RequestFriendInfo(Client, dPtr); break;
                                case DataID.StartVend: StartVending(Client, dPtr); break;
                                case DataID.ChangeAvatar: ChangeAvatar(Client, dPtr); break;
                                case DataID.RequestTeamPosition: RequestTeamMember(Client, dPtr); break;
                                case DataID.Mining: StartMine(Client, dPtr); break;
                                default: Console.WriteLine("Unknown DataPacket: " + dPtr->ID); break;
                            }
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Kernel.NotifyDebugMsg(string.Format("[Packet Processor - {0}, {1}]", Type.ToString("X4"), SubID.ToString("X2")), e.ToString(), true);
            }
        }
    }
}
