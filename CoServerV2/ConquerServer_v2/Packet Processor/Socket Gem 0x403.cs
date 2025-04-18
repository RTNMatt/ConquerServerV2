using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void SocketGem(GameClient Client, GemSocketPacket* Packet)
        {
            if (ServerDatabase.NpcDistanceCheck(35, Client.CurrentDMap, Client.Entity.X, Client.Entity.Y))
            {
                bool socketcheck = false;
                Item TargetItem = Client.Inventory.Search(Packet->TargetItemUID);
                if (TargetItem != null)
                {
                    if (Packet->GemItemUID != 0)
                    {
                        byte GemSlot;
                        Item GemItem = Client.Inventory.Search(Packet->GemItemUID, out GemSlot);
                        if (GemItem != null)
                        {
                            if (GemItem.IsItemType(ItemTypeConst.GemID))
                            {
                                if (Packet->SocketNumber == 1)
                                    socketcheck = (TargetItem.SocketOne == GemsConst.OpenSocket);
                                else if (Packet->SocketNumber == 2)
                                    socketcheck = (TargetItem.SocketTwo == GemsConst.OpenSocket);
                                if (socketcheck)
                                {
                                    Client.Inventory.RemoveBySlot(GemSlot);
                                    if (Packet->SocketNumber == 2)
                                        TargetItem.SocketTwo = (byte)(GemItem.ID - 700000);
                                    else
                                        TargetItem.SocketOne = (byte)(GemItem.ID - 700000);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Packet->SocketNumber == 1)
                            socketcheck = (TargetItem.SocketOne != GemsConst.NoSocket);
                        else if (Packet->SocketNumber == 2)
                            socketcheck = (TargetItem.SocketTwo != GemsConst.NoSocket);
                        if (socketcheck)
                        {
                            if (Packet->SocketNumber == 2)
                                TargetItem.SocketTwo = GemsConst.OpenSocket;
                            else
                                TargetItem.SocketOne = GemsConst.OpenSocket;
                        }
                    }

                    if (socketcheck)
                    {
                        TargetItem.SendInventoryUpdate(Client);
                    }
                }
            }
        }
    }
}