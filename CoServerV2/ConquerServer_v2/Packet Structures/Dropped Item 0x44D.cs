using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;

namespace ConquerServer_v2.Packet_Structures
{
    public enum DropID : ushort
    {
        Visible = 0x01,
        Remove = 0x02,
        Pickup = 0x03
    }

    /// <summary>
    /// 0x44D (Server->Client)
    /// </summary>
    public unsafe struct DroppedItemPacket : IMapObject, IDroppedItem
    {
        public struct SmallItemData
        {
            public byte Plus;
            public byte Bless;
            public byte Enchant;
            public byte SocketOne;
            public byte SocketTwo;
            public short Durability;
            public short MaxDurability;
        }

        public ushort Size;
        public ushort Type;
        private uint m_UID;
        private uint m_ID;
        private ushort m_X;
        private ushort m_Y;
        private ushort m_Color;
        public DropID DropType;
        public fixed sbyte TQServer[8];
        private MapID m_MapID;
        private TIME m_RemoveTime;
        private TIME m_ProtectionTime;
        public SmallItemData Item;
        private uint m_KillerUID;
        private int m_Gold;

        public bool IsGold()
        {
            return (ID >= 1090000 && ID <= 1090020) || (ID >= 1091000 && ID <= 1091020);
        }

        // Required for IMapObject
        public uint UID { get { return m_UID; } }
        public ushort X { get { return m_X; } set { m_X = value; } }
        public ushort Y { get { return m_Y; } set { m_Y = value; } }
        public MapID MapID { get { return m_MapID; } set { m_MapID = value; } }
        public MapObjectType MapObjType { get { return MapObjectType.Item; } }
        public object Owner { get { return this; } }
        public void SendSpawn(GameClient Client)
        {
            if (Client.Screen.Add(this))
            {
                fixed (DroppedItemPacket* pItem = &this)
                {
                    Client.Send(pItem);
                }
            }
        }

        public TIME RemoveTime { get { return m_RemoveTime; } }
        public uint ID { get { return m_ID; }  }
        public ushort Color { get { return m_Color; } }
        public int Gold { get { return m_Gold; } set { m_Gold = value; } }
        public Item GetItem()
        {
            Item original = new Item();
            original.UID = m_UID;
            original.ID = ID;
            original.Plus = Item.Plus;
            original.Bless = Item.Bless;
            original.Enchant = Item.Enchant;
            original.SocketOne = Item.SocketOne;
            original.SocketTwo = Item.SocketTwo;
            original.Durability = Item.Durability;
            original.MaxDurability = Item.MaxDurability;
            original.Color = (byte)m_Color;
            return original;
        }
        public uint KillerUID { get { return m_KillerUID; } }
        public TIME ProtectionTime { get { return m_ProtectionTime; } }

        public static DroppedItemPacket Create(Item Original)
        {
            DroppedItemPacket retn = new DroppedItemPacket();
            retn.Size = 0x14;
            retn.Type = 0x44D;
            retn.DropType = DropID.Visible;
            retn.m_Color = Original.Color;
            PacketBuilder.AppendTQServer((byte*)retn.TQServer, 8);
            retn.m_RemoveTime = TIME.Now.AddSeconds(30);
            retn.m_ID = Original.ID;
            retn.m_UID = Original.UID;
            retn.Item.Plus = Original.Plus;
            retn.Item.Bless = Original.Bless;
            retn.Item.Enchant = Original.Enchant;
            retn.Item.SocketOne = Original.SocketOne;
            retn.Item.SocketTwo = Original.SocketTwo;
            retn.Item.Durability = Original.Durability;
            retn.Item.MaxDurability = Original.MaxDurability;
            return retn;
        }
        public static DroppedItemPacket Create(Item Original, uint ProtectUID, TIME ProtectTime)
        {
            DroppedItemPacket retn = Create(Original);
            if (ProtectUID != 0)
            {
                retn.m_KillerUID = ProtectUID;
                retn.m_ProtectionTime = ProtectTime;
            }
            return retn;
        }
        public static DroppedItemPacket Create(uint ItemId, int GoldAmount)
        {
            DroppedItemPacket retn = new DroppedItemPacket();
            retn.Size = 0x14;
            retn.Type = 0x44D;
            retn.DropType = DropID.Visible;
            PacketBuilder.AppendTQServer((byte*)retn.TQServer, 8);
            retn.m_RemoveTime = TIME.Now.AddSeconds(30);
            retn.m_ID = ItemId;
            retn.m_UID = ConquerServer_v2.Core.Item.NextUID;
            retn.Gold = GoldAmount;
            return retn;
        }
        public static DroppedItemPacket Create(uint ItemId, int GoldAmount, uint ProtectUID, TIME ProtectTime)
        {
            DroppedItemPacket retn = Create(ItemId, GoldAmount);
            if (ProtectUID != 0)
            {
                retn.m_KillerUID = ProtectUID;
                retn.m_ProtectionTime = ProtectTime;
            }
            return retn;
        }
    }
}