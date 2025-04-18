using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public unsafe struct GuildMemberInfoPacket
    {
        public ushort Size;
        public ushort Type;
        public int Donation;
#pragma warning disable
        private byte unknown;
#pragma warning restore
        public fixed sbyte MemberName[16];
        public GuildRank GuildRank;
#pragma warning disable
        private byte _GuildRank;
#pragma warning restore
        public fixed byte TQServer[8];

        public static GuildMemberInfoPacket Create()
        {
            GuildMemberInfoPacket retn = new GuildMemberInfoPacket();
            retn.Size = 0x1C;
            retn.Type = 0x458;
            PacketBuilder.AppendTQServer(retn.TQServer, 8);
            return retn;
        }
    }
}
