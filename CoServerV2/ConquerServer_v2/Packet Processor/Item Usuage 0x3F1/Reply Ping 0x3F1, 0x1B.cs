using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void ReplyPing(GameClient Client, ItemUsuagePacket* lpPacket)
        {
            // TO-DO:
            // Implement a check to check for ping-time
            // ping-time gets faster when CE is turned on, only downside is if the
            // client hits a lag spike, they're gonna get dced, lol.
            Client.Send(lpPacket);
        }
    }
}