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
        public static void RequestJoinTeam(GameClient Client, TeamActionPacket* Packet)
        {
            if (!Client.InTeam)
            {
                GameClient Leader = Kernel.FindClientByUID(Packet->UID);
                if (Leader != null)
                {
                    if (Leader.InTeam)
                    {
                        if (Leader.Team.Leader && !Leader.Team.Full)
                        {
                            Packet->UID = Client.Entity.UID;
                            Leader.Send(Packet);
                        }
                        else
                        {
                            Client.Send(MessageConst.TEAM_FULL);
                        }
                    }
                }
            }
            else
            {
                Client.Send(MessageConst.ALREADY_IN_TEAM);
            }
        }
    }
}
