using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void UnequipItem(GameClient Client, ItemUsuagePacket* lpPacket)
        {
            if (!Client.Entity.Dead)
            {
                ItemPosition position = (ItemPosition)lpPacket->dwParam1;
                if ((Client.Entity.StatusFlag & StatusFlag.Fly) == StatusFlag.Fly)
                    if (position == ItemPosition.Left || position == ItemPosition.Right)
                        return; // Disable unequiping weapons while flying.
                if (Client.Unequip(position, true))
                {
                    Client.DisplayStats();
                }
            }
        }
    }
}