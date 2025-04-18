using System;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void GuildMemberList(GameClient Client, StringPacket Packet)
        {
            if (Client.Guild.ID != 0)
            {
                Client.Send(Client.Guild.QueryMemberList((int)Packet.UID));
            }
        }
    }
}