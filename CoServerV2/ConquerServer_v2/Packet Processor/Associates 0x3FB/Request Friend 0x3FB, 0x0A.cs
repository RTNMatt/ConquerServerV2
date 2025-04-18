using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void RequestFriend(GameClient Client, AssociatePacket* Packet)
        {
            if (Client.Friends.Search(Packet->UID) == null)
            {
                GameClient request = Kernel.FindClientByUID(Packet->UID);
                if (request != null)
                {
                    if (request.PendingFriendUID != Client.Entity.UID)
                    {
                        Client.PendingFriendUID = request.Entity.UID;
                        Packet->UID = Client.Entity.UID;
                        Packet->Name = Client.Entity.Name;
                        request.Send(Packet);
                    }
                    else
                    {
                        if (request.Friends.Search(Client.Entity.UID) == null)
                        {
                            if (request.Friends.Length < 50 && Client.Friends.Length < 50)
                            {
                                /* Need to remake it, because our AssociatePacket has a footer for the username/account */
                                AssociatePacket real = AssociatePacket.Create();
                                real.ID = AssociationID.NewFriend;
                                real.Online = true;

                                real.Name = Client.Entity.Name;
                                real.Account = Client.Account;
                                real.UID = Client.Entity.UID;
                                request.Friends.Add(real);
                                request.Send(&real);

                                real.Name = request.Entity.Name;
                                real.Account = request.Account;
                                real.UID = request.Entity.UID;
                                Client.Friends.Add(real);
                                Client.Send(&real);
                            }
                            else
                            {
                                request.Send(MessageConst.FRIEND_LIST_FULL);
                                Client.Send(MessageConst.FRIEND_LIST_FULL);
                            }
                        }
                    }
                }
            }
        }
    }
}