using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Database
{
    public class NobilityScoreBoard
    {
        private static IniFile Nobility;
#if !TOURNAMENT_NOBILITY
        private static DateTime LastAccess;
#endif

        private static NobilityRank[] m_Ranks;
        private static void SyncRealTime()
        {
            NobilityRankPacket nobility = new NobilityRankPacket();
            foreach (GameClient Client in Kernel.Clients)
            {
                if (Client.Entity.Level >= 70)
                {
                    nobility.Type = NobilityRankType.Icon;
                    nobility.Value = Client.Entity.UID;
                    nobility.SingleRank = NobilityScoreBoard.ObtainNobility(Client);
                    Client.Entity.Nobility = nobility.SingleRank.Rank;
                    Client.Send(nobility);
                }
            }
        }
        public static NobilityRank ObtainNobility(GameClient Client)
        {
            for (int i = 0; i < m_Ranks.Length; i++)
            {
                if (m_Ranks[i].UID == Client.Entity.UID)
                {
                    return m_Ranks[i];
                }
            }
            return new NobilityRank(Client.Entity.UID, Client.Entity.Name, NobilityID.None, 0, -1);
        }
        public static NobilityRank[] QueryRanks()
        {
            UpdateRanks();
            return m_Ranks;
        }

#if TOURNAMENT_NOBILITY
        #region #if TOURNAMENT_NOBILITY
        /* Tournament Nobility Specific */
        private static void UpdateRanks()
        {
            bool Changed;
            TournamentProfile[] Profiles = Tournaments.QueryProfiles(out Changed);
            if (Changed)
            {
                NobilityRank[] temp_Ranks = new NobilityRank[Profiles.Length];
                for (int i = 0; i < temp_Ranks.Length; i++)
                {
                    temp_Ranks[i].Listing = i;
                    temp_Ranks[i].Name = Profiles[i].Name;
                    temp_Ranks[i].Gold = Profiles[i].Amount;
                    temp_Ranks[i].UID = Profiles[i].UID;
                    temp_Ranks[i].Rank = ObtainRank(i, temp_Ranks[i].Gold);
                }
                m_Ranks = temp_Ranks;
                SyncRealTime();
            }
        }
        private static NobilityID ObtainRank(int idx, int score)
        {
            if (idx >= 0 && idx <= 1) // 1st,2nd
                return NobilityID.King;
            else if (idx >= 2 && idx <= 4) // 3rd~5th
                return NobilityID.Prince;
            else if (idx >= 5 && idx <= 9) // 6th~10th
                return NobilityID.Duke;
            if (score >= 150)
                return NobilityID.Earl;
            if (score >= 100)
                return NobilityID.Baron;
            if (score >= 50)
                return NobilityID.Knight;
            return NobilityID.None;
        }
        #endregion
#else
        #region #if !TOURNAMENT_NOBILITY
        /* Conquer 2.0 Nobility Specific */
        private static NobilityID ObtainRank(int idx, int score)
        {
            if (idx >= 0 && idx <= 2) // 1st,2nd,3rd
                return NobilityID.King;
            else if (idx >= 3 && idx <= 14) // 4th to 15th
                return NobilityID.Prince;
            else if (idx >= 15 && idx <= 49) // 16th to 50th
                return NobilityID.Duke;

            if (score >= 200000000)
                return NobilityID.Earl;
            if (score >= 100000000)
                return NobilityID.Baron;
            if (score >= 30000000)
                return NobilityID.Knight;

            return NobilityID.None;
        }
        private static void UpdateRanks()
        {
            lock (Nobility)
            {
                DateTime Time = File.GetLastWriteTime(Nobility.FileName);
                if (Time != LastAccess)
                {
                    NobilityRank tempProfile = new NobilityRank();
                    List<NobilityRank> Profiles = new List<NobilityRank>();
                    if (File.Exists(Nobility.FileName))
                    {
                        using (StreamReader rdr = new StreamReader(Nobility.FileName))
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
                                        int.TryParse(values[1], out tempProfile.Gold);
                                        Profiles.Add(tempProfile);
                                    }
                                }
                            }
                        }
                    }
                    NobilityRank[] temp_Ranks = Profiles.ToArray();
                    Array.Sort(temp_Ranks, ScoreComparer.CMP);
                    for (int i = 0; i < temp_Ranks.Length; i++)
                    {
                        temp_Ranks[i].Listing = i;
                        temp_Ranks[i].Rank = ObtainRank(i, temp_Ranks[i].Gold);
                    }
                    m_Ranks = temp_Ranks;
                    LastAccess = Time;
                    SyncRealTime();
                }
            }
        }
        public static void Donate(GameClient Client, int Amount)
        {
            lock (Nobility)
            {
                string Key = Client.Entity.UID.ToString();
                string[] value = Nobility.ReadString("Nobility", Key, "", 32).Split(',');
                if (value.Length != 2)
                    value = new string[2];
                long donation;
                long.TryParse(value[1], out donation);
                donation = Math.Min(int.MaxValue, donation + Amount);
                value[0] = Client.Entity.Name;
                value[1] = donation.ToString();
                Nobility.WriteString("Nobility", Key, value[0] + "," + value[1]);

                UpdateRanks();
            }
        }
        public static int ToConquerPoints(int Money)
        {
            const float Ratio = 0.00002f; // ConquerPoints/Money
            return (int)Math.Round(Money * Ratio);
        }
        #endregion
#endif

        public static void Init()
        {
            Nobility = new IniFile(ServerDatabase.Path + @"\Misc\Nobility.ini");
            UpdateRanks();
        }
    }
}