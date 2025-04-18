using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void RequestFriendInfo(GameClient Client, DataPacket* Packet)
        {
            IAssociate friend = Client.Friends.Search(Packet->dwParam1);
            if (friend != null)
            {
                AssociateInfoPacket Info = AssociateInfoPacket.Create();
                GameClient friendClient = Kernel.FindClientByUID(friend.UID);
                if (friendClient != null)
                {
                    Info.UID = friend.UID;
                    Info.Job = friendClient.Job;
                    Info.PKPoints = friendClient.PKPoints;
                    Info.Model = friendClient.Entity.Model;
                    Info.Level = (byte)friendClient.Entity.Level;
                    friendClient.Spouse.CopyTo(Info.Spouse);
                    if ((Info.GuildID = friendClient.Guild.ID) != 0)
                        Client.Send(friendClient.Guild.QueryName());
                }
                Client.Send(&Info);
            }
        }
    }
}