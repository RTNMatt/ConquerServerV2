using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Database;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.GuildWar;
using ConquerServer_v2.Monster_AI;

namespace ConquerServer_v2.Attack_Processor
{
    public unsafe partial class AttackProcessor
    {
        /// <summary>
        /// Notifies that theres been a successful entity killed. This will increment the XP bar
        /// integer, and will also update the head-counter (superman, cyclone, etc.) if needed.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Death"></param>
        private static void SucessfulPlayerKill(GameClient Client, RequestAttackPacket* Death)
        {
            Client.XPSkillCounter++;
            bool UpdateCounter = false;
            if ((Client.Entity.StatusFlag & StatusFlag.Cyclone) == StatusFlag.Cyclone)
            {
                if (UpdateCounter = Client.HeadKillCounter < 30)
                    Client.TimeStamps.CycloneFinish = Client.TimeStamps.CycloneFinish.AddSeconds(2);
            }
            else if ((Client.Entity.StatusFlag & StatusFlag.Superman) == StatusFlag.Superman)
            {
                if (UpdateCounter = Client.HeadKillCounter < 30)
                    Client.TimeStamps.SuperManFinish = Client.TimeStamps.SuperManFinish.AddSeconds(2);
            }
            if (UpdateCounter)
            {
                Client.HeadKillCounter++;
                Death->KillCounter = (ushort)Client.HeadKillCounter;
            }
        }

