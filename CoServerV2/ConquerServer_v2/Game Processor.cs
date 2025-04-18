using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Processor;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;
using ConquerServer_v2.Core;
using ConquerServer_v2.Monster_AI;

namespace ConquerServer_v2
{
    public partial class Program
    {
        // Called when a new network client connects to the server.
        static void Game_Connect(NetworkClient nClient)
        {
            GameClient Client = new GameClient(nClient); // Wrap the raw connection in a GameClient object
            nClient.Owner = Client;                     // Link back for access to game logic
            Client.Send(Client.DHKeyExchange.CreateServerKeyPacket()); // Send server's part of the key exchange
        }

        // Called when a client disconnects from the server
        public unsafe static void Game_Disconnect(NetworkClient nClient)
        {
            if (nClient.Owner != null)
            {
                GameClient Client = nClient.Owner as GameClient;
                nClient.Owner = null;

                // If the client hasn't already been flagged as logged out
                if ((Client.ServerFlags & ServerFlags.LoggedOut) != ServerFlags.LoggedOut)
                {
                    Client.ServerFlags |= ServerFlags.LoggedOut;    // Mark as logged out
                    Client.ServerFlags &= ~ServerFlags.LoggedIn;    // Clear logged in flag

                    // Create a packet to tell others this entity should be removed from their screen
                    DataPacket unspawn = DataPacket.Create();
                    unspawn.ID = DataID.RemoveEntity;
                    unspawn.UID = Client.Entity.UID;
                    SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, Client.Entity.UID, Kernel.ToBytes(&unspawn), ConquerCallbackKernel.CommonRemoveScreen);

                    // Handle team-related cleanup
                    if (Client.InTeam)
                    {
                        if (Client.Team.Leader)
                            Client.Team.DismissTeam(null);
                        else
                            Client.Team.LeaveTeam(null);
                    }
                    // Cleanup vending, trades, pets, etc.
                    if (Client.IsVendor)
                        Client.Vendor.StopVending();
                    if (Client.InTrade)
                        Client.Trade.CloseTrade(null);
                    if (Client.Pet != null)
                        Client.Pet.Kill(null, TIME.Now);

                    Client.AutoAttackEntity = null;

                    // Notify friends that this client went offline
                    CreateUIDCallback.Add(Client.Entity, Client.Friends.GetOnlineUIDList(), ConquerCallbackKernel.NotifyFriendsImOffline);
                    // same shit for enemies ^

                    Client.Screen.FullWipe();   // Clear the client’s visible screen
                    Kernel.ClientDictionary.Remove(Client.Entity.UID);  // Remove from global player list

                    // Save the character if their data was fully loaded
                    if ((Client.ServerFlags & ServerFlags.LoadedCharacter) == ServerFlags.LoadedCharacter)
                    {
                        ServerDatabase.SavePlayer(Client);
                        ServerDatabase.DecPlayerOnline();
                    }
                }
            }
        }

        // Handles incoming packets from clients
        static unsafe void Game_ReceivePacket(NetworkClient nClient, byte[] Packet)
        {
            if (nClient.Owner != null)
            {
                GameClient Client = nClient.Owner as GameClient;
                Client.Crypto.Decrypt(Packet);  // Decrypt packet with Blowfish or another scheme

                if (Packet.Length <= 4)
                {
                    Client.NetworkSocket.Disconnect();  // Malformed or too short: force disconnect
                    return;
                }

                // If the secure key exchange is done
                if ((Client.ServerFlags & ServerFlags.DHExchanged) == ServerFlags.DHExchanged)
                {
                    fixed (byte* lpPacket = Packet)
                    {
                        int Counter = 0;
                        byte[] InitialPacket = null;
                        while (Counter < Packet.Length)
                        {
                            ushort Size = (ushort)(*((ushort*)(lpPacket + Counter)) + 8);   // Packet size
                            ushort Type = *((ushort*)(lpPacket + Counter + 2));             // Packet type

                            // If the packet is a valid size within the buffer
                            if (Size < Packet.Length)
                            {
                                InitialPacket = new byte[Size];
                                fixed (byte* lpInitialPacket = InitialPacket)
                                {
                                    MSVCRT.memcpy(lpInitialPacket, lpPacket + Counter, Size);               // Copy part of packet
                                    PacketBuilder.AppendTQServer(lpInitialPacket, InitialPacket.Length);    // Add headers
                                    PacketProcessor.Process(Client, lpInitialPacket, InitialPacket, Type);  // Handle it
                                }
                            }
                            else if (Size > Packet.Length)
                            {
                                nClient.Disconnect();   
                                break;
                            }
                            else
                            {
                                PacketBuilder.AppendTQServer(lpPacket, Packet.Length);  // Exact match
                                PacketProcessor.Process(Client, lpPacket, Packet, Type);
                            }
#if LOG_PACKETS
                        // Optional packet logging for debugging
                        bool OKDump = true;
                        if (Type == 0x3f1)  // Ping packets – skip logging
                            {
                            if (((ItemUsuagePacket*)lpPacket)->ID == ItemUsuageID.Ping)
                                OKDump = false;
                        }
                        if (OKDump)
                            Console.WriteLine("Client:\r\n" + Dump((InitialPacket == null) ? Packet : InitialPacket));
#endif
                            Counter += Size;    // Move to next packet chunk in the stream
                        }
                    }
                }
                else
                {
                    // If the secure handshake hasn’t completed yet, use Blowfish
                    PacketProcessor.AppendBlowfishLanguage(Client, Packet);
                }
            }
        }
    }
}