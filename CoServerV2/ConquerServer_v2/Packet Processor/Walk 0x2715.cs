using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.GuildWar;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void Walk(GameClient Client, MovementPacket* Packet)
        {
            bool petUID = false;
            if (Client.Pet != null)
                petUID = (Packet->UID == Client.Pet.Entity.UID);
            if (!petUID && Packet->UID != Client.Entity.UID)
                return;

            ushort walkX, walkY;
            ConquerAngle dir = (ConquerAngle)(Packet->Direction % 8);
            if (!petUID)
            {
                if (Client.IsVendor)
                    Client.Vendor.StopVending();
                if (Client.IsMining)
                    Client.Mine.Stop();
                Client.Entity.Facing = dir;
                if (Client.Entity.Action != ConquerAction.None)
                    ChangeAction(Client, ConquerAction.None, null);

                walkX = Client.Entity.X;
                walkY = Client.Entity.Y;
            }
            else
            {
                Client.Pet.Entity.Facing = dir;
                walkX = Client.Pet.Entity.X;
                walkY = Client.Pet.Entity.Y;
            }

            Kernel.IncXY(dir, ref walkX, ref walkY);
            if (Client.CurrentDMap != null)
            {
                if (!Client.CurrentDMap.Invalid(walkX, walkY))
                {
                    if (!petUID)
                    {
                        if (Client.Entity.MapID == MapID.GuildWar)
                        {
                            if (!GuildWarKernel.ValidWalk(Client.TileColor, out Client.TileColor, Client.Entity.X, Client.Entity.Y))
                            {
                                Client.Send(MessageConst.NO);
                                Client.Pullback();
                                return;
                            }
                        }

#if PROTECTION_SPEEDHACK
                        if (Client.TimeStamps.LastClientWalk.Time >= Packet->TimeStamp.Time)
                        {
                            Client.Send(MessageConst.SPEED_HACK);
                            Client.Teleport(Client.Entity.MapID, Client.Entity.X, Client.Entity.Y);
                            return;
                        }
                        if ((Client.Entity.StatusFlag & StatusFlag.Cyclone) != StatusFlag.Cyclone)
                        {
                            if (Packet->TimeStamp.Time - Client.TimeStamps.LastClientJump.Time <= 200)
                            {
                                Client.Send(MessageConst.SPEED_HACK);
                                return;
                            }
                        }
                        Client.TimeStamps.LastClientWalk = Packet->TimeStamp;
#endif
                    }
                }
                else
                {
                    if (!petUID)
                        Client.Pullback();
                    return;
                }
            }

            if (!petUID)
            {
                Client.Entity.X = walkX;
                Client.Entity.Y = walkY;

                Client.Send(Packet);
                SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, Client.Entity.UID, Kernel.ToBytes(Packet), null);

                Kernel.GetScreen(Client, null);
            }
            else
            {
                Client.Pet.Entity.X = walkX;
                Client.Pet.Entity.Y = walkY;
                SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, Client.Entity.UID, Kernel.ToBytes(Packet), null);
            }
        }
    }
}