        /// <summary>
        /// Finalizes an attack on a list of entities in the SpellAnimation
        /// </summary>
        /// <param name="Attacker">The attacker (this parameter can be null: i.e. poisions)</param>
        /// <param name="Animate">The spell animation packet.</param>
        public static void FinalizeAttack(IBaseEntity Attacker, SpellAnimationPacket Animate)
        {
            //const float NORMAL_RATE = 1.00F;
            RequestAttackPacket Death = RequestAttackPacket.Create();
            UpdatePacket Update = UpdatePacket.Create();
            MAttackData* MAttackDataPtr = null;
            GameClient OpponentClient = null;
            GameClient AttackerClient = null;
            IBaseEntity Opponent;

            if (Attacker != null)
            {
                if (Attacker.EntityFlag == EntityFlag.Player)
                {
                    AttackerClient = Attacker.Owner as GameClient;
                    MAttackDataPtr = (MAttackData*)AttackerClient.MAttackDataPtr.Addr;
                }
                Death.UID = Attacker.UID;
                Death.AtkType = AttackID.Death;
            }

            foreach (KeyValuePair<IBaseEntity, Damage> DE in Animate.Targets)
            {
                Opponent = DE.Key;
                if (Opponent.EntityFlag == EntityFlag.Player)
                    OpponentClient = Opponent.Owner as GameClient;

                if (Opponent.Dead)
                {
                    Death.OpponentUID = Opponent.UID;
                    Death.X = Opponent.X;
                    Death.Y = Opponent.Y;
                    Death.KilledMonster = (Opponent.EntityFlag == EntityFlag.Monster || Opponent.EntityFlag == EntityFlag.Pet);

                    if (AttackerClient != null)
                        SucessfulPlayerKill(AttackerClient, &Death);
                    SendRangePacket.Add(Opponent, Kernel.ViewDistance, 0, Kernel.ToBytes(&Death), null);
                }

                switch (Opponent.EntityFlag)
                {
                    #region EntityFlag.Player
                    case EntityFlag.Player:
                        {
                            MapSettings Settings = new MapSettings(Opponent.MapID);
                            if (AttackerClient != null)
                            {
                                if (Settings.Status.CanGainPKPoints)
                                {
                                    if (MAttackDataPtr->Aggressive)
                                    {
                                        if ((Opponent.StatusFlag & StatusFlag.BlackName) != StatusFlag.BlackName &&
                                            (Opponent.StatusFlag & StatusFlag.BlueName) != StatusFlag.BlueName)
                                        {
                                            if (Opponent.Dead)
                                            {
                                                ushort Gain = 10;
                                                if (AttackerClient.Guild.ID != 0 && OpponentClient.Guild.ID != 0)
                                                {
                                                    if (AttackerClient.Guild.IsEnemy(OpponentClient.Guild.ID))
                                                        Gain = 3;
                                                }
                                                else if ((Opponent.StatusFlag & StatusFlag.RedName) == StatusFlag.RedName)
                                                    Gain = 5;
                                                AttackerClient.AddPKPoints(Gain);
                                            }
                                            AttackerClient.FlashBlue();
                                        }
                                    }
                                }
                            }
                            Update.UID = Opponent.UID;
                            Update.ID = UpdateID.Hitpoints;
                            Update.Value = (uint)Opponent.Hitpoints;
                            OpponentClient.Send(&Update);
                            /* deduct stam --
                            if (DE.Value.Show > 0 && OpponentClient.Entity.Action == ConquerAction.Sit)
                            {
                                if (OpponentClient.Stamina > 0)
                                {
                                    OpponentClient.Stamina /= 4;
                                    Update.ID = UpdateID.Stamina;
                                    Update.Value = (uint)OpponentClient.Stamina;
                                    OpponentClient.Send(&Update);
                                    OpponentClient.Entity.Action = ConquerAction.None;
                                }
                            }*/
                            if (Opponent.Dead)
                            {
                                if (TournamentAI.Active && Attacker.MapID.Id == TournamentAI.MapID.Id)
                                {
                                    TournamentAI.NotifyHit(Attacker.UID, Attacker.Name);
                                }
                                if (AttackerClient != null)
                                {
                                    if (Settings.Status.CanGainPKPoints)
                                    {
                                        if ((Opponent.StatusFlag & StatusFlag.BlackName) == StatusFlag.BlackName)
                                        {
                                            SendGlobalPacket.Add(new MessagePacket(Opponent.Name + " has been captured by " + Attacker.Name + " and has been sent to jail.", 0x00FFFFFF, ChatID.Talk));
                                        }
                                    }
                                }
                                OpponentClient.KillPlayer(TIME.Now.AddSeconds(3));
                            }
                            break;
                        }
                    #endregion
                    #region EntityFlag.Monster
                    case EntityFlag.Monster:
                        {
                            Monster OpponentMonster = Opponent.Owner as Monster;
                            if (AttackerClient != null)
                            {
                                if ((OpponentMonster.Settings & MonsterSettings.Standard) == MonsterSettings.Standard)
                                {
                                    int Experience = DE.Value.Experience;
                                    AttackerClient.AwardExperience(Experience, true);
                                    AttackerClient.DistributeTeamExperience(OpponentMonster);
                                    if (Animate.SpellID == Kernel.MeeleSpellID)
                                        AttackerClient.AwardProfExperience(Experience);
                                    else
                                        AttackerClient.AwardSpellExperience(Animate.SpellID, Experience);
                                }
                                else if ((OpponentMonster.Settings & MonsterSettings.Guard) == MonsterSettings.Guard)
                                {
                                    if (MAttackDataPtr->Aggressive)
                                        AttackerClient.FlashBlue();
                                }
                            }
                            if (Opponent.Dead)
                            {
                                if (AttackerClient != null)
                                    AttackerClient.XPSkillCounter++;
                                OpponentMonster.Kill(Attacker, TIME.Now.AddSeconds(5));
                            }
                            break;
                        }
                    #endregion
                    #region case EntityFlag.Pet
                    case EntityFlag.Pet:
                        {
                            if (Opponent.Dead)
                            {
                               (Opponent.Owner as MonsterPet).Kill(Attacker, TIME.Now.AddSeconds(5));
                            }
                            break;
                        }
                    #endregion
                    #region EntityFlag.GuildPole & GuildGate
                    case EntityFlag.GuildGate:
                    case EntityFlag.GuildPole:
                        {
                            SOBMonster sob = Opponent.Owner as SOBMonster;
                            if (Attacker != null)
                            {
                                if (sob.Attacked != null)
                                {
                                    if (Attacker.EntityFlag == EntityFlag.Player)
                                        sob.Attacked(AttackerClient, sob, DE.Value.Show);
                                }
                            }
                            if (sob.Dead)
                            {
                                if (sob.Killed != null)
                                    sob.Killed(null, sob, 0);
                            }
                            break;
                        }
                    #endregion
                }
            }
        }
    }
}