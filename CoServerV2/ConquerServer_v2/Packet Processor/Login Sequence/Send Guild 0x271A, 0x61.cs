using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void SendGuild(GameClient Client, DataPacket* Packet)
        {
            GuildInfoPacket Info = GuildInfoPacket.Create();
            Client.Guild.QueryInfo(&Info);
            Client.Send(&Info);
            if (Client.Guild.ID != 0)
            {
                Client.Send(Client.Guild.QueryName());
                Client.Guild.SendAllies();
                Client.Guild.SendEnemies();
                byte[] Bulletin = Client.Guild.QueryBulletin(null);
                if (Bulletin != null)
                    Client.Send(Bulletin);
            }
            Client.Send(Packet);
        }
    }
}
