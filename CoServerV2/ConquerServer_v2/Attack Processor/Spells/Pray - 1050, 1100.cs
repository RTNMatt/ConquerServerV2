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
        public static void Pray(GameClient Attacker, IBaseEntity Opponent, ref Dictionary<IBaseEntity, Damage> Targets)
        {
            if (Opponent != null)
            {
                if (Opponent.EntityFlag == EntityFlag.Player && Opponent.Dead)
                {
                    if (AttackProcessor.SafeHeal(Attacker.Entity, Opponent, (sbyte)Kernel.ViewDistance, false) == 0)
                    {
                        (Opponent.Owner as GameClient).RevivePlayer(true);
                    }
                }
                Targets = new Dictionary<IBaseEntity, Damage>();
                Targets.Add(Opponent, new Damage(0, 0));
            }
        }
    }
}