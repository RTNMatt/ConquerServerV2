using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;
using ConquerServer_v2.Monster_AI;

namespace ConquerServer_v2.Database
{
    public struct MineField
    {
        public static SpecialItemWatcher[][] Gems;
        static MineField()
        {
            Gems = new SpecialItemWatcher[GemsConst.MaxMineGems][];
            for (int i = 0; i < Gems.Length; i++)
            {
                Gems[i] = new SpecialItemWatcher[3];
                for (byte i2 = 0; i2 < Gems[i].Length; i2++)
                {
                    uint ID = (uint)(700000 + (i * 10) + (i2 + 1));
                    int Rate;
                    if (ID == 2)
                        Rate = 10000;
                    else if (ID == 1)
                        Rate = 5000;
                    else
                        Rate = 1000;
                    Gems[i][i2] = new SpecialItemWatcher(ID, Rate);
                }
            }
        }

        public struct Ore
        {
            public uint ID;
            public sbyte MaxQuality;
            public uint GetRandom()
            {
                uint num = ID;
                if (MaxQuality > 0)
                {
                    num += (uint)Kernel.Random.Next(0, MaxQuality);
                }
                return num;
            }
        }
        public Ore[] Ores;
        public uint[] FieldGems;
        public bool ValidField
        {
            get { return (Ores.Length > 0); }
        }
        public MineField(uint MapID)
        {
            string strMap = MapID.ToString();
            Ores = new Ore[ServerDatabase.Mining.ReadSByte(strMap, "OreCount", 0)];
            for (sbyte i = 0; i < Ores.Length; i++)
            {
                Ores[i].ID = ServerDatabase.Mining.ReadUInt32(strMap, "Ore" + i.ToString(), 0);
                Ores[i].MaxQuality = ServerDatabase.Mining.ReadSByte("Quality", Ores[i].ID.ToString(), 0);
            }
            FieldGems = new uint[ServerDatabase.Mining.ReadSByte(strMap, "GemCount", 0)];
            for (sbyte i = 0; i < Ores.Length; i++)
            {
                FieldGems[i] = ServerDatabase.Mining.ReadUInt32(strMap, "Gem" + i.ToString(), 0);
            }
        }
    }
}
