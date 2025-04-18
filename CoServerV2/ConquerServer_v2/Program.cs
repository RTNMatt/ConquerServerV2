using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;
using ConquerServer_v2.Core;
using ConquerServer_v2.Multithreading;
using ConquerServer_v2.Monster_AI;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Processor;
using ConquerServer_v2.GuildWar;

namespace ConquerServer_v2
{
    public partial class Program
    {
        static NetworkServerSocket GameServer;

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Kernel.NotifyDebugMsg("[Global Exception]", e.ExceptionObject.ToString(), true);
            ProcessConsoleCommand("/quit");
        }

        unsafe static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // TO-DO:
            // Add shit to make the console look cooler.
            Console.Title = "Conquer Server - Game";
            GameServer = new NetworkServerSocket();
            GameServer.ClientBufferSize = 4000;
            GameServer.OnConnect = new NetworkClientConnection(Game_Connect);
            GameServer.OnReceive = new NetworkClientReceive(Game_ReceivePacket);
            GameServer.OnDisconnect = new NetworkClientConnection(Game_Disconnect);

            Console.WriteLine("Loading... Please wait...");
            ServerDatabase.Init();
            //
            PlusItemStats p1 = new PlusItemStats(900109, 6);
            PlusItemStats p2 = new PlusItemStats(900109, 5);
            PlusItemStats p3 = new PlusItemStats(900109, 7);
            //
            ConquerScriptEngine.Init();
            GuildWarKernel.Init();
            TimerThreads.Start();
            Console.WriteLine("Finished loading.");

            GameServer.Prepare(5817, 100);
            GameServer.BeginAccept();

            while (true)
            {
                if (!ProcessConsoleCommand(Console.ReadLine()))
                    break;
            }
        }

        public static bool ProcessConsoleCommand(string Command)
        {
            try
            {
                string[] Commands = Command.Split(' ');
                Commands[0] = Commands[0].ToLower();
                switch (Commands[0])
                {
                    case "/help":
                        {
                            Console.WriteLine("/debug - Monitor thread usuage.");
                            Console.WriteLine("/message - Sent a global message.");
                            Console.WriteLine("/playercount - A count of the players online.");
                            Console.WriteLine("/gc - Force a garbage collection.");
                            Console.WriteLine("/quit - Clean up resources, save characters, close.");
                            Console.WriteLine("/clientratio - Provides a ratio between clients to players online");
                            Console.WriteLine("/restartqueues - Restarts the SRP & Attack threads.");
                            break;
                        }
                    case "/packetspeedlog":
                        {
                            const string pfile = @"C:\packetspeed.log";
                            using (StreamWriter writer = new StreamWriter(pfile, false))
                            {
                                foreach (GameClient g in Kernel.Clients)
                                {
                                    writer.WriteLine(g.Account + " ~ " + g.PacketSpeed);
                                }
                            }
                            break;
                        }
                    case "/srpclr":
                        {
                            SendRangePacket.Queue.Clear();
                            break;
                        }
                    case "/restartqueues": 
                        {
                            Console.Write("Restarting... ");
                            if (SendRangePacket.Queue != null)
                            {
                                SendRangePacket.Queue.Stop();
                                SendRangePacket.Queue.Start(ThreadPriority.AboveNormal);
                            }
                            AttackSystem.Physical.Stop();
                            AttackSystem.Physical.Start(ThreadPriority.AboveNormal);
                            AttackSystem.Magical.Stop();
                            AttackSystem.Magical.Start(ThreadPriority.AboveNormal);
                            Console.WriteLine("Done");
                            break;
                        }
                    case "/debug":
                        {
                            string[] debugStrings = new string[]
                                    {
                                        "Creation Events: " + CreationThread.PendingThreads.ToString(),
                                        "SRP Active Threads: " + SendRangePacket.PendingThreads.ToString(),
                                        "SGP Active Threads: " + SendGlobalPacket.PendingThreads.ToString(),
                                        "Attack Events: " + AttackSystem.PendingThreads.ToString(),
                                        "Script Threads: " + ExecuteScriptThread.PendingThreads.ToString(),
                                        "Monster AI: " + MonsterSpawn.PendingThreads.ToString(),
                                    };
                            foreach (string DebugString in debugStrings)
                                Console.WriteLine(DebugString);
                            break;
                        }
                    case "/message":
                        {
                            string globalMsg = Command.Remove(0, 9);
                            if (globalMsg.Length > 255)
                            {
                                globalMsg = globalMsg.Substring(0, 254);
                                Console.WriteLine("Message Truncated; \r\n" + globalMsg);
                            }
                            SendGlobalPacket.Add(new MessagePacket(globalMsg, 0x00FFFFFF, ChatID.Center));  
                            break;
                        }
                    case "/playercount":
                        {
                            Console.WriteLine("{0} Players are online.", Kernel.Clients.Length);
                            break;
                        }
                    case "/gc":
                        {
                            Console.Write("Collecting... ");
                            GC.Collect();
                            Console.WriteLine("Done.");
                            break;
                        }
                    case "/clientratio":
                        {
                            Console.WriteLine("{0} Clients in existance, {1} Clients online.", GameClient.ClientInstances, Kernel.Clients.Length);
                            break;
                        }
                    case "/quit":
                        {
                            Console.Write("Saving players... ");
                            foreach (GameClient Client in Kernel.Clients)
                            {
                                if ((Client.ServerFlags & ServerFlags.LoadedCharacter) == ServerFlags.LoadedCharacter)
                                    ServerDatabase.SavePlayer(Client);
                            }
                            Console.WriteLine("Done.");
                            return false;
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            return true;
        }
    }
}