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
        public static void InviteJoinTeam(GameClient Client, TeamActionPacket* Packet)
        {
            if (Client.InTeam)
            {
                if (Client.Team.Leader && !Client.Team.Full)
                {
                    GameClient Receiver = Kernel.FindClientByUID(Packet->UID);
                    if (Receiver != null)
                    {
                        if (!Receiver.InTeam)
                        {
                            Packet->UID = Client.Entity.UID;
                            Receiver.Send(Packet);
                        }
                        else
                        {
                            Client.Send(MessageConst.ALREADY_IN_TEAM2);
                        }
                    }
                }
                else
                {
                    Client.Send(MessageConst.TEAM_FULL);
                }
            }
        }
    }
}
