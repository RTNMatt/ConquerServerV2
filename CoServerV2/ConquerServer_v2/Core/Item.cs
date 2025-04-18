using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerScriptLinker;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Core
{
    public class ItemTypeConst
    {
        public const ushort
            BowID = 500,
            ArrowID = 1050,
            ShieldID = 900,
            BackswordID = 421,
            SwordID = 420,
            NinjaSwordID = 601,
            FanID = 201,
            TowerID = 202,
            GarmentID_1 = 181,
            GarmentID_2 = 182,
            BottleID = 2100,
            GemID = 700,
            PickaxeID = 562;

        public const uint
            GMRobeID = 137010;
    }

    public interface IDroppedItem
    {
        uint UID { get; }
        uint ID { get; }
        ushort Color { get; }
        ushort X { get; }
        ushort Y { get; }
        MapID MapID { get; }
        int Gold { get; set; }
        TIME RemoveTime { get; }
        TIME ProtectionTime { get; }
        uint KillerUID { get; }
        Item GetItem();
        bool IsGold();
        void SendSpawn(GameClient Client);
    }

    public unsafe class Item : INpcItem
    {
        public const ItemPosition FirstSlot = ItemPosition.Head;
        public const ItemPosition LastSlot = ItemPosition.DefenceTalisman;
        public const byte MaxBless = 7;
        public const byte MaxPlus = 9;

        private static uint m_NextUID;
        private const uint UID_Start = 10000000, UID_End = 99999999;
        private static Item m_Blank;

        static Item()
        {
            m_NextUID = UID_Start;
            m_Blank = new Item();
        }
        public static uint NextUID
        {
            get
            {
                if (m_NextUID >= UID_End)
                    m_NextUID = UID_Start;
                return m_NextUID++;
            }
        }
        public static Item Blank { get { return m_Blank; } }

        public ItemPacket Data;

        public Item()
        {
            Data = ItemPacket.Create();
            Data.UID = NextUID;
        }

        public uint UID { get { return Data.UID; } set { Data.UID = value; } }
        public uint ID 
        { 
            get { return Data.ID; } 
            set 
            { 
                Data.ID = value;
                if (value >= 730001 && value <= 730009)
                    Plus = (byte)(value - 730000);
            } 
        }
        public short Durability { get { return Data.Amount; } set { Data.Amount = value; } }
        public short Arrows { get { return Data.Amount; } set { Data.Amount = value; } }
        public short MaxDurability { get { return Data.MaxAmount; } set { Data.MaxAmount = value; } }
        public ItemMode Mode { get { return Data.Mode; } set { Data.Mode = value; } }
        public ItemPosition Position { get { return Data.Position; } set { Data.Position = value; } }
        public ushort CurrentPosition { get { return (ushort)Data.Position; } set { Data.Position = (ItemPosition)value; } }
        public short RebornEffects { get { return Data.RebornEffects; } set { Data.RebornEffects = (byte)value; } }
        public byte SocketOne { get { return Data.SocketOne; } set { Data.SocketOne = value; } }
        public byte SocketTwo { get { return Data.SocketTwo; } set { Data.SocketTwo = value; } }
        public byte Plus { get { return Data.Plus; } set { Data.Plus = value; } }
        public byte Bless { get { return Data.Bless; } set { Data.Bless = value; } }
        public byte Enchant { get { return Data.Enchant; } set { Data.Enchant = value; } }
        public byte Color { get { return (byte)Data.Color; } set { Data.Color = value; } }
        public int ComposeProgress { get { return Data.ComposeProgress; } set { Data.ComposeProgress = value; } }
        public bool Free { get { return Data.Free; } set { Data.Free = value; } }

        public void Send(GameClient Client)
        {
            fixed (ItemPacket* lpPacket = &Data)
            {
                Client.Send(lpPacket);
            }
        }
        public void Send(INpcPlayer Client)
        {
            fixed (ItemPacket* lpPacket = &Data)
            {
                Client.Send(lpPacket);
            }
        }
        public void SendInventoryUpdate(GameClient Client)
        {
            ItemUsuagePacket remove = ItemUsuagePacket.Create();
            remove.ID = ItemUsuageID.RemoveInventory;
            remove.UID = this.UID;
            Client.Send(&remove);
            this.Position = ItemPosition.Inventory;
            this.Send(Client);
        }
        public void SendDurability(GameClient Client)
        {
            ItemUsuagePacket dura = ItemUsuagePacket.Create();
            dura.ID = ItemUsuageID.UpdateDurability;
            dura.UID = this.UID;
            dura.dwParam1 = (uint)Durability;
            Client.Send(&dura);
        }
        public void SendArrows(GameClient Client)
        {
            ItemUsuagePacket arr = ItemUsuagePacket.Create();
            arr.ID = ItemUsuageID.UpdateArrowCount;
            arr.UID = this.UID;
            arr.dwParam1 = (uint)Arrows;
            Client.Send(&arr);
        }
        public bool IsItemType(int ID)
        {
            return (((int)(this.ID / 1000)) == ID);
        }
        public bool IsTwoHander()
        {
            ushort item_type = GetItemType();
            bool check = (item_type >= 500);
            if (check)
            {
                check = (item_type != ItemTypeConst.ShieldID);
                if (check)
                {
                    check = (item_type != ItemTypeConst.NinjaSwordID);
                }
            }
            return check;
        }
        public bool IsGarment()
        {
            return IsItemType(ItemTypeConst.GarmentID_1) || IsItemType(ItemTypeConst.GarmentID_2);
        }
        public bool IsTalismen()
        {
            return IsItemType(ItemTypeConst.FanID) || IsItemType(ItemTypeConst.TowerID);
        }
        public int GetSmallItemType()
        {
            return (int)(ID / 10000);
        }
        public ushort GetItemType()
        {
            return (ushort)(ID / 1000);
        }
        public byte GetQuality()
        {
            return (byte)(ID % 10);
        } 
    }


    public unsafe class VendingItem
    {
        public VendingItemPacket Data;
        public VendingItem()
        {
            Data = VendingItemPacket.Create();
        }
        public uint UID { get { return Data.UID; } set { Data.UID = value; } }
        public uint ID { get { return Data.ItemID; } set { Data.ItemID = value; } }
        public short Durability { get { return Data.Amount; } set { Data.Amount = value; } }
        public short Arrows { get { return Data.Amount; } set { Data.Amount = value; } }
        public short MaxDurability { get { return Data.MaxAmount; } set { Data.MaxAmount = value; } }
        public VendMode Mode { get { return Data.Mode; } set { Data.Mode = value; } }
        public short RebornEffects { get { return Data.RebornEffect; } set { Data.RebornEffect = value; } }
        public byte SocketOne { get { return Data.SocketOne; } set { Data.SocketOne = value; } }
        public byte SocketTwo { get { return Data.SocketTwo; } set { Data.SocketTwo = value; } }
        public byte Plus { get { return Data.Plus; } set { Data.Plus = value; } }
        public byte Bless { get { return Data.Bless; } set { Data.Bless = value; } }
        public byte Enchant { get { return Data.Enchant; } set { Data.Enchant = value; } }
        public byte Color { get { return (byte)Data.Color; } set { Data.Color = value; } }
        public int ComposeProgress { get { return Data.ComposeProgress; } set { Data.ComposeProgress = value; } }
        public int Price { get { return Data.Price; } set { Data.Price = value; } }
        public uint ShopID { get { return Data.ShopID; } set { Data.ShopID = value; } }

        public void Send(GameClient Client)
        {
            fixed (VendingItemPacket* lpPacket = &Data)
            {
                Client.Send(lpPacket);
            }
        }
        public void FromItem(Item item, bool VendByGold)
        {
            UID = item.UID;
            ID = item.ID;
            Durability = item.Durability;
            MaxDurability = item.MaxDurability;
            Mode = VendByGold ? VendMode.VendByGold : VendMode.VendByConquerPoints;
            RebornEffects = item.RebornEffects;
            SocketOne = item.SocketOne;
            SocketTwo = item.SocketTwo;
            Plus = item.Plus;
            Bless = item.Bless;
            Enchant = item.Enchant;
            Color = item.Color;
            ComposeProgress = item.ComposeProgress;
        }
        public Item ToItem()
        {
            Item item = new Item();
            item.UID = UID;
            item.ID = ID;
            item.Durability = Durability;
            item.MaxDurability = MaxDurability;
            item.RebornEffects = RebornEffects;
            item.SocketOne = SocketOne;
            item.SocketTwo = SocketTwo;
            item.Plus = Plus;
            item.Bless = Bless;
            item.Enchant = Enchant;
            item.Color = Color;
            item.ComposeProgress = ComposeProgress;
            return item;
        }
    }
}
