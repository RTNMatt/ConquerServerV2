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
        public static void KickFromTeam(GameClient Client, TeamActionPacket* Packet)
        {
            if (Client.InTeam)
            {
                if (Client.Team.Leader)
                {
                    GameClient Teammate = Client.Team.Search(Packet->UID);
                    if (Teammate != null)
                    {
                        Teammate.Team.LeaveTeam(Packet);
                    }
                }
            }
        }
    }
}
