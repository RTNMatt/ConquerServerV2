using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Monster_AI
{
    public struct MobRateWatcher
    {
        private int tick;
        private int count;
        public static implicit operator bool(MobRateWatcher q)
        {
            bool result = false;
            q.count++;
            if (q.count == q.tick)
            {
                q.count = 0;
                result = true;
            }
            return result;
        }
        public MobRateWatcher(int Tick)
        {
            tick = Tick;
            count = 0;
        }
    }

    public struct SpecialItemWatcher
    {
        public uint ID;
        public MobRateWatcher Rate;
        public SpecialItemWatcher(uint ID, int Tick)
        {
            this.ID = ID;
            Rate = new MobRateWatcher(Tick);
        }
    }

    public class MobItemGenerator
    {
        private static ushort[] NecklaceType = new ushort[] { 120, 121 };
        private static ushort[] RingType = new ushort[] { 120, 121 };
        private static ushort[] ArmetType = new ushort[] { 111, 112, 113, 114, 117, 118 };
        private static ushort[] ArmorType = new ushort[] { 130, 131, 132, 133, 134 };
        private static ushort[] OneHanderType = new ushort[] { 410, 420, 421, 430, 440, 450, 460, 480, 481, 490, 500, 601 };
        private static ushort[] TwoHanderType = new ushort[] { 510, 530, 560, 561, 580, 900, };
        private MonsterFamily Family;

        private MobRateWatcher Refined;
        private MobRateWatcher Unique;
        private MobRateWatcher Elite;
        private MobRateWatcher Super;
        private MobRateWatcher PlusOne;

        public MobItemGenerator(MonsterFamily family)
        {
            Family = family;
            Refined = new MobRateWatcher(10000 / Family.Level);
            Unique = new MobRateWatcher(40000 / Family.Level);
            Elite = new MobRateWatcher(80000 / Family.Level);
            Super = new MobRateWatcher(100000 / Family.Level);
            PlusOne = new MobRateWatcher(60000 / Family.Level);
        }
        public uint GenerateItemId(out byte dwItemQuality, out bool Special)
        {
            Special = false;
            foreach (SpecialItemWatcher sp in Family.DropSpecials)
            {
                if (sp.Rate)
                {
                    Special = true;
                    dwItemQuality = (byte)(sp.ID % 10);
                    return sp.ID;
                }
            }

            dwItemQuality = GenerateQuality();
            uint dwItemSort = 0;
            uint dwItemLev = 0;

            int nRand = Kernel.Random.Next(0, 1200);
            if (nRand >= 0 && nRand < 20) // 0.17%
            {
                dwItemSort = 160;
                dwItemLev = Family.DropBoots;
            }
            else if (nRand >= 20 && nRand < 50) // 0.25%
            {
                dwItemSort = NecklaceType[Kernel.Random.Next(0, NecklaceType.Length)];
                dwItemLev = Family.DropNecklace;
            }
            else if (nRand >= 50 && nRand < 100) // 4.17%
            {
                dwItemSort = RingType[Kernel.Random.Next(0, RingType.Length)];
                dwItemLev = Family.DropRing;
            }
            else if (nRand >= 100 && nRand < 400) // 25%
            {
                dwItemSort = ArmetType[Kernel.Random.Next(0, ArmetType.Length)];
                dwItemLev = Family.DropArmet;
            }
            else if (nRand >= 400 && nRand < 700) // 25%
            {
                dwItemSort = ArmorType[Kernel.Random.Next(0, ArmorType.Length)];
                dwItemLev = Family.DropArmor;
            }
            else // 45%
            {
                int nRate = Kernel.Random.Next(0, 1000) % 100;
                if (nRate >= 0 && nRate < 20) // 20% of 45% (= 9%) - Backswords
                {
                    dwItemSort = 421;
                }
                else if (nRate >= 40 && nRate < 80)	// 40% of 45% (= 18%) - One handers
                {
                    dwItemSort = OneHanderType[Kernel.Random.Next(0, OneHanderType.Length)];
                    dwItemLev = Family.DropWeapon;
                }
                else if (nRand >= 80 && nRand < 100)// 20% of 45% (= 9%) - Two handers (and shield)
                {
                    dwItemSort = TwoHanderType[Kernel.Random.Next(0, TwoHanderType.Length)];
                    dwItemLev = ((dwItemSort == 900) ? Family.DropShield : Family.DropWeapon);
                }
            }
            if (dwItemLev != 99)
            {
                dwItemLev = AlterItemLevel(dwItemLev, dwItemSort);
                uint idItemType = (dwItemSort * 1000) + (dwItemLev * 10) + dwItemQuality;
                if (ServerDatabase.ValidItemID(idItemType))
                    return idItemType;
            }
            return 0;
        }
        public byte GeneratePurity()
        {
            if (PlusOne)
                return 1;
            return 0;
        }
        public byte GenerateBless()
        {
            if (Kernel.Random.Next(0, 1000) < 250) // 25%
            {
                int selector = Kernel.Random.Next(0, 100);
                if (selector < 1)
                    return 5;
                else if (selector < 6)
                    return 3;
            }
            return 0;
        }
        public byte GenerateSocketCount(uint ItemID)
        {
            if (ItemID >= 410000 && ItemID <= 601999)
            {
                int nRate = Kernel.Random.Next(0, 1000) % 100;
                if (nRate < 5) // 5%
                    return 2;
                else if (nRate < 20) // 15%
                    return 1;
            }
            return 0;
        }
        private byte GenerateQuality()
        {
            if (Refined)
                return 6;
            else if (Unique)
                return 7;
            else if (Elite)
                return 8;
            else if (Super)
                return 9;
            return 3;
        }
        public int GenerateGold(out uint ItemID)
        {
            int amount = Kernel.Random.Next(Family.DropMoney, Family.DropMoney * 10);
            ItemID = Kernel.MoneyToItemID(amount);
            return amount;
        }
        private uint AlterItemLevel(uint dwItemLev, uint dwItemSort)
        {
            int nRand = Kernel.Random.Next(0, 1000) % 100;
            if (nRand < 50) // 50% down one level
            {
                uint dwLev = dwItemLev;
                dwItemLev = (uint)(Kernel.Random.Next(0, (int)(dwLev / 2)) + dwLev / 3);

                if (dwItemLev > 1)
                    dwItemLev--;
            }
            else if (nRand > 80) // 20% up one level
            {
                if ((dwItemSort >= 110 && dwItemSort <= 114) ||
                    (dwItemSort >= 130 && dwItemSort <= 134) ||
                    (dwItemSort >= 900 && dwItemSort <= 999))
                {
                    dwItemLev = Math.Min(dwItemLev + 1, 9);
                }
                else
                {
                    dwItemLev = Math.Min(dwItemLev + 1, 23);
                }
            }
            return dwItemLev;
        }
    }
}
