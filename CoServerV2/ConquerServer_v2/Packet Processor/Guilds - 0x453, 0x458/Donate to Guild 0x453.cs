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
        public static void DonateToGuild(GameClient Client, GuildRequestPacket* Packet)
        {
            if (Client.Guild.ID != 0)
                Client.Guild.Donate((int)Packet->dwParam);
        }
    }
}