using System;
using System.Threading;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void RequestJoinGuild(GameClient Client, GuildRequestPacket* Packet)
        {
            GameClient GuildAdmin = Kernel.FindClientByUID(Packet->dwParam);
            if (GuildAdmin != null)
            {
                if (GuildAdmin.Guild.ID != 0)
                {
                    if (GuildAdmin.Guild.Rank == GuildRank.DeputyLeader ||
                        GuildAdmin.Guild.Rank == GuildRank.Leader)
                    {
                        Packet->dwParam = Client.Entity.UID;
                        GuildAdmin.Send(Packet);
                    }
                }
            }
        }
    }
}