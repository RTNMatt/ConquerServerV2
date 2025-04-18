using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void ChangeAvatar(GameClient Client, DataPacket* lpPacket)
        {
            if (Client.Money >= 500)
            {
                Client.Money -= 500;
                UpdatePacket update = UpdatePacket.Create();
                update.ID = UpdateID.Money;
                update.UID = Client.Entity.UID;
                update.Value = (uint)Client.Money;
                Client.Send(&update);

                ushort avatar = (ushort)lpPacket->dwParam1;
                byte gender = (byte)(Client.Entity.Mesh / 1000);
                if ((gender == 1 && avatar < 201) || 
                    (gender == 2 && (avatar > 200 && avatar < 400)))
                {
                    Client.Entity.Avatar = avatar;
                    update.ID = UpdateID.Model;
                    update.Value = Client.Entity.Model;
                    SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&update), null);
                }
            }
        }
    }
}