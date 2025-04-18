using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.GuildWar;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void AppendGuardJump(GameClient Client, DataPacket* Packet)
        {
            if (Client.Pet != null)
            {
                if (!Client.CurrentDMap.Invalid(Packet->dwParam_Lo, Packet->dwParam_Hi))
                {
                    /* Make sure the guard is only jumping close to it's owner */
                    if (Kernel.GetDistance(Client.Entity.X, Client.Entity.Y, Packet->dwParam_Lo, Packet->dwParam_Hi) >= 12)
                    {
                        Packet->dwParam_Lo = Client.Entity.X;
                        Packet->dwParam_Hi = Client.Entity.Y;
                    }
                }
                else
                {
                    Packet->dwParam_Lo = Client.Entity.X;
                    Packet->dwParam_Hi = Client.Entity.Y;
                }

                Client.Pet.Entity.Facing = Kernel.GetFacing(Kernel.GetAngle(Client.Entity.X, Client.Entity.Y, Packet->dwParam_Lo, Packet->dwParam_Hi));
                Client.Pet.Entity.X = Packet->dwParam_Lo;
                Client.Pet.Entity.Y = Packet->dwParam_Hi;

                Packet->ID = DataID.Jump;
                Packet->UID = Client.Pet.Entity.UID;
                SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(Packet), null);
            }
        }
    }
}