#define GUILDWAR_ALWAYS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;
using ConquerServer_v2.Client;

namespace ConquerServer_v2.GuildWar
{
    public unsafe class GuildWarKernel
    {
        public struct GuildWarScore : IScorable
        {
            public ushort GuildID;
            private int m_Score;
            public int Score { get { return m_Score; } set { m_Score = value; } }
        }

        public const int LeftGateSlot = 0;
        public const int RightGateSlot = 1;
        public const int PoleSlot = 2;

        public const uint LeftGateUID = 108883;
        public const uint RightGateUID = 121171;
        public const uint PoleUID = 104804;

        public static SOBMonster[] Monsters;
        private static CompressedBitmap GuildWarMap;
        private static IniFile Settings;
        private static ushort m_PoleGuildID;
        private static string m_PoleGuildName;
        private static bool m_Active;
        private static DictionaryV2<ushort, GuildWarScore> GuildScores;

        public static bool LeftGateOpen { get { return ((Monsters[LeftGateSlot]).Dead); } }
        public static bool RightGateOpen { get { return ((Monsters[RightGateSlot]).Dead); } }
        public static ushort PoleGuildID
        {
            get
            {
                ushort id = Settings.ReadUInt16("Settings", "GuildWarPoleID", 0);
                if (m_PoleGuildID != id)
                {
                    Guild.GetGuildName(id, out m_PoleGuildName);
                    m_PoleGuildID = id;
                }
                return m_PoleGuildID;
            }
            set
            {
                if (!Guild.GetGuildName(value, out m_PoleGuildName))
                    value = 0;
                Settings.Write<ushort>("Settings", "GuildWarPoleID", value);
            }
        }
        public static bool Active { get { return m_Active; } }

        public static void ReverseGate(SOBMonster Gate)
        {
            if (Gate.UID == LeftGateUID)
            {
                Gate.Spawn.SOBMesh = LeftGateOpen ? SOBMesh.RightGate : SOBMesh.LeftGate;
            }
            else if (Gate.UID == RightGateUID)
            {
                Gate.Spawn.SOBMesh = RightGateOpen ? SOBMesh.LeftGate : SOBMesh.RightGate;
            }
            fixed (SpawnSOBPacket* lpGate = &Gate.Spawn)
                SendRangePacket.Add(Gate, Kernel.ViewDistance, 0, Kernel.ToBytes(lpGate), null);
        }
        public static void PoleAttacked(GameClient Client, SOBMonster Sender, int Param)
        {
            if (Client.Entity.Spawn.GuildID != 0)
            {
                lock (GuildScores)
                {
                    GuildWarScore tmp;
                    if (GuildScores.TryGetValue(Client.Entity.Spawn.GuildID, out tmp))
                    {
                        tmp.Score += Param;
                        GuildScores[tmp.GuildID] = tmp;
                    }
                    else
                    {
                        tmp.Score = Param;
                        tmp.GuildID = Client.Entity.Spawn.GuildID;
                        GuildScores.Add(tmp.GuildID, tmp);
                    }
                }
            }
        }
        public static void GateOpen(GameClient Client, SOBMonster Sender, int Param)
        {
            ReverseGate(Sender);
            if (PoleGuildID != 0)
            {
                foreach (GameClient iClient in Kernel.Clients)
                {
                    if (iClient != null)
                    {
                        if (iClient.Entity.Spawn.GuildID == PoleGuildID)
                        {
                            //iClient.Send(Constants.Messages.GATE_BROKEN);
                        }
                    }
                }
            }
        }
        public static void GuildWarOver(GameClient Client, SOBMonster Sender, int Param)
        {
            ushort Winner = 0;
            int Score = 0;
            lock (GuildScores)
            {
                foreach (KeyValuePair<ushort, GuildWarScore> DE in GuildScores)
                {
                    if (DE.Value.Score > Score)
                    {
                        Score = DE.Value.Score;
                        Winner = DE.Key;
                    }
                }
                GuildScores.Clear();
            }
            PoleGuildID = Winner;
            Sender.Name = m_PoleGuildName;
            Sender.Dead = false;
            fixed (SpawnSOBPacket* lpPole = &Sender.Spawn)
                SendRangePacket.Add(Sender, Kernel.ViewDistance, 0, Kernel.ToBytes(lpPole), null);
            byte[] Alert = new MessagePacket("Congratulations to " + m_PoleGuildName + ", they've won the guildwar with a score of " + Score.ToString(), 0x00FFFFFF, ChatID.Center);
            SendGlobalPacket.Add(Alert);

            SOBMonster gate;
            if (LeftGateOpen)
            {
                gate = Monsters[LeftGateSlot];
                gate.Dead = false;
                ReverseGate(gate);
            }
            if (RightGateOpen)
            {
                gate = Monsters[RightGateSlot];
                gate.Dead = false;
                ReverseGate(gate);
            }
        }
        
