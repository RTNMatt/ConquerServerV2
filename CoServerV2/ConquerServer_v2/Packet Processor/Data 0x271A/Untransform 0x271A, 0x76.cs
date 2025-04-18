using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void Untransform(GameClient Client)
        {
            if (Client.InTransformation)
            {
                Client.Transform.Stop();
                Client.Transform.SendUpdates();
            }
        }
    }
}