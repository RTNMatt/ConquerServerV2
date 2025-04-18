using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void EnterPortal(GameClient Client, DataPacket* Packet)
        {
            bool Failed = true;
            if (Client.CurrentDMap != null)
            {
                if (Kernel.GetDistance(Client.Entity.X, Client.Entity.Y, Packet->dwParam_Lo, Packet->dwParam_Hi) <= 5)
                {
                    uint DestMapID;
                    ushort DestX, DestY;
                    if (ServerDatabase.FindPortal(Client.Entity.MapID, Packet->dwParam_Lo, Packet->dwParam_Hi, out DestMapID, out DestX, out DestY))
                    {
                        Client.Teleport(DestMapID, DestX, DestY);
                        Failed = false;
                    }
                }
            }
            if (Failed)
            {
                for (byte i = 0; i < 3; i++)
                    Kernel.IncXY(ConquerAngle.South, ref Client.Entity.Spawn.X, ref Client.Entity.Spawn.Y);
                Client.Pullback();
            }
        }
    }
}