using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void SendAssociates(GameClient Client, DataPacket* Packet)
        {
            Client.Friends.ObtainDatabaseData(true, Client);
            // Client.Enemies.ObtainDatabaseData(false, Client);

            CreateUIDCallback.Add(Client.Entity, Client.Friends.GetOnlineUIDList(), ConquerCallbackKernel.NotifyFriendsImOnline);
            // ^ same shit but with enemies, lol

            Client.Send(Packet);
        }
    }
}
