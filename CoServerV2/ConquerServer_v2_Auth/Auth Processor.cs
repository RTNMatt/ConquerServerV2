using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;
using ConquerServer_v2.BruteForce;
using Liberate.Cryptography;

namespace ConquerServer_v2
{
    public partial class Program
    {
        static unsafe void Auth_ClientConnect(NetworkClient Client)
        {
            Client.Owner = new AuthClient(Client);
            if (BruteforceProtection.IsBanned(Client.IP))
                Client.Disconnect();
            else
            {
                PasswordSeed PassSeed = PasswordSeed.Create();
                ((AuthClient)Client.Owner).Send(&PassSeed);
                Console.WriteLine("[COSource-Auth] Client connected.");
            }
        }
        static unsafe void Auth_ClientReceive(NetworkClient nClient, byte[] Received)
        {
            AuthClient Client = nClient.Owner as AuthClient;
            Client.Crypto.Decrypt(Received, Received, Received.Length);

            fixed (byte* pReceived = Received)
            {
                ushort Type = *((ushort*)(pReceived + 2));
                switch (Type)
                {
                    case LoginPacket.cType:
                        {
                            LoginPacket* login = (LoginPacket*)pReceived;
                            if (Received.Length == LoginPacket.cSize)
                            {
                                Client.Account = login->User;

                                msvcrt.msvcrt.srand(123);
                                var rc5Key = new byte[0x10];
                                for (int i = 0; i < 0x10; i++)
                                    rc5Key[i] = (byte)msvcrt.msvcrt.rand();
                                byte[] EncryptedPassword = new byte[16];
                                Buffer.BlockCopy(Received, 132, EncryptedPassword, 0, 16);
                                Client.Password =  Encoding.ASCII.GetString(
                                              (new ConquerPasswordCryptpographer(Client.Account).Decrypt(
                                                  (new RC5(rc5Key)).Decrypt(EncryptedPassword)))).Trim((char)0x0000);


                                Client.AuthID = ServerDatabase.ValidAccount(Client.Account, Client.Password);
                                int PermanentBan = ServerDatabase.PermanentBan(Client.Account);
                                AuthResponsePacket resp = AuthResponsePacket.Create();

                                if (PermanentBan == 2 && pReceived[131] == 0xFF)
                                {
                                    ServerDatabase.AddFullPermanentBan(Client.Account);
                                }
                                else if (PermanentBan == 4)
                                {
                                    ServerDatabase.RemovePermanentBan(Client.Account);
                                }

                                if (Client.AuthID != 0)
                                {
                                    if (PermanentBan == 2)
                                        resp.Type = 0x41E;
                                    else if (PermanentBan == 4)
                                    {
                                        resp.Type = 0x41D;
                                    }
                                    resp.IPAddress = "192.168.1.7";
                                    resp.Key1 = Client.AuthID;

                                    System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
                                    byte[] bs = System.Text.Encoding.UTF8.GetBytes(Client.Password);
                                    bs = x.ComputeHash(bs);
                                    System.Text.StringBuilder s = new System.Text.StringBuilder();
                                    foreach (byte b in bs)
                                    {
                                        s.Append(b.ToString("x2").ToLower());
                                    }
                                   string newpassword = s.ToString().Remove(8);
                                   string newnewnumber = "";

                                   byte[] data = new byte[newpassword.Length * 2];
                                   for (int i = 0; i < 2; ++i)
                                   {
                                       char ch = newpassword[i];
                                       data[i * 2] = (byte)(ch & 0xFF);
                                       data[i * 2 + 1] = (byte)((ch & 0xFF00) >> 8);
                                       string number = data[i * 2].ToString() + data[i * 2 + 1].ToString();
                                       newnewnumber = newnewnumber + number;
                                   }

                                   

                                    resp.Key2 = int.Parse(newnewnumber);
                                    resp.Port = 5817;
                                    ServerDatabase.AddAuthData(Client);
                                    ServerDatabase.AddLastLogin(Client.Account);
                                }
                                else
                                {
                                    resp.Key1 = 1;
                                    BruteforceProtection.AddWatch(nClient.IP);
                                }
                                Client.Send(&resp);
                            }
                            else
                            {
                                nClient.Disconnect();
                            }
                            break;
                        }
                }
            }
        }
    }
}
