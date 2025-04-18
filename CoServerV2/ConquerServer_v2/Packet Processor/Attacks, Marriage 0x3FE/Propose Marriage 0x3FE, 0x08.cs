using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;
using ConquerServer_v2.Attack_Processor;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void ProposeMarriage(GameClient Client, RequestAttackPacket* Packet)
        {
            GameClient iClient = Kernel.FindClientByUID(Packet->OpponentUID);
            if (iClient != null)
            {
                if (iClient.Spouse == "None" && Client.Spouse == "None")
                {
                    Packet->OpponentUID = Client.Entity.UID;
                    iClient.Send(Packet);
                }
                else
                {
                    Client.Send(new MessagePacket(iClient.Entity.Name + " is / you are already married.", 0x00FF0000, ChatID.TopLeft));
                }
            }
        }
    }
}