using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void StartMine(GameClient Client, DataPacket* lpPacket)
        {
            if (!Client.IsMining)
            {
                Client.Mine = new PlayerMiner();
                if (!Client.Mine.Start(Client))
                {
                    Client.Mine.Stop();
                }
            }
        }
    }
}