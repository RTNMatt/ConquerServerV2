using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public unsafe struct GemSocketPacket
    {
        public ushort Size;
        public ushort Type;
        public uint UID;
        public uint TargetItemUID;
        public uint GemItemUID;
        public ushort SocketNumber;
        public ushort wPadding;
        public fixed sbyte TQServer[8];
    }
}