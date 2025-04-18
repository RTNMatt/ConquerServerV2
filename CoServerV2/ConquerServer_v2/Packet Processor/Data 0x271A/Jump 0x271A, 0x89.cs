using System;
using System.Threading;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.GuildWar;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void AppendJump(GameClient Client, DataPacket* Packet)
        {
            if (!Client.Entity.Dead)
            {
                if (Client.IsVendor)
                    Client.Vendor.StopVending();
                if (Client.IsMining)
                    Client.Mine.Stop();

                if (Client.Entity.Action != ConquerAction.None)
                    ChangeAction(Client, ConquerAction.None, null);

                if (Packet->UID != Client.Entity.UID)
                    return;

                if (!Client.CurrentDMap.Invalid(Packet->dwParam_Lo, Packet->dwParam_Hi))
                {
                    if (Kernel.GetDistance(Client.Entity.X, Client.Entity.Y, Packet->dwParam_Lo, Packet->dwParam_Hi) > Kernel.ViewDistance)
                    {
                        Client.NetworkSocket.Disconnect();
                        return;
                    }
#if PROTECTION_SPEEDHACK
                    else
                    {
                        if (Client.TimeStamps.LastClientJump.Time >= TIME.Parse(Packet->TimeStamp.ToString()).Time)
                        {
                            Client.Send(MessageConst.SPEED_HACK);
                            return;
                        }
                        if ((Client.Entity.StatusFlag & StatusFlag.Cyclone) != StatusFlag.Cyclone &&
                            !Client.InTransformation)
                        {
                            if (TIME.Parse(Packet->TimeStamp.ToString()).Time - Client.TimeStamps.LastClientJump.Time <= 500)
                            {
                                Client.Send(MessageConst.SPEED_HACK);
                                Client.Pullback();
                                return;
                            }
                            else
                            {
                                if (TIME.Now.Time - Client.TimeStamps.LastServerJump.Time <= 300)
                                {
                                    Client.Send(MessageConst.SPEED_HACK);
                                    Client.Pullback();
                                    return;
                                }
                            }
                        }
                        Client.TimeStamps.LastClientJump = TIME.Parse(Packet->TimeStamp.ToString());
                        Client.TimeStamps.LastServerJump = TIME.Now;
                    }
#endif
                    if (Client.Entity.MapID == MapID.GuildWar)
                    {
                        if (!GuildWarKernel.ValidJump(Client.TileColor, out Client.TileColor, Packet->dwParam_Lo, Packet->dwParam_Hi))
                        {
                            Client.Send(MessageConst.WALK_ONLY);
                            Client.Pullback();
                            return;
                        }
                    }


                    Client.TimeStamps.SpawnProtection = TIME.Now;
                    Client.Send(Packet);
                    SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, Client.Entity.UID, Kernel.ToBytes(Packet), null);

                    Client.Entity.Facing = Kernel.GetFacing(Kernel.GetAngle(Client.Entity.X, Client.Entity.Y, Packet->dwParam_Lo, Packet->dwParam_Hi));
                    Client.Entity.X = Packet->dwParam_Lo;
                    Client.Entity.Y = Packet->dwParam_Hi;

                    Kernel.GetScreen(Client, null);
                }
                else
                {
                    Client.Pullback();
                }
            }
        }
    }
}