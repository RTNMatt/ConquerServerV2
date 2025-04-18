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

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void MagicAttack(GameClient Client, ushort SpellID)
        {
            ChangeAction(Client, ConquerAction.None, null);
            AttackSystem.NotifyMagic(Client, SpellID);
        }
    }
}