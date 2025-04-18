using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void RemoveFriend(GameClient Client, AssociatePacket* Packet)
        {
            int FriendIdx, ClientIdx;
            IAssociate ClientAssociate;
            IAssociate friendAssociate = Client.Friends.Search(Packet->UID, out FriendIdx);

            if (friendAssociate != null)
            {
                GameClient friend = Kernel.FindClientByUID(Packet->UID);
                if (friend == null)
                {
                    string friendAccount = friendAssociate.Account;
                    FlexibleArray<IAssociate> tempFriends = new FlexibleArray<IAssociate>();
                    FlexibleArray<IAssociate> tempEnemies = new FlexibleArray<IAssociate>();
                    ServerDatabase.LoadAssociates(friendAccount, ref tempFriends, ref tempEnemies);
                    for (int i = 0; i < tempFriends.Length; i++)
                    {
                        if (tempFriends.Elements[i].Account == Client.Account)
                        {
                            tempFriends.Remove(i);
                            ServerDatabase.SaveAssociates(friendAccount, ref tempFriends, ref tempEnemies);
                            break;
                        }
                    }
                }
                else
                {
                    ClientAssociate = friend.Friends.Search(Client.Entity.UID, out ClientIdx);
                    if (ClientAssociate != null)
                    {
                        friend.Friends.Remove(ClientIdx);
                        Packet->UID = Client.Entity.UID;
                        friend.Send(Packet);
                        Packet->UID = friend.Entity.UID;
                    }
                }
                Client.Friends.Remove(FriendIdx);
            }
            Client.Send(Packet);
        }
    }
}