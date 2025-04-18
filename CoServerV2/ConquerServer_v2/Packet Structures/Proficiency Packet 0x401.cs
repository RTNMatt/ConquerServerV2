using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerScriptLinker;

namespace ConquerServer_v2.Packet_Structures
{
    /// <summary>
    /// 0x401 (Server->Client)
    /// </summary>
    public unsafe struct ProficiencyPacket
    {
        public ushort Size;
        public ushort Type;
        public uint ID;
        public uint Level;
        public int Experience;
        private fixed byte TQServer[8];

        public static ProficiencyPacket Create()
        {
            ProficiencyPacket packet = new ProficiencyPacket();
            packet.Size = 0x10;
            packet.Type = 0x401;
            PacketBuilder.AppendTQServer(packet.TQServer, 8);
            return packet;
        }
    }
}
