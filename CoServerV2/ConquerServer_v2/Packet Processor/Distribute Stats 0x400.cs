using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void DistributeStatPoints(GameClient Client, DistributeStatPacket* Packet)
        {
            if (Client.Stats.StatPoints > 0)
            {
                if (Packet->Strength)
                    Client.Stats.Strength++;
                else if (Packet->Agility)
                    Client.Stats.Agility++;
                else if (Packet->Spirit)
                    Client.Stats.Spirit++;
                else if (Packet->Vitality)
                    Client.Stats.Vitality++;

                BigUpdatePacket Big = new BigUpdatePacket(5);
                Big.AllStats(Client, 0);
                Client.Send(Big);
            }
        }
    }
}