using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using System.IO;

namespace ConquerServer_v2.Database
{
    public unsafe class ServerDatabase
    {
        private static string m_Path;
        private static string m_Startup;
        public static string Path { get { return m_Path; } }
        public static string Startup { get { return m_Startup; } }

        private static IniFile AuthSystem;

        static ServerDatabase()
        {
            m_Startup = System.Windows.Forms.Application.StartupPath;
            string[] path = m_Startup.Split('\\');
            // CODE_DEBUG:
            // This symbol should be defined if the ConquerServer_v2 is being deployed on the machine
            // with the source code, if the application is running on a dedicated computer
            // the application, and the database-folder should be located in the same path
            // i.e.:
            // c:\ConquerServer_v2.exe
            // c:\Database\
#if CODE_DEBUG
            m_Path = "";
            for (int i = 0; i < path.Length - 3; i++)
                m_Path += path[i] + "\\";
#else
            m_Path = m_Startup;
#endif
            m_Path += "Database";

            AuthSystem = new IniFile(Path + @"\Misc\AuthSystem.ini");
        }

        public static void AddAuthData(AuthClient Client)
        {
            AuthSystem.WriteString("AuthSystem", Client.AuthID.ToString(), Client.Account);
        }
        public static uint ValidAccount(string Account, string Password)
        {
            IniFile rdr = new IniFile(Path + "\\Accounts\\" + Account + ".ini");
            if (File.Exists(rdr.FileName))
            {
                if (rdr.ReadString("Account", "Username", "", 16).ToLower() == Account.ToLower())
                {
                    if (rdr.ReadString("Account", "Password", "", 16) == Password)
                    {
                        return rdr.ReadUInt32("Character", "UID", 0);
                    }
                }
            }
            return 0;
        }

        public static int PermanentBan(string Account)
        {
            IniFile rdr = new IniFile(Path + "\\Accounts\\" + Account + ".ini");
            return rdr.ReadByte("Account", "Banned", 0);
        }

        public static void AddFullPermanentBan(string Account)
        {
            IniFile wrtr = new IniFile(Path + "\\Accounts\\" + Account + ".ini");
            wrtr.Write<byte>("Account", "Banned", 3);
        }
        public static void RemovePermanentBan(string Account)
        {
            IniFile wrtr = new IniFile(Path + "\\Accounts\\" + Account + ".ini");
            wrtr.Write<byte>("Account", "Banned", 0);
        }
        public static void AddLastLogin(string Account)
        {
            IniFile wrtr = new IniFile(Path + "\\Accounts\\" + Account + ".ini");
            wrtr.Write<string>("Account", "LastLoginServer", DateTime.Now.ToString());
        }
    }
}
