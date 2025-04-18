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
        public static void SendSpells(GameClient Client, DataPacket* Packet)
        {
            for (int i = 0; i < Client.Spells.Length; i++)
            {
                Client.Spells.Elements[i].Send(Client);
            }
            Client.Send(Packet);
        }
    }
}
