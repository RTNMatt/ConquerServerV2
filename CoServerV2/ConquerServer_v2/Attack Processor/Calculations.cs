using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Monster_AI;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Attack_Processor
{
    public delegate int DamageCalculationCallback<T>(int Base, IBaseEntity Attacker, IBaseEntity Opponent, T Arg);

    public unsafe partial class AttackProcessor
    {
        public static DamageCalculationCallback<object> MagicDamage = new DamageCalculationCallback<object>(CalculateMagicDmg);
        public static DamageCalculationCallback<double> PhysicalDamage = new DamageCalculationCallback<double>(CalculatePhysDmg);
        public static DamageCalculationCallback<double> ArcherDamage = new DamageCalculationCallback<double>(CalculateArchDmg);

        public static int CalculateForTalismens(int CurrentDamage, IBaseEntity Attacker, IBaseEntity Opponent, bool Magic)
        {
            GameClient Client;
            if (Attacker.EntityFlag == EntityFlag.Player)
            {
                Client = Attacker.Owner as GameClient;
                if (Magic)
                    CurrentDamage += Client.TalismenMAttack;
                else
                    CurrentDamage += Client.TalismenAttack;
                CurrentDamage += (int)Client.Gems[GemsConst.ThunderGem];
            }
            if (Opponent.EntityFlag == EntityFlag.Player)
            {
                Client = Opponent.Owner as GameClient;
                if (Magic)
                    CurrentDamage -= Client.TalismenMDefence;
                else
                    CurrentDamage -= Client.TalismenDefence;
                Item Armor = Client.Equipment[ItemPosition.Armor];
                if (Armor != null)
                {
                    if (Armor.ID == ItemTypeConst.GMRobeID)
                        CurrentDamage = 1;
                }
                CurrentDamage -= (int)Client.Gems[GemsConst.GloryGem];
            }
            else if (Opponent.EntityFlag == EntityFlag.Monster)
            {
                Monster mob = Opponent.Owner as Monster;
                if ((mob.Settings & MonsterSettings.Invincible) == MonsterSettings.Invincible)
                    CurrentDamage = 1;
            }
            return Math.Max(CurrentDamage, 1);
        }
        public static int CalculatePhysDmg(int Base, IBaseEntity Attacker, IBaseEntity Opponent, double Offset)
        {
            Base = (int)(Math.Min(Base, 100000) * Offset);
            if (Attacker.EntityFlag == EntityFlag.Monster)
                Base = (int)(Base * (1 + (GetLevelBonus(Attacker.Level, Opponent.Level) * 0.08)));
            if ((Attacker.StatusFlag & StatusFlag.Stigma) == StatusFlag.Stigma)
                Base = (int)(Base * StigmaPercent);
            if (Attacker.EntityFlag == EntityFlag.Player)
            {
                GameClient Client = Attacker.Owner as GameClient;
                if (Client.Equipment[ItemPosition.Left] != null || Client.Equipment[ItemPosition.Right] != null)
                    Base = (int)(Base * 1.5);
                if ((Client.Entity.StatusFlag & StatusFlag.Superman) == StatusFlag.Superman)
                {
                    if (Opponent.EntityFlag == EntityFlag.Player)
                        Base *= 3;
                    else
                        Base *= 10;
                }
            }
            if (Opponent.EntityFlag == EntityFlag.Player)
            {
                if ((Opponent.StatusFlag & StatusFlag.PurpleShield) == StatusFlag.PurpleShield)
                {
                    Base -= (int)(Opponent.Defence * 2);
                }
            }
            Base -= Opponent.Defence;
            Base = RemoveExcessDamage(Base, Attacker, Opponent);
            Base = Math.Max(Base, 1);

            return CalculateForTalismens(Base, Attacker, Opponent, false);
        }
        public static int CalculatePhysDmg(IBaseEntity Attacker, IBaseEntity Opponent, double Offset)
        {
            return CalculatePhysDmg(Kernel.Random.Next(Attacker.MinAttack, Attacker.MaxAttack), Attacker, Opponent, Offset);
        }
        public static int RemoveExcessDamage(int CurrentDamage, IBaseEntity Attacker, IBaseEntity Opponent)
        {
            if (Opponent.EntityFlag != EntityFlag.Player) 
                return CurrentDamage;
            GameClient Client = Opponent.Owner as GameClient;
            if (Client.Entity.Reborn == 1)
                CurrentDamage = (int)Math.Round((double)(CurrentDamage * 0.7));
            else if (Client.Entity.Reborn == 2)
                CurrentDamage = (int)Math.Round((double)(CurrentDamage * 0.5));
            CurrentDamage = (int)Math.Round((double)(CurrentDamage * (1.00 - (Client.BlessPercent * 0.01))));
            
            if (Client.Gems[GemsConst.TortoiseGem] > 0)
                CurrentDamage = (int)Math.Round(CurrentDamage * Math.Max(1.00 - Client.Gems[GemsConst.TortoiseGem], 0.50));
            
            return CurrentDamage;
        }
        public static int CalculateMagicDmg(int Base, IBaseEntity Attacker, IBaseEntity Opponent, object Omit)
        {
            Base += Attacker.MagicAttack;
            Base = (int)(((Base * 0.75) * (1 - (Opponent.MDefence * 0.01))) - Opponent.PlusMDefence);
            Base = RemoveExcessDamage(Base, Attacker, Opponent);
            Base = Math.Max(1, (int)(Base * 0.55));
            Base = CalculateForTalismens(Base, Attacker, Opponent, true);
            return Base;
        }
        public static int CalculateArchDmg(int Base, IBaseEntity Attacker, IBaseEntity Opponent, double Factor)
        {
            float Dodge = (float)(1.00 - (Math.Min(Opponent.Dodge, 99) * 0.01));
            if (Opponent.EntityFlag == EntityFlag.Monster)
            {
                Base = (int)(Base * (1 + (GetLevelBonus(Attacker.Level, Opponent.Level) * 0.8)) * Dodge);
            }
            else
            {
                Base = (int)(Base * Dodge);
            }
            Base = RemoveExcessDamage(Base, Attacker, Opponent);
            Base = Math.Max((int)(Base * Factor), 1);
            Base = CalculateForTalismens(Base, Attacker, Opponent, false);
            return Base;
        }

        public static int GetLevelBonus(int l1, int l2)
        {
            int num = l1 - l2;
            int bonus = 0;
            if (num >= 3)
            {
                num -= 3;
                bonus = 1 + (num / 5);
            }
            return bonus;
        }
        public static int CalculateExpBonus(ushort Level, ushort MonsterLevel, int Experience)
        {
            int leveldiff = (2 + Level - MonsterLevel);
            if (leveldiff < -5) 
                Experience = (int)(Experience * 1.3);
            else if (leveldiff < -1) 
                Experience = (int)(Experience * 1.2);
            else if (leveldiff == 4) 
                Experience = (int)(Experience * 0.8);
            else if (leveldiff == 5) 
                Experience = (int)(Experience * 0.3);
            else if (leveldiff > 5) 
                Experience = (int)(Experience * 0.1);
            return Experience;
        }
    }
}