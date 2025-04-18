using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.GuildWar;

namespace ConquerServer_v2.Attack_Processor
{
    public unsafe partial class AttackProcessor
    {
        /// <summary>
        /// Use this function to test whether the attacking entity, can sucessfully attack
        /// the opponent entity. Returns 0 if successful.
        /// </summary>
        /// <param name="Attacker">The attacking entity</param>
        /// <param name="Opponent">The opponent entity</param>
        /// <param name="AttackRange">The attackers attack range</param>
        /// <param name="PkMode">The PK mode of the attacker</param>
        public static sbyte SafeAttack(IBaseEntity Attacker, IBaseEntity Opponent, sbyte AttackRange, PKMode PkMode)
        {
            if (Attacker == null || Opponent == null)
                return 2;
            if (Attacker.MinAttack > Attacker.MaxAttack)
                return 3;
            if (Attacker.MapID.Id != Opponent.MapID.Id)
                return 4;
            if (Kernel.GetDistance(Attacker.X, Attacker.Y, Opponent.X, Opponent.Y) >= AttackRange)
                return 5;
            if (Attacker.Dead || Opponent.Dead)
                return 6;
            if (Attacker.UID == Opponent.UID)
                return 7;
            if ((Opponent.StatusFlag & StatusFlag.Fly) == StatusFlag.Fly)
            {
                if (Attacker.EntityFlag == EntityFlag.Monster)
                    return 8;
            }
            GameClient AttackerClient = null;
            GameClient OpponentClient = null;
            if (Opponent.EntityFlag == EntityFlag.Player)
            {
                OpponentClient = Opponent.Owner as GameClient;
                if ((OpponentClient.ServerFlags & ServerFlags.LoggedIn) != ServerFlags.LoggedIn)
                    return 9;
            }
            if (Attacker.EntityFlag == EntityFlag.Player)
            {
                AttackerClient = Attacker.Owner as GameClient;
                if (AttackerClient.Guild.ID != 0)
                {
                    if (Opponent.EntityFlag == EntityFlag.GuildPole)
                    {
                        if (!GuildWarKernel.Active)
                            return 10;
                        if (AttackerClient.Entity.GuildID == 0)
                            return 11;
                        if (AttackerClient.Entity.GuildID == GuildWarKernel.PoleGuildID)
                            return 12;
                    }
                }
                /*if (Opponent.EntityFlag == EntityFlag.TrainingGround)
                {
                    SOBMonster tgMonster = Opponent.Owner as SOBMonster;
                    if (tgMonster.Data.SOBType == SOBType.Scarecrow)
                    {
                        if (((RequestAttackPacket*)atkClient.AutoAttackPtr.Addr)->AtkType != AttackTypes.Magic)
                            return 20;
                    }
                    if (tgMonster.Level > atkClient.Entity.Data.Level)
                    {
                        atkClient.Send(Constants.Messages.TG_ERROR);
                        return 22;
                    }
                }*/
                if ((Opponent.StatusFlag & StatusFlag.Fly) == StatusFlag.Fly)
                {
                    if (((MAttackData*)AttackerClient.MAttackDataPtr.Addr)->GroundAttack)
                    {
                        return 13;
                    }
                }
            }
            if (AttackerClient != null && OpponentClient != null)
            {
                if (AttackerClient.Entity.MapID == MapID.TrainingGrounds)
                    return 14;
                if (AttackerClient.Entity.MapID.Id == TournamentAI.MapID.Id && TournamentAI.Active)
                {
                    if (!TournamentAI.CanFight)
                    {
                        AttackerClient.Send(MessageConst.PK_FORBIDDEN);
                        return 15;
                    }
                }
                else
                {
                    MapSettings Settings = new MapSettings(AttackerClient.Entity.MapID);
                    if (!Settings.Status.PKing)
                    {
                        AttackerClient.Send(MessageConst.PK_FORBIDDEN);
                        return 16;
                    }
                }
                if (OpponentClient.TimeStamps.SpawnProtection.Time > TIME.Now.Time)
                    return 17;
                if (PkMode != PKMode.Kill)
                {
                    if (PkMode == PKMode.Peace)
                        return 18;
                    if (PkMode == PKMode.Capture)
                    {
                        if ((Opponent.StatusFlag & StatusFlag.BlueName) != StatusFlag.BlueName &&
                            (Opponent.StatusFlag & StatusFlag.BlackName) != StatusFlag.BlackName)
                            return 19;
                    }
                    else if (PkMode == PKMode.Team)
                    {
                        if (AttackerClient.InTeam)
                        {
                            if (AttackerClient.Team.Search(Opponent.UID) != null)
                                return 20;
                        }
                        if (AttackerClient.Guild.ID != 0)
                        {
                            if (AttackerClient.Guild.ID == OpponentClient.Guild.ID)
                                return 21;
                            if (AttackerClient.Guild.IsAlly(OpponentClient.Guild.ID))
                                return 22;
                        }
                        if (AttackerClient.SpouseAccount != "")
                        {
                            if (AttackerClient.SpouseAccount == OpponentClient.Account)
                                return 23;
                        }
                        if (AttackerClient.Friends.Search(Opponent.UID) != null)
                            return 24;
                    }
                }
            }

            if (Attacker.EntityFlag == EntityFlag.Player &&
                Opponent.EntityFlag == EntityFlag.Player)
            {
                MapSettings Settings = new MapSettings(Opponent.MapID);
                if (!Settings.Status.PKing)
                {
                    return 10;
                }
            }
            return 0;
        }

        /// <summary>
        /// User this function to test if the attacking entity can heal the opponent entity
        /// </summary>
        /// <param name="Attacker">The attacking entity</param>
        /// <param name="Opponent">The opponent entity</param>
        /// <param name="AttackRange">The range of the healing spell (usually Kernel.ViewDistance)</param>
        /// <param name="CheckIfDead">Whether to check if they're dead (if they are dead and this is true, this function will fail).</param>
        public static sbyte SafeHeal(IBaseEntity Attacker, IBaseEntity Opponent, sbyte AttackRange, bool CheckIfDead)
        {
            if (Attacker == null || Opponent == null)
                return 2;
            if (Attacker.MapID.Id != Opponent.MapID.Id)
                return 4;
            if (Kernel.GetDistance(Attacker.X, Attacker.Y, Opponent.X, Opponent.Y) > AttackRange)
                return 5;
            if (Attacker.Dead)
                return 6;
            if (CheckIfDead)
            {
                if (Opponent.Dead)
                    return 7;
            }
            /*if (Opponent.EntityFlag == EntityFlag.TrainingGround)
            {
                if ((Opponent.Owner as SOBMonster).Data.SOBType == SOBType.Stake)
                    return 9;
            }*/
            return 0;
        }
    }
}
