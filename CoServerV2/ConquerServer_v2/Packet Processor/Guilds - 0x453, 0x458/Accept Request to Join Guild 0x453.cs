using System;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void AcceptJoinGuild(GameClient Client, GuildRequestPacket* Packet)
        {
            if (Client.Guild.ID != 0)
            {
                if (Client.Guild.Rank == GuildRank.DeputyLeader || Client.Guild.Rank == GuildRank.Leader)
                {
                    GameClient Invitee = Kernel.FindClientByUID(Packet->dwParam);
                    if (Invitee != null)
                    {
                        if (Invitee.Guild.ID == 0)
                        {
                            Invitee.Guild.Join(Client.Guild.ID);
                        }
                    }
                }
            }
        }
    }
}