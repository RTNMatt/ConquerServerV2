using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public enum GuildRequestID : uint
    {
        RequestJoin = 0x01,
        AcceptJoin = 0x02,
        Quit = 0x03,
        Donate = 0x0B,
        RequestInfo = 0x0C
    }

    public struct GuildRequestPacket
    {
        public ushort Size;
        public ushort Type;
        public GuildRequestID ID;
        public uint dwParam;
    }
}
