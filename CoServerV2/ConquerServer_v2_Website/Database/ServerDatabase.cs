using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
