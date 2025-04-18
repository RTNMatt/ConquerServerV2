using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public enum SOBType : ushort
    {
        Gate = 0x1A,
        Scarecrow = 0x16,
        Stake = 0x15,
        Pole = 0x0A
    }
    public enum SOBMesh : ushort
    {
        LeftGate = 0x00F1,
        RightGate = 0x0115,
        Pole = 0x471
    }

    /// <summary>
    /// 0x455 (Server->Client)
    /// </summary>
    public unsafe struct SpawnSOBPacket
    {
        public ushort Size;
        public ushort Type;
        public uint UID;
        public int MaxHitpoints;
        public int Hitpoints;
        public ushort X;
        public ushort Y;
        public SOBMesh SOBMesh;
        public SOBType SOBType;
        public ushort Facing;
        public bool ShowName;
        public byte NameLength;
        public fixed byte Strings[24];

        public static SpawnSOBPacket Create()
        {
            SpawnSOBPacket Data = new SpawnSOBPacket();
            Data.Size = 0x1C;
            Data.Type = 0x455;  
            PacketBuilder.AppendTQServer(Data.Strings, 8);
            return Data;
        }
    }
}
