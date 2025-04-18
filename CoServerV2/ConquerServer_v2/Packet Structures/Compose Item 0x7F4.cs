using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    /// <summary>
    /// 0x7F4 (Client->Server)
    /// </summary>
    public unsafe struct ComposeItemPacket
    {
        public ushort Size;
        public ushort Type;
        public uint dwJunk;
        public uint MainItem;
        public uint MinorItem;
        public fixed sbyte TQServer[8];
    }
}
