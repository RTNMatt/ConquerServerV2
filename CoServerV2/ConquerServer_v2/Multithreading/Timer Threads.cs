using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;
using ConquerServer_v2.Attack_Processor;
using ConquerServer_v2.GuildWar;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Packet_Processor;

namespace ConquerServer_v2.Multithreading
{
    public unsafe class TimerThreads
    {
        private static TIME WatchStamina;
        private const int StaminaWaitTime = 1800;
        private static TIME ShortWatchStamina;
        private const int ShortStaminaWaitTime = 800;
        private static TIME WatchXPSkills;
        private const int WatchXPSkillsTime = 2250;
        private static TIME WatchLong;
        private const int WatchLongTime = 10000;
        private static TIME WatchMeele;
        private const int WatchMeeleTime = 400;
        private static TIME WatchMagic;
        private const int WatchMagicTime = 400;
        private static TIME WatchTrainingGrounds;
        private const int WatchTrainingGroundsTime = 400;
        private static TIME WatchArcher;
        private const int WatchArcherTime = 400;
        private static TIME WatchScore;
        private const int WatchScoreTime = 30000;
        private static TIME WatchPKPoints;
        private const int WatchPKPointsTime = 3 * 60000;

        private static void ScoreTimer()
        {
            MessagePacket[] Msgs = new MessagePacket[6];
            MessagePacket Msg = new MessagePacket("", 0xCCCC00, ChatID.ClearTopRight);
            Msgs[0] = Msg;
            Msg.ChatType = ChatID.TopRight; 
            byte i = 1;
            foreach (string StrMsg in GuildWarKernel.ShuffleGuildScores())
            {
                Msg.Message = StrMsg;
                Msgs[i] = Msg;
                i++;
            }
            SendGlobalPacket.Add(Msgs[0], MapID.GuildWar,
                delegate(IBaseEntity nil, IBaseEntity _Caller)
                {
                    GameClient Caller = _Caller.Owner as GameClient;
                    for (byte i2 = 1; i2 < Msgs.Length; i2++)
                    {
                        Caller.Send(Msgs[i2]);
                    }
                    return 0;
                }
            );
            if (TournamentAI.Active)
            {
                i = 1;
                foreach (string str in TournamentAI.ShuffleScores())
                {
                    Msg.Message = str;
                    Msgs[i] = Msg;
                    i++;
                }
                SendGlobalPacket.Add(Msgs[0], TournamentAI.MapID,
                    delegate(IBaseEntity nil, IBaseEntity _Caller)
                    {
                        GameClient Caller = _Caller.Owner as GameClient;
                        for (byte i2 = 1; i2 < Msgs.Length; i2++)
                        {
                            Caller.Send(Msgs[i2]);
                        }
                        return 0;
                    }
                );
            }
        }
        private static void StaminaTimer(GameClient Client, UpdatePacket* UpdatePtr, bool Short)
        {
            if (Client.Stamina < 100)
            {
                if (Short)
                {
                    if (Client.Entity.Action == ConquerAction.Sit)
                        Client.Stamina += 10;
                }
                else
                {
                    if (Client.Entity.Action != ConquerAction.Sit)
                        Client.Stamina += 7;//3;
                }
                Client.Stamina = Math.Min(Client.Stamina, (sbyte)100);
                UpdatePtr->ID = UpdateID.Stamina;
                UpdatePtr->UID = Client.Entity.UID;
                UpdatePtr->Value = (uint)Client.Stamina;
                Client.Send(UpdatePtr);
            }
        }
        private static void WatchLongTimer(GameClient Client, UpdatePacket* UpdatePtr, TIME Now)
        {
            bool SendStatus = false;
            if (Client.InTransformation)
            {
                if (Client.TimeStamps.TransformFinish.Time <= Now.Time)
                {
                    Client.Transform.Stop();
                    Client.Transform.SendUpdates();
                }
            }
            if ((Client.Entity.StatusFlag & StatusFlag.Superman) == StatusFlag.Superman)
            {
                if (Client.TimeStamps.SuperManFinish.Time <= Now.Time)
                {
                    SendStatus = true;
                    Client.Entity.Spawn.StatusFlag &= ~StatusFlag.Superman;
                }
            }
            if ((Client.Entity.StatusFlag & StatusFlag.Cyclone) == StatusFlag.Cyclone)
            {
                if (Client.TimeStamps.CycloneFinish.Time <= Now.Time)
                {
                    SendStatus = true;
                    Client.Entity.Spawn.StatusFlag &= ~StatusFlag.Cyclone;
                }
            }
            if ((Client.Entity.StatusFlag & StatusFlag.Stigma) == StatusFlag.Stigma)
            {
                if (Client.TimeStamps.StigmaFinish.Time <= Now.Time)
                {
                    SendStatus = true;
                    Client.Entity.Spawn.StatusFlag &= ~StatusFlag.Stigma;
                }
            }
            if ((Client.Entity.StatusFlag & StatusFlag.PurpleShield) == StatusFlag.PurpleShield)
            {
                if (Client.TimeStamps.XPShieldFinish.Time <= Now.Time)
                {
                    SendStatus = true;
                    Client.Entity.Spawn.StatusFlag &= ~StatusFlag.PurpleShield;
                }
            }
            if ((Client.Entity.StatusFlag & StatusFlag.BlueName) == StatusFlag.BlueName)
            {
                if (Client.TimeStamps.BlueName.Time <= Now.Time)
                {
                    SendStatus = true;
                    Client.Entity.Spawn.StatusFlag &= ~StatusFlag.BlueName;
                }
            }
            if (SendStatus)
            {
                UpdatePtr->ID = UpdateID.RaiseFlag;
                UpdatePtr->UID = Client.Entity.UID;
                UpdatePtr->BigValue = Client.Entity.StatusFlag;
                SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(UpdatePtr), null);
                UpdatePtr->BigValue = 0;
            }
        }
        private static void XPSkillsTimer(GameClient Client, UpdatePacket* UpdatePtr, TIME Now)
        {
            if (!Client.Entity.Dead)
            {
                if ((Client.Entity.StatusFlag & StatusFlag.XPSkills) == StatusFlag.XPSkills)
                {
                    if (Now.Time > Client.TimeStamps.CanSeeXPSkills.Time)
                    {
                        Client.Entity.Spawn.StatusFlag &= ~StatusFlag.XPSkills;
                        UpdatePtr->ID = UpdateID.RaiseFlag;
                        UpdatePtr->UID = Client.Entity.UID;
                        UpdatePtr->BigValue = Client.Entity.StatusFlag;
                        Client.Send(UpdatePtr);
                    }
                }
                else
                {
                    Client.XPSkillCounter++;
                    if (Client.XPSkillCounter >= 100)
                    {
                        Client.Entity.Spawn.StatusFlag |= StatusFlag.XPSkills;
                        Client.Entity.Spawn.StatusFlag &= ~StatusFlag.Superman;
                        Client.Entity.Spawn.StatusFlag &= ~StatusFlag.Cyclone;
                        Client.TimeStamps.CanSeeXPSkills = Now.AddSeconds(20);
                        Client.XPSkillCounter = 0;

                        UpdatePtr->ID = UpdateID.RaiseFlag;
                        UpdatePtr->UID = Client.Entity.UID;
                        UpdatePtr->BigValue = Client.Entity.StatusFlag;
                        Client.Send(UpdatePtr);
                    }
                }
            }
        }
        private static void WatchPKPointTimer(GameClient Client, UpdatePacket* UpdatePtr)
        {
            if (Client.PKPoints > 0)
            {
                if (Client.AddPKPoints(-1))
                {
                    UpdatePtr->UID = Client.Entity.UID;
                    UpdatePtr->ID = UpdateID.RaiseFlag;
                    UpdatePtr->BigValue = Client.Entity.StatusFlag;
                    SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(UpdatePtr), null);
                    UpdatePtr->BigValue = 0; // Theres a point to this.
                }
                UpdatePtr->Value = Client.PKPoints;
                UpdatePtr->ID = UpdateID.PKPoints;
                Client.Send(UpdatePtr);
            }
        }
        private static void EventTimers()
        {  
            UpdatePacket singleupdate = UpdatePacket.Create();

            while (true)
            {
                TIME Now = TIME.Now;
                bool WatchStaminaTick;
                bool WatchXPSkillsTick;
                bool WatchLongTick;
                bool WatchPKPointsTick;
                bool ShortWatchStaminaTick;

                if (WatchStaminaTick = (WatchStamina.Time <= Now.Time))
                    WatchStamina = Now.AddMilliseconds(StaminaWaitTime);
                if (ShortWatchStaminaTick = (ShortWatchStamina.Time <= Now.Time))
                    ShortWatchStamina = Now.AddMilliseconds(ShortStaminaWaitTime);
                if (WatchXPSkillsTick = (WatchXPSkills.Time <= Now.Time))
                    WatchXPSkills = Now.AddMilliseconds(WatchXPSkillsTime);
                if (WatchLongTick = (WatchLong.Time <= Now.Time))
                    WatchLong = Now.AddMilliseconds(WatchLongTime);
                if (WatchScore.Time <= Now.Time)
                {
                    WatchScore = Now.AddMilliseconds(WatchScoreTime);
                    ScoreTimer();
                }
                if (WatchPKPointsTick = (WatchPKPoints.Time <= Now.Time))
                    WatchPKPoints = Now.AddMilliseconds(WatchPKPointsTime);
                
                foreach (GameClient Client in Kernel.Clients)
                {
                    if (WatchStaminaTick)
                        StaminaTimer(Client, &singleupdate, false);
                    if (ShortWatchStaminaTick)
                        StaminaTimer(Client, &singleupdate, true);
                    if (WatchXPSkillsTick)
                        XPSkillsTimer(Client, &singleupdate, Now);
                    if (WatchLongTick)
                        WatchLongTimer(Client, &singleupdate, Now);
                    if (WatchPKPointsTick)
                        WatchPKPointTimer(Client, &singleupdate);
                    if (Client.IsMining)
                    {
                        if (Client.Mine.CanMine)
                        {
                            Client.Mine.SwingPickaxe();
                        }
                    }
                }

                Thread.Sleep(200);
            }
        }

        private static sbyte AutoAttackCheck(GameClient iClient, AttackID atkType, TIME Now)
        {
            if (iClient != null)
            {
                if ((iClient.ServerFlags & ServerFlags.IsAutoAttacking) == ServerFlags.IsAutoAttacking)
                {
                    RequestAttackPacket* lpPacket = (RequestAttackPacket*)iClient.AutoAttackPtr.Addr;
                    if (lpPacket->AtkType == atkType)
                    {
                        if (iClient.TimeStamps.CanAttack.Time <= Now.Time)
                        {
                            if (iClient.Entity.X == lpPacket->AttackerX &&
                                iClient.Entity.Y == lpPacket->AttackerY)
                            {
                                if (iClient.AutoAttackEntity != null)
                                {
                                    if (iClient.AutoAttackEntity.Dead)
                                    {
                                        iClient.ServerFlags &= ~ServerFlags.IsAutoAttacking;
                                        return -1;
                                    }
                                }
                                return 1;
                            }
                            else
                            {
                                iClient.ServerFlags &= ~ServerFlags.IsAutoAttacking;
                                return -1;
                            }
                        }
                    }
                }
            }
            return 0;
        }
        private static void MeeleTimer(GameClient Client, TIME Now)
        {
            if ((Client.ServerFlags & ServerFlags.PhysicalAuto) == ServerFlags.PhysicalAuto)
            {
                if (AutoAttackCheck(Client, AttackID.Physical, Now) == 1)
                {
                    PacketProcessor.PhysialAttack(Client);
                }
            }
        }
        private static void MagicTimer(GameClient Client, TIME Now)
        {
            if ((Client.ServerFlags & ServerFlags.MagicAuto) == ServerFlags.MagicAuto)
            {
                if (AutoAttackCheck(Client, AttackID.Magic, Now) == 1)
                {
                    RequestAttackPacket* AtkPtr = (RequestAttackPacket*)Client.AutoAttackPtr.Addr;
                    if (AtkPtr->AutoStepped)
                        PacketProcessor.MagicAttack(Client, AtkPtr->SpellID);
                    else
                    {
                        AttackSystem.ClearFlags(Client);
                    }
                }
            }
        }
        private static void TrainingGroundsTimer(GameClient Client, TIME Now)
        {
            if (Client.Entity.MapID == MapID.TrainingGrounds)
            {
               
            }
        }
        private static void ArcherTimer(GameClient Client, TIME Now)
        {
            if (AutoAttackCheck(Client, AttackID.Archer, Now) == 1)
                PacketProcessor.PhysialAttack(Client);
        }
        private static void AttackTimers()
        {
            while (true)
            {
                TIME Now = TIME.Now;
                bool WatchMeeleTick;
                bool WatchMagicTick;
                bool WatchTrainingGroundsTick;
                bool WatchArcherTick;

                if (WatchMeeleTick = (WatchMeele.Time <= Now.Time))
                    WatchMeele = Now.AddMilliseconds(WatchMagicTime);
                if (WatchMagicTick = (WatchMagic.Time <= Now.Time))
                    WatchMagic = Now.AddMilliseconds(WatchMagicTime);
                if (WatchTrainingGroundsTick = (WatchTrainingGrounds.Time <= Now.Time))
                    WatchTrainingGrounds = Now.AddMilliseconds(WatchTrainingGroundsTime);
                if (WatchArcherTick = (WatchArcher.Time <= Now.Time))
                    WatchArcher = Now.AddMilliseconds(WatchArcherTime);

                foreach (GameClient Client in Kernel.Clients)
                {
                    if (WatchMeeleTick)
                        MeeleTimer(Client, Now);
                    if (WatchMagicTick)
                        MagicTimer(Client, Now);
                    //if (WatchTrainingGroundsTick)
                    //    TrainingGroundsTimer(Client, Now);
                    if (WatchArcherTick)
                        ArcherTimer(Client, Now);
                }

                Thread.Sleep(200);
            }
        }

        public static void Start()
        {
            new Thread(EventTimers).Start();
            new Thread(AttackTimers).Start();
        }
    }
}
