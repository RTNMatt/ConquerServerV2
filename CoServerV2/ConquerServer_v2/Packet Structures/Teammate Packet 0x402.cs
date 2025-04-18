using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public unsafe struct TeammatePacket
    {
        public ushort Size;
        public ushort Type;
        public uint dwUnknown;
        public fixed sbyte szName[16];
        public uint UID;
        public uint Model;
        public ushort MaxHP;
        public ushort HP;
        //public fixed byte StupidTQPad[7*16];
        private fixed byte TQServer[8];

        public static TeammatePacket Create()
        {
            TeammatePacket packet = new TeammatePacket();
            packet.Size = (byte)(0x94 - (7*16));
            packet.Type = 0x402;
            packet.dwUnknown = 0x100;
            PacketBuilder.AppendTQServer(packet.TQServer, 8);
            return packet;
        }
    }
}
