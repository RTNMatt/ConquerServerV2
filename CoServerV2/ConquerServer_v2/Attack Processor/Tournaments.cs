using System;
using System.Collections.Generic;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;

namespace ConquerServer_v2
{
    public class TournamentAI
    {
        private class TournamentValue : IScorable
        {
            public uint UID;
            public string User;
            private int m_Score;
            public int Score { get { return m_Score; } set { m_Score = value; } }
        }
        public static MapID MapID;
        public static ushort X;
        public static ushort Y;
        private static DictionaryV2<uint, TournamentValue> Scores;

        public const int
            TOURN_NONE = -1,
            TOURN_NORMAL = 0;

        public static bool CanJoin;
        public static bool CanFight;
        public static bool Active;
        public static int Type;

        public static void Init(GameClient Host)
        {
            if (!Active)
            {
                X = Host.Entity.X;
                Y = Host.Entity.Y;
                MapID = Host.Entity.MapID;
                MapID.MakeDynamic();
                Host.Teleport(MapID, X, Y);

                Scores = new DictionaryV2<uint, TournamentValue>();
                SendGlobalPacket.Add(MessageConst.TOURNAMENT_START1);
                SendGlobalPacket.Add(MessageConst.TOURNAMENT_START2);

                CanJoin = true;
                CanFight = false;
                Active = true;
                Type = TOURN_NORMAL;
            }
        }
        public static void Fight()
        {
            CanJoin = false;
            CanFight = true;
            SendGlobalPacket.Add(MessageConst.FIGHT, MapID);
        }
        private static unsafe void Reward(int i, TournamentValue Value)
        {
            Tournaments.AwardPoints(Value.UID, Value.User, (5-i) + Value.Score); 
        }
        public static void End()
        {
            TournamentValue[] values;
            lock (Scores)
            {
                Scores.SynchoronizeValues();
                values = Scores.EnumerableValues;
            }
            Array.Sort(values, ScoreComparer.CMP);
            for (sbyte i = 0; i < values.Length; i++)
            {
                if (i == 0)
                {
                    MessagePacket MsgPacket = new MessagePacket("Congratulations to " + values[0].User + " for winning the tournament!", 0x00FFFFFF, ChatID.Center);
                    SendGlobalPacket.Add(MsgPacket, null);
                }

                Reward(i, values[i]);

                if (i == 4)
                    break;
            }

            NobilityScoreBoard.QueryRanks();

            CanJoin = false;
            CanFight = false;
            Active = false;
            Type = TOURN_NONE;
            if (Scores != null)
            {
                Scores.Clear();
                Scores = null;
            }
            MapID = 0;
            X = 0;
            Y = 0;
        }
        public static void NotifyHit(uint AttackerUID, string AttackerName)
        {
            if (Type == TOURN_NORMAL)
            {
                lock (Scores)
                {
                    TournamentValue val;
                    if (Scores.TryGetValue(AttackerUID, out val))
                    {
                        val.Score++;
                    }
                    else
                    {
                        val = new TournamentValue();
                        val.User = AttackerName;
                        val.Score = 1;
                        val.UID = AttackerUID;
                        lock (Scores)
                        {
                            Scores.Add(val.UID, val);
                        }
                    }
                }
            }
        }
        public static string[] ShuffleScores()
        {
            string[] ret = new string[5];
            for (byte i = 0; i < ret.Length; i++)
                ret[i] = "";
            TournamentValue[] values;
            lock (Scores)
            {
                values = Scores.EnumerableValues;
            }
            Array.Sort(values, ScoreComparer.CMP);
            for (sbyte i = 0; i < values.Length; i++)
            {
                ret[i] = values[i].User + " - " + values[i].Score.ToString();
                if (i == 4)
                    break;
            }
            return ret;
        }
    }
}