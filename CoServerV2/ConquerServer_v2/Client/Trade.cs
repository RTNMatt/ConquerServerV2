using System;
using System.Collections.Generic;
using ConquerServer_v2.Core;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Client
{
    public unsafe class ClientTradeSession
    {
        public GameClient Owner;
        public GameClient Partner;
        public bool WindowOpen;
        public bool Confirmed;
        private int Money;
        private int ConquerPoints;
        private FlexibleArray<uint> ItemUIDs;

        /// <summary>
        /// Creates a new trading instance
        /// </summary>
        /// <param name="owner">The owner of this side of the window</param>
        public ClientTradeSession(GameClient owner)
        {
            Owner = owner;
        }
        /// <summary>
        /// Adds money to the trade (safe: implements check sums)
        /// </summary>
        /// <param name="Packet">The packet received to add money.</param>
        public void AddMoney(TradePacket* Packet)
        {
            if (Partner.InTrade)
            {
                Money += (int)Packet->dwParam;
                Money = Math.Max(Math.Min(Money, Owner.Money), 0);
                Packet->ID = TradeID.DisplayMoney;
                Packet->dwParam = (uint)Money;
                Partner.Send(Packet);
            }
        }
        /// <summary>
        /// Add cps to the trade (safe: implements check sums)
        /// </summary>
        /// <param name="Packet">The packet received to add cps.</param>
        public void AddConquerPoints(TradePacket* Packet)
        {
            if (Partner.InTrade)
            {
                ConquerPoints += (int)Packet->dwParam;
                ConquerPoints = Math.Max(Math.Min(ConquerPoints, Owner.ConquerPoints), 0);
                Packet->ID = TradeID.DisplayConquerPoints;
                Packet->dwParam = (uint)ConquerPoints;
                Partner.Send(Packet);
            }
        }
        /// <summary>
        /// Adds an item to the trade
        /// </summary>
        /// <param name="Item">The Item interface to the item</param>
        public void AddItem(Item Item)
        {
            if (Partner.InTrade)
            {
                if (ItemUIDs == null)
                    ItemUIDs = new FlexibleArray<uint>();
                ItemUIDs.Add(Item.UID);
                Item.Mode = ItemMode.Trade;
                Item.Send(Partner);
                Item.Mode = ItemMode.Default;
            }
        }
        /// <summary>
        /// Closes the trading window between both parties
        /// </summary>
        /// <param name="Packet">The packet received when closing the window. If this is null, it will be created.</param>
        public void CloseTrade(TradePacket* Packet)
        {
            if (Packet == null)
            {
                TradePacket temp = TradePacket.Create();
                Packet = &temp;
                Packet->dwParam = Owner.Entity.UID;
            }
            /*TradePartnerPacket partner = TradePartnerPacket.Create();
            partner.ID = TradePartnerID.RemoveTradePartner;
            partner.PartnerUID = Partner.AuthId;
            Client.Send(&partner, partner.Size);*/
            if (Partner.InTrade)
            {
                //partner.PartnerUID = Client.AuthId;
                //Partner.Send(&partner, partner.Size);

                Packet->ID = TradeID.CloseTradeWindow;
                Partner.Send(Packet);

                this.RestoreInventoryItems();
                Owner.Trade = null;
                Partner.Trade.RestoreInventoryItems();
                Partner.Trade = null;
            }
        }
        private void RestoreInventoryItems()
        {
            if (ItemUIDs != null)
            {
                for (int i = 0; i < ItemUIDs.Length; i++)
                {
                    Item item = Owner.Inventory.Search(ItemUIDs.Elements[i]);
                    if (item != null)
                    {
                        item.Mode = ItemMode.Default;
                        item.Send(Owner);
                    }
                }
            }
        }
        public void ExchangeMoney()
        {
            Owner.Money += (Partner.Trade.Money - Owner.Trade.Money);
            Partner.Money += (Owner.Trade.Money - Partner.Trade.Money);
            Owner.ConquerPoints += (Partner.Trade.ConquerPoints - Owner.Trade.ConquerPoints);
            Partner.ConquerPoints += (Owner.Trade.ConquerPoints - Partner.Trade.ConquerPoints);

            BigUpdatePacket big = new BigUpdatePacket(2);
            big.UID = Owner.Entity.UID;
            big.Append(0, UpdateID.Money, (uint)Owner.Money);
            big.Append(1, UpdateID.ConquerPoints, (uint)Owner.ConquerPoints);
            Owner.Send(big);
            big.UID = Partner.Entity.UID;
            big.Append(0, UpdateID.Money, (uint)Partner.Money);
            big.Append(1, UpdateID.ConquerPoints, (uint)Partner.ConquerPoints);
            Partner.Send(big);
        }

        public bool ValidateItems(out byte[] ClientItemSlots, out byte[] PartnerItemSlots)
        {
            List<byte> ItemsBuffer = new List<byte>();
            bool ItemOk = true;
            if (ItemUIDs != null)
            {
                for (byte b = 0; b < Owner.Inventory.MaxPossibleItems; b++)
                {
                    for (int i = 0; i < ItemUIDs.Length; i++)
                    {

                        if (Owner.Inventory[b] != null)
                        {
                            if (Owner.Inventory[b].UID == ItemUIDs.Elements[i])
                            {
                                ItemsBuffer.Add(b);
                                ItemUIDs.Remove(i);
                                break;
                            }
                        }
                    }
                }
                ItemOk = ItemOk && (ItemUIDs.Length == 0);
            }
            ClientItemSlots = ItemsBuffer.ToArray();
            ItemsBuffer.Clear();
            if (Partner.Trade.ItemUIDs != null)
            {
                for (byte b = 0; b < Partner.Inventory.MaxPossibleItems; b++)
                {
                    for (int i = 0; i < Partner.Trade.ItemUIDs.Length; i++)
                    {

                        if (Partner.Inventory[b] != null)
                        {
                            if (Partner.Inventory[b].UID == Partner.Trade.ItemUIDs.Elements[i])
                            {
                                ItemsBuffer.Add(b);
                                Partner.Trade.ItemUIDs.Remove(i);
                                break;
                            }
                        }
                    }
                }
                ItemOk = ItemOk && (Partner.Trade.ItemUIDs.Length == 0);
            }
            PartnerItemSlots = ItemsBuffer.ToArray();
            return ItemOk;
        }
    }
}