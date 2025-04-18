using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using ConquerServer_v2.Database;
using System.IO;

namespace ConquerServer_v2
{
    public partial class Program
    {
        static NetworkServerSocket AuthServer;
        static void Main(string[] args)
        {
            // TO-DO:
            // Add shit to make the console look cooler.
            Console.Title = "Conquer Server - Auth";

            BruteForce.BruteforceProtection.Init(7);

            AuthServer = new NetworkServerSocket();
            AuthServer.ClientBufferSize = 1000;
            AuthServer.OnConnect = new NetworkClientConnection(Auth_ClientConnect);
            AuthServer.OnReceive = new NetworkClientReceive(Auth_ClientReceive);

            AuthServer.Prepare(9960, 100);
            AuthServer.BeginAccept();

            const int RestartMin = 10;
            TIME RestartSQL = TIME.Now.AddMinutes(RestartMin);
            string SQLPath = @"C:\Server\ConquerServer_v2_Website\bin\Debug\ConquerServer_v2_Website.exe";

            while (true)
            {
                if (File.Exists(SQLPath))
                {
                    if (TIME.Now.Time >= RestartSQL.Time)
                    {
                        try { KillWebsiteServerProcess(); }
                        catch { }
                        Process.Start(SQLPath);

                        RestartSQL = TIME.Now.AddMinutes(RestartMin);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        static void KillWebsiteServerProcess()
        {
            int count = 0;
            foreach (Process p in Process.GetProcesses())
            {
                string pToString = p.ToString();
                if (pToString.Contains("cmd"))
                {
                    if (p.MainWindowTitle.Contains("Conquer Server - Website"))
                    {
                        p.Kill();
                        count++;
                    }
                }
                else if (pToString.Contains("ConquerServer_v2_Website"))
                {
                    p.Kill();
                    count++;
                }
                if (count == 2)
                    break;
            }
        }
    }
}
