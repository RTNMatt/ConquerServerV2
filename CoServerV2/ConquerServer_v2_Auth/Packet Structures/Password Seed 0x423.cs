using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    /// <summary>
    /// 0x423 (Server->Client)
    /// </summary>
    public unsafe struct PasswordSeed
    {
        public ushort Size;
        public ushort Type;
        public uint Seed;

        public static PasswordSeed Create()
        {
            PasswordSeed retn = new PasswordSeed();
            retn.Size = 0x08;
            retn.Type = 0x423;
            retn.Seed = 123;
            return retn;
        }
    }
}
