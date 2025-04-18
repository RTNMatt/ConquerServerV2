using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Database
{
    public struct TournamentProfile : IScorable
    {
        public string Name;
        public int Amount;
        public uint UID;
        public int Score { get { return Amount; } set { Amount = value; } }
    }

    public class Tournaments
    {
        public static IniFile PointsAdapter;
        private static TournamentProfile[] m_Profiles;
        private static DateTime LastAccess;

        public static void Init()
        {
            PointsAdapter = new IniFile(ServerDatabase.Path + @"\Misc\Tournament.ini");
        }
        public static void AwardPoints(uint UID, string Name, int Amount)
        {
            lock (PointsAdapter)
            {
                string[] value = PointsAdapter.ReadString("Points", UID.ToString(), "", 32).Split(',');
                if (value.Length != 2)
                    value = new string[2];
                int score;
                int.TryParse(value[1], out score);
                value[0] = Name;
                value[1] = (score + Amount).ToString();
                PointsAdapter.WriteString("Points", UID.ToString(), value[0] + "," + value[1]);
            }
        }
        public static TournamentProfile[] QueryProfiles(out bool Changed)
        {
            Changed = false;
            lock (PointsAdapter)
            {
                DateTime Time = File.GetLastWriteTime(PointsAdapter.FileName);
                if (Time != LastAccess)
                {
                    Changed = true;
                    TournamentProfile tempProfile = new TournamentProfile();
                    List<TournamentProfile> Profiles = new List<TournamentProfile>();
                    if (File.Exists(PointsAdapter.FileName))
                    {
                        using (StreamReader rdr = new StreamReader(PointsAdapter.FileName))
                        {
                            string Current;
                            Current = rdr.ReadLine();
                            if (Current != null)
                            {
                                while ((Current = rdr.ReadLine()) != null)
                                {
                                    int idx = Current.IndexOf('=');
                                    uint.TryParse(Current.Substring(0, idx), out tempProfile.UID);
                                    string[] values = Current.Substring(idx + 1, Current.Length - idx - 1).Split(',');
                                    if (values.Length == 2)
                                    {
                                        tempProfile.Name = values[0];
                                        int.TryParse(values[1], out tempProfile.Amount);
                                        Profiles.Add(tempProfile);
                                    }
                                }
                            }
                        }
                    }
                    TournamentProfile[] temp = Profiles.ToArray();
                    Array.Sort(temp, ScoreComparer.CMP);
                    m_Profiles = temp;
                    LastAccess = Time;
                }
            }
            return m_Profiles;
        }
        public static TournamentProfile[] QueryProfiles()
        {
            bool junk;
            return QueryProfiles(out junk);
        }
    }
}