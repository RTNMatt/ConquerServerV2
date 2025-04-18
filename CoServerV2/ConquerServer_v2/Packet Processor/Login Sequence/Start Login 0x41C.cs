using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void LoginStart(GameClient Client, byte* Ptr)
        {
            Client.Entity.UID = *((uint*)(Ptr + 8));
            int PasswordCheckSum = *((int*)(Ptr + 4));


            bool New;
            GameClient existingClient = Kernel.FindClientByUID(Client.Entity.UID);
            if (existingClient != null)
            {
                existingClient.NetworkSocket.Disconnect();
                Program.Game_Disconnect(existingClient.NetworkSocket);
            }
            if (ServerDatabase.LoadPlayer(Client, PasswordCheckSum, out New))
            {
                if (Client.BannedFlag == 2 || Client.BannedFlag == 3) // Permanent
                {
                    Client.BannedFlag = 3;
                    Client.Send(MessageConst.ANSWER_NO);
                    return;
                }
                else if (Client.BannedFlag == 1) // Character
                {
                    Client.Send(MessageConst.ANSWER_NO);
                    return;
                }

                Kernel.ClientDictionary.Override(Client.Entity.UID, Client);
                ServerDatabase.IncPlayerOnline();
                DateTimePacket date = DateTimePacket.Create();
                
                Client.Send(MessageConst.ANSWER_OK);
                Client.Send(new CharacterInfoPacket(Client));
                Client.Send(&date);
                Client.TimeStamps.SpawnProtection = TIME.Now.AddSeconds(20);
            }
            else if (New)
            {
                Client.Send(MessageConst.NEW_ROLE);
            }
        }
    }
}
