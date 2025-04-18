using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void OpenWarehouse(GameClient Client, ItemUsuagePacket* lpPacket)
        {
            Client.ActiveWarehouseID = lpPacket->UID;
            lpPacket->dwParam1 = (uint)new Warehouse(Client.Account, Client.ActiveWarehouseID).ReadGold();
            Client.Send(lpPacket);
        }
    }
}