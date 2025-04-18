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
        public static void Meditation(GameClient Attacker, MAttackData* Data, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            Attacker.Manapoints = (ushort)Math.Min(Attacker.MaxManapoints, Attacker.Manapoints + Data->BaseDamage);
            UpdatePacket Update = UpdatePacket.Create();
            Update.UID = Attacker.Entity.UID;
            Update.ID = UpdateID.Mana;
            Update.Value = (uint)Attacker.Manapoints;
            Attacker.Send(&Update);

            Targets = new Dictionary<IBaseEntity, Damage>();
            Targets.Add(Attacker.Entity, new Damage(Data->BaseDamage, Data->BaseDamage));
        }
    }
}