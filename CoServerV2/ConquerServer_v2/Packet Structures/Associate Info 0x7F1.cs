using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public unsafe struct AssociateInfoPacket
    {
        public ushort Size;
        public ushort Type;
        public uint UID;
        public uint Model;
        public byte Level;
        public byte Job;
        public ushort PKPoints;
        public uint GuildID;
        public fixed sbyte Spouse[16];
        public int IsEnemy;
        public fixed byte TQServer[8];

        public static AssociateInfoPacket Create()
        {
            AssociateInfoPacket retn = new AssociateInfoPacket();
            retn.Size = 0x28;
            retn.Type = 0x7F1;
            PacketBuilder.AppendTQServer(retn.TQServer, 8);
            return retn;
        }
    }
}
