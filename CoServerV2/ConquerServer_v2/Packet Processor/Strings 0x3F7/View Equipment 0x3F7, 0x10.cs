using System;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void ViewEquipment(GameClient Client, StringPacket Packet)
        {
            GameClient viewClient = Kernel.FindClientByUID(Packet.UID);
            if (viewClient != null)
            {
                for (ItemPosition p = Item.FirstSlot; p <= Item.LastSlot; p++)
                {
                    if (viewClient.Equipment[p] != null)
                    {
                        ItemPacket temp = viewClient.Equipment[p].Data;
                        temp.Mode = ItemMode.View;
                        temp.UID = viewClient.Entity.UID;
                        Client.Send(&temp);
                    }
                }
                Packet.Strings = new string[1];
                Packet.Strings[0] = viewClient.Spouse;
                Packet.StringsLength = (byte)viewClient.Spouse.Length;
                Packet.UID = viewClient.Entity.UID;
                Client.Send(Packet);
            }
        }
    }
}