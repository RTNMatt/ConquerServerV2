using System;
using System.Collections;
using System.Collections.Generic;
using ConquerServer_v2.Database;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Attack_Processor
{
    public unsafe partial class AttackProcessor
    {
        public const double StigmaPercent = 1.20;
        public static void Stigma(GameClient Attacker, IBaseEntity Opponent, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            if (Opponent != null)
            {
                if (Opponent.EntityFlag == EntityFlag.Player)
                {
                    GameClient OpponentClient = Opponent.Owner as GameClient;
                    OpponentClient.TimeStamps.StigmaFinish = TIME.Now.AddSeconds(Data->SecondsTimer);
                    OpponentClient.Entity.Spawn.StatusFlag |= StatusFlag.Stigma;

                    UpdatePacket status = UpdatePacket.Create();
                    status.UID = Opponent.UID;
                    status.ID = UpdateID.RaiseFlag;
                    status.BigValue = Opponent.StatusFlag;
                    SendRangePacket.Add(Opponent, Kernel.ViewDistance, 0, Kernel.ToBytes(&status), null);
                }
                Targets = new Dictionary<IBaseEntity, Damage>();
                Targets.Add(Opponent, new Damage(0, 0));
            }
        }
    }
}