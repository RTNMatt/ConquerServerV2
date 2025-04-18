using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Database
{
    public unsafe struct DatabaseItem
    {
        public uint OldUID;
        public uint ID;
        public short RebornEffects;
        public ItemPosition Position;
        public byte Plus;
        public byte Bless;
        public byte Enchant;
        public byte SocketOne;
        public byte SocketTwo;
        public byte Color;
        public int ComposeProgress;
        public short Durability;
        public short Arrows { get { return Durability; } set { Durability = value; } }
        public short MaxDurability;

        public void FromItem(Item item)
        {
            OldUID = item.UID;
            ID = item.ID;
            RebornEffects = item.RebornEffects;
            Position = item.Position;
            Bless = item.Bless;
            Plus = item.Plus;
            Enchant = item.Enchant;
            SocketOne = item.SocketOne;
            SocketTwo = item.SocketTwo;
            Color = item.Color;
            ComposeProgress = item.ComposeProgress;
            Durability = item.Durability;
            MaxDurability = item.MaxDurability;
        }
        public Item GetItem()
        {
            uint uid;
            return GetItem(out uid);
        }
        public Item GetItem(out uint OldUID)
        {
            OldUID = this.OldUID;
            Item item = new Item();
            item.UID = Item.NextUID;
            item.ID = ID;
            item.Plus = Math.Min(Plus, Item.MaxPlus);
            item.Bless = Math.Min(Bless, Item.MaxBless);
            item.Enchant = Enchant;
            item.Position = Position;
            item.RebornEffects = RebornEffects;
            item.SocketOne = SocketOne;
            item.SocketTwo = SocketTwo;
            item.Color = Color;
            item.ComposeProgress = ComposeProgress;
            item.Durability = Durability;
            item.MaxDurability = MaxDurability;
            return item;
        }
    }
}
