using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public enum GuildRank : byte
    {
        None = 0x00,
        Member = 0x32,
        InternManager = 0x3C,
        DeputyManager = 0x46,
        BranchManager = 0x50,
        DeputyLeader = 0x5A,
        Leader = 0x64
    }

    public unsafe struct GuildInfoPacket
    {
        public ushort Size;
        public ushort Type;
        public uint ID;
        public uint Donation;
        public uint Fund;
        public uint MemberCount;
        public GuildRank Rank;
        public fixed sbyte Leader[16];
        public fixed sbyte Junk[3];
        public fixed byte TQServer[8];

        public static GuildInfoPacket Create()
        {
            GuildInfoPacket retn = new GuildInfoPacket();
            retn.Size = 0x28;
            retn.Type = 0x452;
            PacketBuilder.AppendTQServer(retn.TQServer, 8);
            return retn;
        }
    }
}