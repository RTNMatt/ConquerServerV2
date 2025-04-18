using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void RequestEntity(GameClient Client, DataPacket* lpPacket)
        {
            IBaseEntity Base = Kernel.FindEntity(lpPacket->dwParam1, Client.CurrentDMap);
            if (Base != null)
            {
                if (Base.EntityFlag == EntityFlag.Player || Base.EntityFlag == EntityFlag.Monster)
                {
                    CommonEntity common = Base as CommonEntity;
                    if (common != null)
                    {
                        if (Kernel.GetDistance(Client.Entity.X, Client.Entity.Y, Base.X, Base.Y) < Kernel.ViewDistance)
                        {
                            Client.Screen.Add(common);
                            fixed (SpawnEntityPacket* spawn = &common.Spawn)
                                Client.Send(spawn);
                            if (common.GuildID != 0)
                            {
                                Client.Send((common.Owner as GameClient).Guild.QueryName());
                            }
                        }
                    }
                }
            }
        }
    }
}