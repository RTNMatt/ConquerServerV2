using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public enum VendMode : ushort
    {
        VendByGold = 0x01,
        VendByConquerPoints = 0x03
    }

    public unsafe struct VendingItemPacket
    {
        public ushort Size;
        public ushort Type;
        public uint UID;
        public uint ShopID;
        public int Price;
        public uint ItemID;
        public short Amount;
        public short MaxAmount;
        public VendMode Mode;
        public fixed byte Junk[4]; // (?)
        public short RebornEffect;
        public byte SocketOne;
        public byte SocketTwo;
        public ushort wUnknown;
        public byte Plus;
        public byte Bless;
        public byte Enchant;
        public fixed byte Junk2[9]; // suspicious, locked (?)
        public uint Color;
        public int ComposeProgress;
        public fixed byte TQServer[8];

        public static VendingItemPacket Create()
        {
            VendingItemPacket retn = new VendingItemPacket();
            retn.Size = 0x38;
            retn.Type = 0x454;
            PacketBuilder.AppendTQServer(retn.TQServer, 8);
            return retn;
        }
    }
}
