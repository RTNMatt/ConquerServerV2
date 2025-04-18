using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void LeaveTeam(GameClient Client, TeamActionPacket* Packet)
        {
            if (Client.InTeam)
            {
                Client.Team.LeaveTeam(Packet);
            }
        }
    }
}
