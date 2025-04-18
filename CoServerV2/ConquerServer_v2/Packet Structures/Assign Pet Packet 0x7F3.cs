using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public unsafe struct AssignPetPacket
    {
        public ushort Size;
        public ushort Type;
        public uint UID;
        public uint Model;
        public uint Unknown1; // AI Type?
        public ushort X;
        public ushort Y;
        public fixed sbyte Name[16];
        public fixed sbyte Padding[20];
        public fixed byte TQServer[8];

        public static AssignPetPacket Create()
        {
            AssignPetPacket packet = new AssignPetPacket();
            packet.Size = 0x38;
            packet.Type = 0x7F3;
            PacketBuilder.AppendTQServer(packet.TQServer, 8);
            return packet;
        }
    }
}
