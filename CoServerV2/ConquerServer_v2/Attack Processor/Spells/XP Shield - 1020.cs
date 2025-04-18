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
        public static void XPShield(GameClient Attacker, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            Attacker.TimeStamps.XPShieldFinish = WinMM.timeGetTime().AddSeconds(Data->SecondsTimer);
            Attacker.Entity.Spawn.StatusFlag |= StatusFlag.PurpleShield;

            UpdatePacket status = UpdatePacket.Create();
            status.UID = Attacker.Entity.UID;
            status.ID = UpdateID.RaiseFlag;
            status.BigValue = Attacker.Entity.StatusFlag;
            SendRangePacket.Add(Attacker.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&status), null);

            Targets = new Dictionary<IBaseEntity, Damage>();
            Targets.Add(Attacker.Entity, new Damage(0, 0));
        }
    }
}