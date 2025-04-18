using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    /// <summary>
    /// 0x43E (Client->Server)
    /// </summary>
    public unsafe struct LoginPacket
    {
        public const ushort cSize = 0x114;
        public const ushort cType = 1060;

        public ushort Size;
        public ushort Type;
        public fixed sbyte szUser[128];
        public fixed uint szPassword[32];
        public fixed sbyte szServer[16];

        public string User { get { fixed (sbyte* ptr = szUser) { return new string(ptr); } } }
        public string Server { get { fixed (sbyte* ptr = szServer) { return new string(ptr); } } }
        public string Password { get { fixed (uint* ptr = szPassword) { return new string((sbyte*)ptr); } } }
    }
}
