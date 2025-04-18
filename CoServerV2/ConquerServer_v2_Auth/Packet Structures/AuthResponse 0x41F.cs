using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    /// <summary>
    /// 0x41F (Server->Client)
    /// </summary>
    public unsafe struct AuthResponsePacket
    {
        public ushort Size;
        public ushort Type;
        public int Key2;
        public uint Key1;
        public ushort Port;

        public ushort unknown1;
        public byte unknown2;
        public byte unknown3;
        public byte unknown4;
        public byte unknown5;

        private fixed sbyte szIPAddress[16];

        public unsafe string IPAddress
        {
            get { fixed (sbyte* bp = szIPAddress) { return new string(bp); } }
            set
            {
                string ip = value;
                fixed (sbyte* bp = szIPAddress)
                {
                    for (int i = 0; i < ip.Length; i++)
                        bp[i] = (sbyte)ip[i];
                }
            }
        }

        public static AuthResponsePacket Create()
        {
            AuthResponsePacket retn = new AuthResponsePacket();
            retn.Size = 0x52;
            retn.Type = 0x41F;
            retn.unknown1 = 0;
            retn.unknown2 = 190;
            retn.unknown3 = 125;
            retn.unknown4 = 48;
            retn.unknown5 = 16;
            return retn;
        }
    }
}