        public static string[] ShuffleGuildScores()
        {
            string[] ret = new string[5];
            for (byte i = 0; i < ret.Length; i++)
                ret[i] = "";
            GuildWarScore[] values = GuildScores.EnumerableValues;
            Array.Sort(values, ScoreComparer.CMP);
            for (sbyte i = 0; i < values.Length; i++)
            {
                string Name;
                Guild.GetGuildName(values[i].GuildID, out Name);
                ret[i] = "No " + (i + 1).ToString() + ". " + Name + " (" + values[i].Score.ToString() + ")";
                if (i == 4)
                    break;
            }
            return ret;
        }

        public static void Init()
        {
            Settings = new IniFile(ServerDatabase.Path + @"\Guilds\Settings.ini");
            GuildScores = new DictionaryV2<ushort, GuildWarScore>();
            GuildWarMap = new CompressedBitmap(ServerDatabase.Path + @"\Misc\GuildWarMap.bmp");

            SOBMonster LeftGate = new SOBMonster(EntityFlag.GuildGate);
            LeftGate.UID = LeftGateUID;
            LeftGate.Spawn.SOBType = SOBType.Gate;
            LeftGate.Spawn.SOBMesh = SOBMesh.LeftGate;
            LeftGate.X = 163;
            LeftGate.Y = 209;
            SOBMonster RightGate = new SOBMonster(EntityFlag.GuildGate);
            RightGate.UID = RightGateUID;
            RightGate.Spawn.SOBType = SOBType.Gate;
            RightGate.Spawn.SOBMesh = SOBMesh.RightGate;
            RightGate.X = 222;
            RightGate.Y = 177;
            LeftGate.Spawn.MaxHitpoints = RightGate.Spawn.MaxHitpoints =
                RightGate.Spawn.Hitpoints = LeftGate.Spawn.Hitpoints = 10000000;
            LeftGate.Killed = RightGate.Killed = new SOBMonsterEvent(GateOpen);

            SOBMonster Pole = new SOBMonster(EntityFlag.GuildPole);
            Pole.Spawn.UID = PoleUID;
            Pole.Spawn.SOBType = SOBType.Pole;
            Pole.Spawn.SOBMesh = SOBMesh.Pole;
            Pole.X = 84;
            Pole.Y = 99;
            Pole.MaxHitpoints = Pole.Hitpoints = 40000000;
            Pole.Attacked = new SOBMonsterEvent(PoleAttacked);
            Pole.Killed = new SOBMonsterEvent(GuildWarOver);
            if (PoleGuildID != 0)
                Pole.Name = m_PoleGuildName;
            else
                Pole.Name = "None";

            Pole.MapID = LeftGate.MapID = RightGate.MapID = MapID.GuildWar;

            Monsters = new SOBMonster[3];
            Monsters[LeftGateSlot] = LeftGate;
            Monsters[RightGateSlot] = RightGate;
            Monsters[PoleSlot] = Pole;

#if !GUILDWAR_ALWAYS
            if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday ||
                DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            {
#endif
            m_Active = true;
#if !GUILDWAR_ALWAYS
            }
#endif
        }
        public static bool ValidJump(CompressBitmapColor Current, out CompressBitmapColor New, ushort X, ushort Y)
        {
            New = Current;
            CompressBitmapColor temp = GuildWarMap[X, Y];
            if (temp != CompressBitmapColor.Black)
            {
                if (Current == CompressBitmapColor.White)
                {
                    if (temp < Current)
                        return false;
                }
                New = temp;
                return true;
            }
            return false;
        }
        public static bool ValidWalk(CompressBitmapColor CurrentColor, out CompressBitmapColor NewColor, ushort X, ushort Y)
        {
            NewColor = CurrentColor;
            CompressBitmapColor temp = GuildWarMap[X, Y];
            if (temp != CompressBitmapColor.Blue)
            {
                if (Y == 209)
                {
                    if (Kernel.GetDistance(X, Y, 164, 209) <= 3)
                    {
                        if (!LeftGateOpen)
                            return false;
                    }
                }
                else if (X == 222)
                {
                    if (Kernel.GetDistance(X, Y, 222, 177) <= 3)
                    {
                        if (!RightGateOpen)
                            return false;
                    }
                }
                NewColor = temp;
                return true;
            }
            return false;
        }

        public static SOBMonster Search(uint GateID)
        {
            foreach (SOBMonster mob in Monsters)
                if (mob.UID == GateID)
                    return mob;
            return null;
        }
    }
}