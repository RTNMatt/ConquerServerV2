using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    [StructLayout(LayoutKind.Sequential, Size = 0x14 + 8)]
    public unsafe struct MovementPacket
    {
        public ushort Size;
        public ushort Type;
        public int Direction;
        public uint UID;
        public int Running;
        public TIME TimeStamp;
        private fixed byte TQServer[8];

        public static MovementPacket Create()
        {
            MovementPacket retn = new MovementPacket();
            retn.Size = 0x14;
            retn.Type = 0x2715;
            PacketBuilder.AppendTQServer(retn.TQServer, 8);
            return retn;
        }
    }
}