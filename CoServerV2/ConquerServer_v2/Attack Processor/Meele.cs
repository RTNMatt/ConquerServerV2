using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConquerServer_v2.Database;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Monster_AI;

namespace ConquerServer_v2.Attack_Processor
{
    public unsafe partial class AttackProcessor
    {
        private static bool ProcessArcher(GameClient Attacker, IBaseEntity Opponent, Item Quivery, SpellAnimationPacket Animate)
        {
            if (Quivery != null)
            {
                if (Quivery.IsItemType(ItemTypeConst.ArrowID))
                {
                    if (Attacker.Entity.MapID != MapID.TrainingGrounds)
                    {
                        Quivery.Arrows -= 1;
                        if (Quivery.Arrows <= 0)
                            Attacker.Unequip(Quivery.Position, false);
                        else
                            Quivery.SendArrows(Attacker);
                    }
                    if (Opponent != null)
                    {
                        Damage damage = new Damage();
                        damage.Show = CalculateArchDmg(Kernel.Random.Next(Attacker.Entity.MinAttack, Attacker.Entity.MaxAttack),
                            Attacker.Entity, Opponent, 1.00);
                        damage.Experience = Math.Min(damage.Show, Opponent.Hitpoints);
                        Opponent.Hitpoints -= damage.Show;
                        Animate.Targets.Add(Opponent, damage);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Instigates one entity verses another.
        /// </summary>
        /// <param name="Attacker">The attacking entity (i.e. Player, Monster, etc.)</param>
        /// <param name="Opponent">The defending entity (i.e. Player, Monster, etc.)</param>
        /// <param name="AttackType">The attack type, if the attacker is not a player, this should be Physical</param>
        /// <returns></returns>
        public static bool ProcessMeele(IBaseEntity Attacker, IBaseEntity Opponent, AttackID AttackType)
        {
            if (Attacker.MapID.Id == TournamentAI.MapID.Id && TournamentAI.Active)
                return false;

            bool Success = false;
            try
            {
                if (Opponent != null)
                {
                    PKMode PKMode = PKMode.Kill;
                    sbyte AttackRange = 1;
                    GameClient ClientAttacker = null;
                    Monster MonsterAttacker = null;
                    if (Attacker.EntityFlag == EntityFlag.Player)
                    {
                        ClientAttacker = Attacker.Owner as GameClient;
                        PKMode = ClientAttacker.PKMode;
                        AttackRange += (sbyte)(ClientAttacker.AttackRange);
                        if (Opponent.EntityFlag != EntityFlag.Player)
                            AttackRange += 2;
                    }
                    else 
                    {
                        if (Attacker.EntityFlag == EntityFlag.Monster || Attacker.EntityFlag == EntityFlag.Pet)
                        {
                            MonsterAttacker = Attacker.Owner as Monster;
                            AttackRange += MonsterAttacker.Family.AttackRange;
                        }
                    }
                    if (SafeAttack(Attacker, Opponent, AttackRange, PKMode) == 0)
                    {
                        SpellAnimationPacket animate = new SpellAnimationPacket();
                        animate.SpellID = Kernel.MeeleSpellID;
                        animate.X = Opponent.X;
                        animate.Y = Opponent.Y;
                        animate.AttackerUID = Attacker.UID;
                        animate.Targets = new Dictionary<IBaseEntity, Damage>(1);

                        if (ClientAttacker != null)
                        {
                            Item left = ClientAttacker.Equipment[ItemPosition.Left];
                            Item right = ClientAttacker.Equipment[ItemPosition.Right];
                            if (AttackType == AttackID.Archer)
                                Success = (ProcessArcher(ClientAttacker, Opponent, left, animate));
                            else
                                Success = (TryUsePassiveSkill(ClientAttacker, Opponent, left ?? Item.Blank, right ?? Item.Blank, animate) != 0);
                        }
                        if (animate.Targets.Count == 0 && !Success && AttackType == AttackID.Physical && Opponent != null)
                        {
                            Damage damage = new Damage();
                            damage.Show = CalculatePhysDmg(Attacker, Opponent, 1.00);
                            damage.Experience = Math.Min(damage.Show, Opponent.Hitpoints);
                            Opponent.Hitpoints -= damage.Show;
                            animate.Targets.Add(Opponent, damage);
                            Success = true;
                        }
                        if (Success && animate.Targets.Count > 0)
                        {
                            if (MonsterAttacker != null)
                            {
                                if (MonsterAttacker.OverrideSpell != 0)
                                {
                                    animate.SpellID = MonsterAttacker.OverrideSpell;
                                    animate.Level = MonsterAttacker.OverrideSpellLevel;
                                }
                            }
                            if (animate.SpellID == Kernel.MeeleSpellID)
                            {
                                RequestAttackPacket show = RequestAttackPacket.Create();
                                show.AtkType = AttackType;
                                show.AttackerX = Attacker.X;
                                show.AttackerY = Attacker.Y;
                                show.UID = Attacker.UID;
                                show.OpponentUID = Opponent.UID;
                                show.SpellID = 350;
                                show.Damage = animate.Targets[Opponent].Show;
                                SendRangePacket.Add(Attacker, Kernel.ViewDistance, 0, Kernel.ToBytes(&show), null);
                            }
                            else
                            {
                                SendRangePacket.Add(Attacker, Kernel.ViewDistance, 0, animate, null);
                            }
                            FinalizeAttack(Attacker, animate);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Kernel.NotifyDebugMsg(
                    string.Format("[Physical Processor Error - {0} vs {1} : {2}]", 
                    Attacker.EntityFlag, Opponent.EntityFlag, AttackType),
                    e.ToString(), true
                );
                Success = false;
            }
            return Success;
        }

        private static ushort TryUsePassiveSkill(GameClient Attacker, IBaseEntity Opponent, Item Left, Item Right, SpellAnimationPacket animate)
        {
            ushort LeftType = Left.GetItemType();
            ushort RightType = Right.GetItemType();
            if (LeftType == ItemTypeConst.BackswordID)
                LeftType = ItemTypeConst.SwordID;
            if (RightType == ItemTypeConst.BackswordID)
                RightType = ItemTypeConst.SwordID;

            bool OneHanded = (LeftType == 0 || RightType == 0);
            MAttackData Data;

            for (int i = 0; i < Attacker.Spells.Length; i++)
            {
                ISkill skill = Attacker.Spells.Elements[i];
                ServerDatabase.GetMAttackData(skill.ID, skill.Level, &Data);
                if (Data.TargetType == MAttackTargetType.WeaponSkill)
                {
                    if (Data.Weapon != 0 && Data.SpellID != 0 && Data.BaseDamage > 30000)
                    {
                        if (LeftType == Data.Weapon || RightType == Data.Weapon)
                        {
                            if (Data.SuccessRate > (Kernel.Random.Next(1000) % 100))
                            {
                                ProcessBySort(Attacker.Entity, Attacker, Opponent, &Data, Opponent.X, Opponent.Y, Data.Sort, ref animate.Targets);
                                animate.SpellID = Data.SpellID;
                                animate.Level = Data.SpellLevel;
                                return Data.SpellID;
                            }
                        }
                    }
                }
            }
            return 0;
        }
    }
}
