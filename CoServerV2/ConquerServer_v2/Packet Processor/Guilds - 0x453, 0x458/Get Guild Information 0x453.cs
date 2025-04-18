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
        public static void GetGuildInfo(GameClient Client)
        {
            GuildInfoPacket Info = GuildInfoPacket.Create();
            Client.Guild.QueryInfo(&Info);
            Client.Send(&Info);
            byte[] Bulletin = Client.Guild.QueryBulletin(null);
            if (Bulletin != null)
                Client.Send(Bulletin);
        }
    }
}