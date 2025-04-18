using System;
using System.Collections;
using System.Collections.Generic;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Client
{
    public enum InventoryErrNo
    {
        SUCCESS,
        ID_IS_ZERO,
        INVENTORY_FULL,
        FAILED_FIND_FREE_SLOT,
        ITEM_NOT_FOUND
    }

    public unsafe class ClientInventory
    {
        private GameClient Client;
        private byte Count;
        private Item[] Items;

        public byte ItemCount { get { return Count; } set { Count = value; } }
        public int MaxPossibleItems { get { return Items.Length; } }

        public ClientInventory(GameClient _Client)
        {
            Client = _Client;
            Count = 0;
            Items = new Item[40];
        }
        public Item this[int index]
        {
            get { return Items[index]; }
            set { Items[index] = value; }
        }

        public InventoryErrNo Add(Item Item)
        {
            lock (this)
            {
                if (Item.ID != 0)
                {
                    if (Count < 40)
                    {
                        byte Slot;
                        if (FindFreeSlot(out Slot))
                        {
                            Item.Position = ItemPosition.Inventory;
                            Items[Slot] = Item;
                            Item.Send(Client);
                            Count++;
                            return InventoryErrNo.SUCCESS;
                        }
                        return InventoryErrNo.FAILED_FIND_FREE_SLOT;
                    }
                    return InventoryErrNo.INVENTORY_FULL;
                }
                return InventoryErrNo.ID_IS_ZERO;
            }
        }
        public InventoryErrNo Remove(uint UID)
        {
            lock (this)
            {
                for (byte i = 0; i < 40; i++)
                {
                    if (Items[i] != null)
                    {
                        if (Items[i].UID == UID)
                        {
                            return RemoveBySlot(i);
                        }
                    }
                }
                return InventoryErrNo.ITEM_NOT_FOUND;
            }
        }
        public InventoryErrNo RemoveBySlot(byte Slot, bool RemovePacket)
        {
            lock (this)
            {
                if (Items[Slot] != null)
                {
                    if (Items[Slot].ID != 0)
                    {
                        if (RemovePacket)
                        {
                            ItemUsuagePacket Packet = ItemUsuagePacket.Create();
                            Packet.ID = ItemUsuageID.RemoveInventory;
                            Packet.UID = Items[Slot].UID;
                            Client.Send(&Packet);
                        }
                        Items[Slot] = null;
                        Count--;
                        return InventoryErrNo.SUCCESS;
                    }
                    return InventoryErrNo.ID_IS_ZERO;
                }
                return InventoryErrNo.ITEM_NOT_FOUND;
            }
        }
        public InventoryErrNo RemoveBySlot(byte Slot)
        {
            return RemoveBySlot(Slot, true);
        }
        public Item Search(uint UID)
        {
            for (byte i = 0; i < 40; i++)
            {
                if (Items[i] != null)
                {
                    if (Items[i].UID == UID)
                    {
                        return Items[i];
                    }
                }
            }
            return null;
        }
        public Item Search(uint UID, out byte Slot)
        {
            Slot = 255;
            for (byte i = 0; i < 40; i++)
            {
                if (Items[i] != null)
                {
                    if (Items[i].UID == UID)
                    {
                        Slot = i;
                        return Items[i];
                    }
                }
            }
            return null;
        }
        public byte CountItem(uint ItemID)
        {
            byte retn = 0;
            for (byte i = 0; i < 40; i++)
            {
                if (Items[i] != null)
                {
                    if (Items[i].ID == ItemID)
                    {
                        retn++;
                    }
                }
            }
            return retn;
        }
        public bool Contains(uint ItemID)
        {
            return (CountItem(ItemID) > 0);
        }
        private bool FindFreeSlot(out byte rSlot)
        {
            rSlot = 255;
            for (byte i = 0; i < 40; i++)
            {
                if (Items[i] == null)
                {
                    rSlot = i;
                    break;
                }
            }
            return (rSlot != 255);
        }
    }
}