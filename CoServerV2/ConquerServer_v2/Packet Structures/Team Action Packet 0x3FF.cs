using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public enum TeamActionID : uint
    {
        Create = 0x00,
        RequestJoin = 0x01,
        LeaveTeam = 0x02,
        AcceptInvite = 0x03,
        RequestInvite = 0x04,
        AcceptJoin = 0x05,
        Dismiss = 0x06,
        Kick = 0x07
    }

    public unsafe struct TeamActionPacket
    {
        public ushort Size;
        public ushort Type;
        public TeamActionID ID;
        public uint UID;
        private fixed byte TQServer[8];
        
        public static TeamActionPacket Create()
        {
            TeamActionPacket retn = new TeamActionPacket();
            retn.Size = 0x0C;
            retn.Type = 0x3FF;
            PacketBuilder.AppendTQServer(retn.TQServer, 8);
            return retn;
        }
    }
}
