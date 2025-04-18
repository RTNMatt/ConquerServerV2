using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;
using ConquerServer_v2.Attack_Processor;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void AcceptMarriage(GameClient Client, RequestAttackPacket* Packet)
        {
            GameClient iClient = Kernel.FindClientByUID(Packet->OpponentUID);
            if (iClient != null)
            {
                if (iClient.Spouse == "None" && Client.Spouse == "None")
                {
                    IniFile ini = new IniFile(ServerDatabase.Path + "\\Accounts\\" + Client.Account + ".ini");
                    ini.WriteString("Character", "Spouse", iClient.Account);
                    ini.FileName = ServerDatabase.Path + "\\Accounts\\" + iClient.Account + ".ini";
                    ini.WriteString("Character", "Spouse", Client.Account);

                    iClient.Spouse = Client.Entity.Name;
                    iClient.SpouseAccount = Client.Account;
                    Client.Spouse = iClient.Entity.Name;
                    Client.SpouseAccount = iClient.Account;

                    StringPacket Marriage = new StringPacket();
                    Marriage.ID = StringID.Spouse;

                    Marriage.UID = iClient.Entity.UID;
                    Marriage.Strings = new string[] { Client.Entity.Name };
                    Marriage.StringsLength = (byte)Marriage.Strings[0].Length;
                    iClient.Send(Marriage);

                    Marriage.UID = Client.Entity.UID;
                    Marriage.Strings = new string[] { iClient.Entity.Name };
                    Marriage.StringsLength = (byte)Marriage.Strings[0].Length;
                    Client.Send(Marriage);

                    SendGlobalPacket.Add(new MessagePacket("Congratulations, " + Client.Entity.Name + " and  " +
                        Client.Spouse + " have been united in holy matrimony!", 0x00FF0000, ChatID.Center));
                }
            }
        }
    }
}