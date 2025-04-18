using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void DismissTeam(GameClient Client, TeamActionPacket* Packet)
        {
            if (Client.InTeam)
            {
                if (Client.Team.Leader)
                {
                    Client.Team.DismissTeam(Packet);
                }
            }
        }
    }
}
