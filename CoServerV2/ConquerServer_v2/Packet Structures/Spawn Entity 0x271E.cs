using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Structures
{
    /// <summary>
    /// 0x271E (Server->Client)
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct SpawnEntityPacket
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Type;
        [FieldOffset(4)]
        public uint Model;
        [FieldOffset(8)]
        public uint UID;
        [FieldOffset(12)]
        public ushort GuildID;
        [FieldOffset(16)]
        public GuildRank GuildRank;
        [FieldOffset(22)]
        public ulong StatusFlag;
        [FieldOffset(30)]
        public ulong StatusFlag2;

        [FieldOffset(40)]
        public uint HelmetID;
        [FieldOffset(44)]
        public uint GarmId;
        [FieldOffset(48)]
        public uint ArmorID;
        [FieldOffset(52)]
        public uint LeftHandID;
        [FieldOffset(56)]
        public uint RightHandID;
        [FieldOffset(68)]
        public uint HorseID;

        [FieldOffset(80)]
        public ushort Hitpoints;
        [FieldOffset(99)]
        public ushort Level;
        [FieldOffset(84)]
        public ushort Hairstyle;
        [FieldOffset(86)]
        public ushort X;
        [FieldOffset(88)]
        public ushort Y;
        [FieldOffset(90)]
        public ConquerAngle Facing;
        [FieldOffset(91)]
        public ConquerAction Action;
        [FieldOffset(98)]
        public byte Reborn;

        [FieldOffset(69)]
        public ushort LevelPotency;
        [FieldOffset(119)]
        public NobilityID Nobility;

        [FieldOffset(123)]
        public ushort ArmorColor;
        [FieldOffset(107)]
        public ushort ShieldColor;
        [FieldOffset(109)]
        public ushort HeadColor;

        [FieldOffset(218)]
        public byte StringsCount;
        [FieldOffset(219)]
        public byte NameLength;
        [FieldOffset(220)]
        public fixed byte Strings[24];

        public void SetName(string value)
        {
            string m_Name = value;
            if (m_Name.Length > 15)
                m_Name = m_Name.Substring(0, 15);
            Size = (byte)(220 + m_Name.Length);
            StringsCount = 3;
            NameLength = (byte)m_Name.Length;
            fixed (byte* ptr = Strings)
            {
                MSVCRT.memset(ptr, 0, 24);
                value.CopyTo(ptr);
                PacketBuilder.AppendTQServer(ptr + NameLength, 8);
            }
        }

        public static SpawnEntityPacket Create()
        {
            // Size and TQServer are appended when SetName() is called.
            SpawnEntityPacket packet = new SpawnEntityPacket();
            packet.Type = 0x271E;
            return packet;
        }
    }
}
