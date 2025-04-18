using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;
using ConquerServer_v2.Attack_Processor;

namespace ConquerServer_v2
{
    public unsafe class AttackSystem
    {
        public static void ApplyDelay(GameClient Client, TIME Now)
        {
            if ((Client.Entity.StatusFlag & StatusFlag.Cyclone) == StatusFlag.Cyclone)
                Client.TimeStamps.CanAttack = Now.AddMilliseconds(Client.AutoAttackSpeed / 10);
            else
                Client.TimeStamps.CanAttack = Now.AddMilliseconds(Client.AutoAttackSpeed);
        }
        public static void ForceDelay(GameClient Client, TIME Now)
        {
            Client.TimeStamps.CanAttack = Now;
        }
        public static void ClearFlags(GameClient Client)
        {
            Client.ServerFlags &= ~ServerFlags.IsAutoAttacking;
            Client.ServerFlags &= ~ServerFlags.MagicAuto;
            Client.ServerFlags &= ~ServerFlags.PhysicalAuto;
            Client.AutoAttackEntity = null;
        }

        public class MagicAttackData
        {
            public ushort SpellID;
            public GameClient Client;
            public MagicAttackData(ushort id, GameClient client)
            {
                SpellID = id;
                Client = client;
            }
        }
        public class MagicAttackSystem : SmartQueue<MagicAttackData>
        {
            public MagicAttackSystem()
                : base(4)
            {
            }
            protected override void OnDequeue(MagicAttackData MData)
            {
                GameClient Client = MData.Client;
                if (!Client.InTransformation)
                {
                    RequestAttackPacket* Packet = (RequestAttackPacket*)Client.AutoAttackPtr.Addr;
                    bool AutoStepped = Packet->AutoStepped;
                    Packet->AutoStepped = false;

                    TIME Now = TIME.Now;
                    if (Client.TimeStamps.CanAttack.Time <= Now.Time || AutoStepped)
                    {
                        Client.TimeStamps.SpawnProtection = Now;
                        ISkill skill = null;
                        if (AutoStepped)
                        {
                            if (!Client.Spells.GetSkill(Packet->SpellID, out skill))
                            {
                                skill = new Spell();
                                skill.ID = Packet->SpellID;
                                skill.Level = 0;
                            }
                        }
                        else
                        {
                            if ((Packet->UID != Client.Entity.UID ||
                                !Client.Spells.GetSkill(Packet->SpellID, out skill)))
                            {
                                Client.NetworkSocket.Disconnect();
                                return;
                            }
                        }
                        if (skill != null)
                        {
                            MAttackData* attackDataPtr = (MAttackData*)Client.MAttackDataPtr.Addr;
                            if (Packet->SpellID != MData.SpellID)
                                return; // failed verification

                            if (attackDataPtr->SpellID != skill.ID || attackDataPtr->SpellLevel != skill.Level)
                                ServerDatabase.GetMAttackData(skill.ID, skill.Level, attackDataPtr);
                            if (AutoStepped || attackDataPtr->NextSpellID != 0)
                                ApplyDelay(Client, Now);
                            else
                                ForceDelay(Client, Now.AddMilliseconds(500));

                            IBaseEntity Opponent = Client.Screen.FindObject(Packet->OpponentUID) as IBaseEntity;
                            ClearFlags(Client);
                            if (AttackProcessor.ProcessMagic(Client.Entity, Opponent, attackDataPtr, Packet->X, Packet->Y))
                            {
                                Client.AutoAttackEntity = Opponent;
                                Client.ServerFlags |= (ServerFlags.IsAutoAttacking | ServerFlags.MagicAuto);
                            }
                        }
                    }
                }
            }
        }
        public class PhysicalAttackSystem : SmartQueue<GameClient>
        {
            public PhysicalAttackSystem()
                : base(2)
            {
            }
            protected override void OnDequeue(GameClient Client)
            {
                RequestAttackPacket* Packet = (RequestAttackPacket*)Client.AutoAttackPtr.Addr;

                TIME Now = TIME.Now;
                if (Client.TimeStamps.CanAttack.Time <= Now.Time)
                {
                    Client.TimeStamps.SpawnProtection = Now;
                    ApplyDelay(Client, Now);

                    IBaseEntity Opponent = Client.Screen.FindObject(Packet->OpponentUID) as IBaseEntity;
                    MAttackData* attackDataPtr = (MAttackData*)Client.MAttackDataPtr.Addr;
                    if (attackDataPtr->SpellID != Kernel.MeeleSpellID)
                    {
                        ServerDatabase.GetMAttackData(Kernel.MeeleSpellID, 0, attackDataPtr);
                        attackDataPtr->GroundAttack = (Packet->AtkType == AttackID.Physical);
                    }

                    ClearFlags(Client);
                    if (AttackProcessor.ProcessMeele(Client.Entity, Opponent, Packet->AtkType))
                    {
                        Client.AutoAttackEntity = Opponent;
                        Client.ServerFlags |= (ServerFlags.IsAutoAttacking | ServerFlags.PhysicalAuto);
                    }
                }
            }
        }

        public static PhysicalAttackSystem Physical;
        public static MagicAttackSystem Magical;

        public static int PendingThreads
        {
            get { return Physical.Count + Magical.Count; }
        }
        static AttackSystem()
        {
            Physical = new PhysicalAttackSystem();
            Physical.Start(ThreadPriority.AboveNormal);
            Magical = new MagicAttackSystem();
            Magical.Start(ThreadPriority.AboveNormal);
        }

        public static void NotifyPhysical(GameClient Client)
        {
            Physical.Enqueue(Client);
        }
        public static void NotifyMagic(GameClient Client, ushort SpellID)
        {
            Magical.Enqueue(new MagicAttackData(SpellID, Client));
        }
    }
}
