using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public enum ItemUsuageID : uint
    {
        BuyItem = 0x01,
        SellItem = 0x02,
        RemoveInventory = 0x03,
        Equip = 0x04,
        SetEquipPosition = 0x05,
        Unequip = 0x06,
        UpgradeEnchant = 0x07,
        ShowWarehouseMoney = 0x09,
        DepositWarehouse = 0x0A,
        WithdrawWarehouse = 0x0B,
        RepairItem = 0x0E,
        UpdateDurability = 0x11,
        RemoveEquipment = 0x12,
        UpgradeDragonball = 0x13,
        UpgradeMeteor = 0x14,
        ShowVendingList = 0x15,
        AddVendingItemGold = 0x16,
        RemoveVendingItem = 0x17,
        BuyVendingItem = 0x18,
        UpdateArrowCount = 0x19,
        ParticleEffect = 0x1A,
        Ping = 0x1B,
        UpdateEnchant = 0x1C,
        AddVendingItemConquerPts = 0x1D,
        UpdatePurity = 0x23,
        DropItem = 0x25,
        DropGold = 0x26
    }

    /// <summary>
    /// 0x3F1 (Client->Server, Server->Client)
    /// </summary>
    public unsafe struct ItemUsuagePacket
    {
        public ushort Size;
        public ushort Type;
        public uint UID;
        public uint dwParam1;
        public ItemUsuageID ID;
        public TIME TimeStamp;
        public uint dwParam2;
        public uint dwParam3;
        public fixed sbyte TQServer[8];

        public static ItemUsuagePacket Create()
        {
            ItemUsuagePacket retn = new ItemUsuagePacket();
            retn.Size = 0x1C;
            retn.Type = 0x3F1;
            retn.TimeStamp = WinMM.timeGetTime();
            PacketBuilder.AppendTQServer((byte*)retn.TQServer, 8);
            return retn;
        }
    }
}
