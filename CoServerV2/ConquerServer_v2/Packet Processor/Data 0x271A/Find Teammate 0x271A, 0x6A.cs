using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void RequestTeamMember(GameClient Client, DataPacket* Packet)
        {
            if (Client.InTeam)
            {
                GameClient Target = Client.Team.Search(Packet->dwParam1);
                if (Target != null)
                {
                    if (Target.Entity.MapID.Id == Client.Entity.MapID.Id)
                    {
                        Packet->dwParam1 = 0;
                        Packet->wParam1 = Target.Entity.X;
                        Packet->wParam2 = Target.Entity.Y;
                        Client.Send(Packet);
                    }
                }
            }
        }
    }
}