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
        public static void AcceptJoinTeam(GameClient Client, TeamActionPacket* Packet)
        {
            if (Client.InTeam)
            {
                if (Client.Team.Leader && !Client.Team.Full)
                {
                    GameClient Joiner = Kernel.FindClientByUID(Packet->UID);
                    if (Joiner != null)
                    {
                        if (!Joiner.InTeam)
                        {
                            Joiner.Team = new Team(Joiner, false, null);
                            Joiner.Team.JoinTeam(Client.Team);
                        }
                    }
                }
            }
        }
    }
}
