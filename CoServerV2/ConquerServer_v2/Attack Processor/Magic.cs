using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConquerServer_v2.Database;
using ConquerServer_v2.Monster_AI;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Attack_Processor
{
    public unsafe partial class AttackProcessor
    {
        /// <summary>
        /// Instigates a new magic attack.
        /// </summary>
        /// <param name="Attacker">The attacking entity.</param>
        /// <param name="Opponent">The opponent entity.</param>
        /// <param name="Data">A pointer to the magic-attack data (spell info).</param>
        /// <param name="X">The X coordinate provided in the 0x3FE packet.</param>
        /// <param name="Y">The Y coordinate provided in the 0x3FE packet.</param>
        public static bool ProcessMagic(IBaseEntity Attacker, IBaseEntity Opponent, MAttackData* Data, ushort X, ushort Y)
        {
            try
            {
                Dictionary<IBaseEntity, Damage> Targets = null;
                GameClient AttackerClient = null;
                if (Attacker.EntityFlag == EntityFlag.Player)
                    AttackerClient = Attacker.Owner as GameClient;

                if (MAttackChecks(Attacker, Data, AttackerClient) == 0)
                {
                    ProcessBySort(Attacker, AttackerClient, Opponent, Data, X, Y, Data->Sort, ref Targets);
                }
                if (Targets != null)
                {
                    SpellAnimationPacket animate;

                    animate = new SpellAnimationPacket(Data->SpellID, Data->SpellLevel, X, Y, Attacker.UID, Targets);
                    SendRangePacket.Add(Attacker, Kernel.ViewDistance, 0, animate, null);

                    FinalizeAttack(Attacker, animate);
                    return true;
                }
            }
            catch (Exception e)
            {
                Kernel.NotifyDebugMsg(
                    string.Format("[Magical Processor Error - {0} {1}]",
                    Data->SpellID, Data->SpellLevel),
                    e.ToString(), true
                );
            }
            return false;
        }

        private static int MAttackChecks(IBaseEntity Attacker, MAttackData* Data, GameClient AttackerClient)
        {
            if (Data->SpellID == 0)
                return -1;
            if (AttackerClient != null)
            {
                if (Data->Stamina > AttackerClient.Stamina)
                    return -2;
                if (Data->Mana > AttackerClient.Manapoints)
                    return -3;
                if (Attacker.MapID != MapID.TrainingGrounds)
                {
                    AttackerClient.Stamina -= Data->Stamina;
                    AttackerClient.Manapoints -= Data->Mana;
                }
                if (TournamentAI.Active)
                {
                    if (Attacker.MapID.Id == TournamentAI.MapID.Id)
                    {
                        if (Data->SpellID != 1045 && Data->SpellID != 1046)
                            return -6;
                    }
                }
            }
            if (Data->SuccessRate > 0)
            {
                if (Data->SuccessRate < (Kernel.Random.Next(1000) % 100))
                    return -4;
            }
            if (Data->IsXPSkill)
            {
                if ((Attacker.StatusFlag & StatusFlag.XPSkills) != StatusFlag.XPSkills || AttackerClient == null)
                    return -5;
                AttackerClient.Entity.Spawn.StatusFlag &= ~StatusFlag.XPSkills;
            }
            if (AttackerClient != null)
            {
                BigUpdatePacket big = new BigUpdatePacket(3);
                big.UID = Attacker.UID;
                big.Append(0, UpdateID.RaiseFlag, Attacker.StatusFlag);
                big.Append(1, UpdateID.Stamina, AttackerClient.Stamina);
                big.Append(2, UpdateID.Mana, AttackerClient.Manapoints);
                AttackerClient.Send(big);
            }
            return 0;
        }

        private static bool ProcessBySort(IBaseEntity Attacker, GameClient AttackerClient, IBaseEntity Opponent, MAttackData* Data, ushort X, ushort Y, MagicSort Sort, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            bool Success = false;
            switch (Sort)
            {
                case MagicSort.ATTACK: Success = ProcessStandard(Attacker, AttackerClient, Opponent, Data, ref Targets); break;
                case MagicSort.RECRUIT: Success = ProcessRecruit(AttackerClient, Opponent, Data, ref Targets); break;
                case MagicSort.FAN: Success = ProcessFan(AttackerClient, Opponent, Data, ref Targets, X, Y); break;
                case MagicSort.BOMB: Success = ProcessBomb(AttackerClient, Opponent, Data, ref Targets); break;
                case MagicSort.ATTACHSTATUS: Success = ProcessAttach(AttackerClient, Opponent, Data, ref Targets); break;
                case MagicSort.DETACHSTATUS: Success = ProcessDetach(AttackerClient, Opponent, Data, ref Targets); break;
                case MagicSort.TRANSFORM: ProcessTransform(AttackerClient, Data, ref Targets); break;
                case MagicSort.ADDMANA: Success = ProcessAddMana(AttackerClient, Data, ref Targets); break;
                case MagicSort.DECLIFE: Success = ProcessDecLife(AttackerClient, Opponent, Data, ref Targets); break;
                case MagicSort.ATKSTATUS: ProcessAtkStatus(AttackerClient, Opponent, Data, ref Targets); break;
                case MagicSort.LINE:
                case MagicSort.LINE_PENETRABLE: Success = ProcessLineAttack(AttackerClient, ref Targets, Data, ref X, ref Y); break;
                case MagicSort.CALLPET: ProcessCallPet(AttackerClient, Data, ref Targets); break;
            }
            if (AttackerClient != null)
            {
                if (Success)
                {
                    // auto step
                    if (AttackerClient.Entity.MapID == MapID.TrainingGrounds ||
                        Sort == MagicSort.RECRUIT)
                    {
                        if (Data->NextSpellID == 0)
                            Data->NextSpellID = Data->SpellID;
                    }

                    if (Data->NextSpellID != 0)
                    {
                        RequestAttackPacket* Packet = (RequestAttackPacket*)AttackerClient.AutoAttackPtr.Addr;
                        if (Packet->SpellID != Data->NextSpellID)
                        {
                            Packet->SpellID = Data->NextSpellID;
                            Packet->SpellLevel = 0;
                        }
                        Packet->AutoStepped = true;
                    }
                }
            }
            return Success;
        }
        private static bool ProcessStandard(IBaseEntity Attacker, GameClient AttackerClient, IBaseEntity Opponent, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            PKMode PK = PKMode.Kill;
            if (AttackerClient != null)
                PK = AttackerClient.PKMode;
            if (SafeAttack(Attacker, Opponent, Data->Distance, PK) == 0)
            {
                Damage damage = new Damage();
                bool Magic = false;
                if (Data->Weapon == 0)
                {
                    damage.Show = CalculateMagicDmg(Data->BaseDamage, Attacker, Opponent, null);
                    Magic = true;
                }
                else if (AttackerClient != null) // && Data->Weapon != 0
                {
                    Item left = AttackerClient.Equipment[ItemPosition.Left];
                    Item right = AttackerClient.Equipment[ItemPosition.Right];
                    if (left == null || right == null)
                        return false;
                    if (left.GetItemType() != 601 && right.GetItemType() != 601)
                        return false;
                    damage.Show = CalculatePhysDmg(Attacker, Opponent, Data->BaseDamagePercent);
                }
                else
                {
                    return false;
                }
                damage.Experience = Math.Min(Opponent.Hitpoints, damage.Show);
                Opponent.Hitpoints -= damage.Show;

                Targets = new Dictionary<IBaseEntity, Damage>(1);
                Targets.Add(Opponent, damage);
                return Magic;
            }
            return false;
        }
        private static bool ProcessRecruit(GameClient AttackerClient, IBaseEntity Opponent, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            Damage damage = new Damage();
            if (Data->MultipleTargets)
            {
                Opponent = null;
                if (AttackerClient != null)
                {
                    if (AttackerClient.InTeam)
                    {
                        Targets = new Dictionary<IBaseEntity, Damage>(AttackerClient.Team.Teammates.Length);
                        foreach (GameClient TeamMember in AttackerClient.Team.Teammates)
                        {
                            Opponent = TeamMember.Entity;
                            if (SafeHeal(AttackerClient.Entity, Opponent, Data->Distance, true) == 0)
                            {
                                damage.Show = Data->BaseDamage;
                                damage.Experience = Math.Min(Opponent.MaxHitpoints - Opponent.Hitpoints, Data->BaseDamage);
                                Opponent.Hitpoints = Math.Min(Opponent.Hitpoints + Data->BaseDamage, Opponent.MaxHitpoints);
                                Targets.Add(Opponent, damage);
                            }
                        }
                        return true;
                    }
                    else
                    {
                        Opponent = AttackerClient.Entity;
                    }
                }
            }
            //
            if (Opponent != null)
            {
                if (SafeHeal(AttackerClient.Entity, Opponent, Data->Distance, true) == 0)
                {
                    Targets = new Dictionary<IBaseEntity, Damage>();
                    damage.Show = Data->BaseDamage;
                    damage.Experience = Math.Min(Opponent.MaxHitpoints - Opponent.Hitpoints, Data->BaseDamage);
                    Opponent.Hitpoints = Math.Min(Opponent.Hitpoints + Data->BaseDamage, Opponent.MaxHitpoints);
                    Targets.Add(Opponent, damage);
                    return true;
                }
            }
            return false;
        }
        private static bool ProcessFan(GameClient AttackerClient, IBaseEntity Opponent, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets, ushort X, ushort Y)
        {
            if (AttackerClient != null)
            {
                bool Success = false;
                //const short DefaultFanWidth = 120; // degrees
                int Base = Kernel.Random.Next(AttackerClient.Entity.MinAttack, AttackerClient.Entity.MaxAttack);
                if (Data->Weapon == ItemTypeConst.BowID)
                {
                    Item Quivery = AttackerClient.Equipment[ItemPosition.Left];
                    if (Quivery != null)
                    {
                        if (Quivery.IsItemType(ItemTypeConst.ArrowID))
                        {
                            if (Quivery.Arrows >= 3)
                            {
                                if (AttackerClient.Entity.MapID != MapID.TrainingGrounds)
                                {
                                    Quivery.Arrows -= 3;
                                    if (Quivery.Arrows <= 0)
                                        AttackerClient.Unequip(ItemPosition.Left, false);
                                    else
                                        Quivery.SendArrows(AttackerClient);
                                }

                                Success = true;
                                Targets = AttackProcessor.FieldOfViewMAttack<double, short>(AttackerClient, Data, AttackerClient.PKMode,
                                                ArcherDamage, Data->BaseDamagePercent, ConquerCallbackKernel.AngleCheck, Kernel.GetAngle(AttackerClient.Entity.X, AttackerClient.Entity.Y, X, Y), Data->Range, Base);
                            }
                        }
                    }
                }
                else
                {
                    Success = true;
                    Targets = AttackProcessor.FieldOfViewMAttack<double, short>(AttackerClient, Data, AttackerClient.PKMode,
                        PhysicalDamage, Data->BaseDamagePercent, ConquerCallbackKernel.AngleCheck, Kernel.GetAngle(AttackerClient.Entity.X, AttackerClient.Entity.Y, X, Y), Data->Range, Base);
                }
                return Success;
            }
            return false;
        }
        private static bool ProcessBomb(GameClient AttackerClient, IBaseEntity Opponent, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            if (AttackerClient != null)
            {
                bool Success = false;
                if (Data->TargetType == MAttackTargetType.BombMagic)
                {
                    Success = true;
                    Targets = AttackProcessor.FieldOfViewMAttack<object, object>(AttackerClient, Data, AttackerClient.PKMode,
                        MagicDamage, null, null, null, Data->Range, Data->BaseDamage);
                }
                else if (Data->TargetType == MAttackTargetType.Physical ||
                         Data->TargetType == MAttackTargetType.WeaponSkill)
                {
                    Success = true;
                    int Base = Kernel.Random.Next(AttackerClient.Entity.MinAttack, AttackerClient.Entity.MaxAttack);
                    Targets = AttackProcessor.FieldOfViewMAttack<double, object>(AttackerClient, Data, AttackerClient.PKMode,
                        PhysicalDamage, Data->BaseDamagePercent, null, null, Data->Range, Base);
                }
                return Success;
            }
            return false;
        }
        private static bool ProcessAttach(GameClient AttackerClient, IBaseEntity Opponent, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            /* Sadly, I have no idea how this one works. I've compared it all I can and have drawn a blank :( */
            switch (Data->SpellID)
            {
                case 1025: Superman(AttackerClient, Data, ref Targets); break;
                case 1020: XPShield(AttackerClient, Data, ref Targets); break;
                case 1110: Cyclone(AttackerClient, Data, ref Targets); break;
                case 1095: Stigma(AttackerClient, Opponent, Data, ref Targets); break;
                default: return false;
            }
            return true;
        }
        private static bool ProcessDetach(GameClient AttackerClient, IBaseEntity Opponent, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            /* Sadly, I have no idea how this one works. I've compared it all I can and have drawn a blank :( */
            switch (Data->SpellID)
            {
                case 1050:
                case 1100: Pray(AttackerClient, Opponent, ref Targets); break;
                default: return true;
            }
            return false;
        }
        private static bool ProcessTransform(GameClient Attacker, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            if (!Attacker.InTransformation)
            {
                string Section = Data->SpellID.ToString() + "-" + Data->SpellLevel.ToString();
                ushort Mesh = ServerDatabase.Transform.ReadUInt16(Section, "Mesh", 0);
                if (Mesh != 0)
                {
                    Attacker.Transform.Start(Mesh, Data->SpellID);
                    Attacker.TimeStamps.TransformFinish = TIME.Now.AddSeconds(Data->SecondsTimer);
                    Attacker.Entity.OverlappingMesh = Mesh;
                    double HPModifier = (double)Attacker.Entity.Hitpoints / Attacker.Entity.MaxHitpoints;
                    Attacker.Entity.MaxHitpoints = ServerDatabase.Transform.ReadInt32(Section, "HP", 0);
                    Attacker.Entity.Hitpoints = Math.Max((int)(Attacker.Entity.MaxHitpoints * HPModifier), 1);
                    Attacker.Entity.MaxAttack = ServerDatabase.Transform.ReadInt32(Section, "MaxAttack", 0);
                    Attacker.Entity.MinAttack = ServerDatabase.Transform.ReadInt32(Section, "MinAttack", 0);
                    Attacker.Entity.Defence = ServerDatabase.Transform.ReadUInt16(Section, "Defence", 0);
                    Attacker.Entity.Dodge = ServerDatabase.Transform.ReadSByte(Section, "Dodge", 0);
                    Attacker.Entity.MDefence = ServerDatabase.Transform.ReadUInt16(Section, "MDefence", 0);
                    Attacker.Transform.SendUpdates();
                }
            }
            Targets = new Dictionary<IBaseEntity, Damage>();
            Targets.Add(Attacker.Entity, new Damage(0, 1));
            return true;
        }
        private static bool ProcessAddMana(GameClient Attacker, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            Attacker.Manapoints = (ushort)Math.Min(Attacker.MaxManapoints, Attacker.Manapoints + Data->BaseDamage);
            
            UpdatePacket Update = UpdatePacket.Create();
            Update.UID = Attacker.Entity.UID;
            Update.ID = UpdateID.Mana;
            Update.Value = (uint)Attacker.Manapoints;
            Attacker.Send(&Update);

            Targets = new Dictionary<IBaseEntity, Damage>();
            Targets.Add(Attacker.Entity, new Damage(Data->BaseDamage, Data->BaseDamage));
            return true;
        }
        private static bool ProcessDecLife(GameClient Attacker, IBaseEntity Opponent, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            if (SafeAttack(Attacker.Entity, Opponent, Data->Distance, Attacker.PKMode) == 0)
            {
                Damage damage = new Damage();
                damage.Show = (int)(Opponent.Hitpoints * Data->BaseDamagePercent);
                damage.Experience = Math.Min(Opponent.Hitpoints, damage.Show);
                Opponent.Hitpoints -= damage.Show;

                Targets = new Dictionary<IBaseEntity, Damage>(1);
                Targets.Add(Opponent, damage);
                return true;
            }
            return false;
        }
        private static void ProcessAtkStatus(GameClient Attacker, IBaseEntity Opponent, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            if (Opponent != null)
            {
                Damage damage = new Damage();
                damage.Show = CalculatePhysDmg(Attacker.Entity, Opponent, Data->BaseDamagePercent);
                damage.Experience = Math.Min(damage.Show, Opponent.Hitpoints);
                Opponent.Hitpoints -= damage.Show;
                Targets.Add(Opponent, damage);
            }
        }
        private static bool ProcessLineAttack(GameClient Attacker, ref Dictionary<IBaseEntity, Damage> Targets, MAttackData* Data, ref ushort X, ref ushort Y)
        {
            Point[] LOS = DDALineAlgorithm.Line(Attacker.Entity.X, Attacker.Entity.Y, X, Y, Data->Range);
            IBaseEntity Opponent;
            Targets = new Dictionary<IBaseEntity, Damage>();
            Damage damage = new Damage();
            foreach (IMapObject obj in Attacker.Screen.Objects)
            {
                if (obj != null)
                {
                    foreach (Point coord in LOS)
                    {
                        if (coord.X == obj.X && coord.Y == obj.Y)
                        {
                            if (obj.MapObjType == MapObjectType.Monster || obj.MapObjType == MapObjectType.Player || obj.MapObjType == MapObjectType.SOB)
                            {
                                Opponent = obj as IBaseEntity;
                                if (!Targets.ContainsKey(Opponent))
                                {
                                    if (AttackProcessor.SafeAttack(Attacker.Entity, Opponent, Data->Range, Attacker.PKMode) == 0)
                                    {
                                        damage.Show = CalculatePhysDmg(Attacker.Entity, Opponent, 1.00);
                                        damage.Experience = Math.Min(damage.Show, Opponent.Hitpoints);
                                        Opponent.Hitpoints -= damage.Show;
                                        Targets.Add(Opponent, damage);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        private static void ProcessCallPet(GameClient Attacker, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            try
            {
                string Name = String.Empty;
                switch (Data->SpellID)
                {
                    case 4000:
                        {
                            switch (Data->SpellLevel)
                            {
                                case 0: Name = "IronGuard"; break;
                                case 1: Name = "CopperGuard"; break;
                                case 2: Name = "SilverGuard"; break;
                                case 3: Name = "GoldGuard"; break;
                            }
                            break;
                        }
                    default: Name = Data->GetName(); break;
                }

                AssignPetPacket Assignment = AssignPetPacket.Create();
                MonsterPet.GetAssignmentData(Attacker, Name, &Assignment);
                Attacker.Send(&Assignment);

                new MonsterPet(Attacker, Name, Assignment);
                Attacker.Pet.Attach();

                Targets = new Dictionary<IBaseEntity, Damage>();
                Targets.Add(Attacker.Entity, new Damage(0, 1));
            }
            catch (ArgumentException)
            {
                // pet not implemented/bad pet name
            }
        }

        private static Dictionary<IBaseEntity, Damage> FieldOfViewMAttack<TDamage, TValid>(GameClient Attacker, MAttackData* DataPtr, PKMode PKMode, 
            DamageCalculationCallback<TDamage> DamageCallback, TDamage DamageCallbackParameter,
            ConquerCallback<TValid> ValidCallback, TValid ValidCallbackParameter, sbyte Range, int BaseDamage)
        {
            Dictionary<IBaseEntity, Damage> Result = new Dictionary<IBaseEntity, Damage>();
            IBaseEntity Opponent;
            Damage damage;
            foreach (IMapObject obj in Attacker.Screen.Objects)
            {
                if (obj.MapObjType == MapObjectType.Monster || obj.MapObjType == MapObjectType.Player ||
                    obj.MapObjType == MapObjectType.SOB)
                {
                    Opponent = obj as IBaseEntity;
                    if (SafeAttack(Attacker.Entity, Opponent, Range, Attacker.PKMode) == 0)
                    {
                        if (ValidCallback != null)
                        {
                            if (ValidCallback(Attacker.Entity, Opponent, ValidCallbackParameter) == 0)
                                continue;
                        }
                        damage.Show = DamageCallback(BaseDamage, Attacker.Entity, Opponent, DamageCallbackParameter);
                        damage.Experience = Math.Min(Opponent.Hitpoints, damage.Show);
                        Opponent.Hitpoints -= damage.Show;
                        Result.Add(Opponent, damage);
                    }
                }
            }
            return Result; 
        }
    }
}
