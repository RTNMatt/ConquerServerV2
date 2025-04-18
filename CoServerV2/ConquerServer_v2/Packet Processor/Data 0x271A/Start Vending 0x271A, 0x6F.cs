using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void StartVending(GameClient Client, DataPacket* Packet)
        {
            if (!Client.IsVendor && Client.Entity.MapID == MapID.Market)
            {
                Client.Vendor = new ClientVendor(Client);
                if (Client.Vendor.StartVending())
                {
                    Packet->dwParam1 = Client.Vendor.ShopID;
                    Client.Send(Packet);
                }
            }
        }
    }
}