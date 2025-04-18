using System;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Client
{
    public unsafe class ClientEquipment
    {
        private Item[] Items;
        private GameClient Client;
        public int ItemCount { get { return Items.Length; } }

        public ClientEquipment(GameClient _Client)
        {
            Client = _Client;
            Items = new Item[(int)Item.LastSlot];
        }
        public Item this[ItemPosition Position]
        {
            get { return Items[((ushort)Position) - 1]; }
            set
            {
                if (value != null)
                {
                    value.Position = Position;
                    SetSlot(Position, value.ID, value.Color);
                }
                else
                {
                    SetSlot(Position, 0, 0);
                }
                Items[((ushort)Position) - 1] = value;
            }
        }

        [Obsolete("Use GameClient.Equip() instead of calling this function directly.")]
        /// <summary>
        /// Equips an item to the owner of this class. This does not remove the item in the inventory.
        /// This does not either tell the client to re-calculate bonus, damage, or potency.
        /// </summary>
        public void Equip(Item Item, ItemPosition Slot)
        {
            Item.Position = Slot;
            ServerDatabase.LoadItemStats(Client, Item);
            this[Slot] = Item;
            SetSlot(Slot, Item.ID, Item.Color);
        }
        [Obsolete("Use GameClient.Unequip() instead of calling this function directly.")]
        /// <summary>
        /// Unequips an item from the owner of this class. This does not add the item in the inventory.
        /// This does not either tell the client to re-calculate bonus, damage, or potency.
        /// </summary>
        public Item Unequip(ItemPosition Slot)
        {
            Item old = this[Slot];
            if (old != null)
            {
                ServerDatabase.UnloadItemStats(Client, old);
                this[Slot] = null;
                SetSlot(Slot, 0, 0);
            }
            return old;
        }
        /// <summary>
        /// This should only be called after the database loads the elements nested in this class.
        /// </summary>
        public void Initialize()
        {
            foreach (Item Equipment in Items)
            {
                if (Equipment != null)
                {
                    SetSlot(Equipment.Position, Equipment.ID, Equipment.Color);
                }
            }
        }
        public void SetSlot(ItemPosition ItemSlot, uint ID, byte Color)
        {
            switch (ItemSlot)
            {
                case ItemPosition.Garment:
                    {
                        Client.Entity.Spawn.ArmorID = ID; 
                        Client.Entity.Spawn.ArmorColor = Color;
                        if (ID == 0)
                        {
                            Item item = this[ItemPosition.Armor];
                            if (item != null)
                            {
                                Client.Entity.Spawn.ArmorID = item.ID;
                                Client.Entity.Spawn.ArmorColor = item.Color;
                            }
                        }
                        break;
                    }
                case ItemPosition.Head:
                    {
                        Client.Entity.Spawn.HelmetID = ID;
                        Client.Entity.Spawn.HeadColor = Color;
                        break;
                    }
                case ItemPosition.Armor:
                    {
                        Client.Entity.Spawn.ArmorID = ID; 
                        Client.Entity.Spawn.ArmorColor = Color;
                        break;
                    }
                case ItemPosition.Right: Client.Entity.Spawn.RightHandID = ID; break;
                case ItemPosition.Left: 
                    {
                        Client.Entity.Spawn.LeftHandID = ID;
                        if ((int)(ID / 1000) == ItemTypeConst.ShieldID)
                            Client.Entity.Spawn.ShieldColor = Color;
                        break;
                    }
            }
        }
    }
}