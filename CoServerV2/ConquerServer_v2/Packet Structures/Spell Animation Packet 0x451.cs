using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Core;
using ConquerServer_v2.Attack_Processor;

namespace ConquerServer_v2.Packet_Structures
{
    public unsafe class SpellAnimationPacket
    {
        public ushort Level;
        public ushort X, Y, SpellID;
        public uint AttackerUID;
        public Dictionary<IBaseEntity, Damage> Targets;
        public SpellAnimationPacket(
            ushort _SpellID, ushort _SpellLvl,
            ushort _X, ushort _Y,
            uint _AttackerUID,
            Dictionary<IBaseEntity, Damage> _Targets)
        {
            SpellID = _SpellID;
            Level = _SpellLvl;
            X = _X;
            Y = _Y;
            AttackerUID = _AttackerUID;
            Targets = _Targets;
        }
        public SpellAnimationPacket() { }
        public static implicit operator byte[](SpellAnimationPacket MAttack)
        {
            byte[] Packet = new byte[0x20 + (MAttack.Targets.Count * 12) + 8];
            fixed (byte* Pointer = Packet)
            {
                *((ushort*)(Pointer + 0)) = (ushort)(Packet.Length - 8);
                *((ushort*)(Pointer + 2)) = 0x451;
                *((uint*)(Pointer + 4)) = MAttack.AttackerUID;
                *((ushort*)(Pointer + 8)) = MAttack.X;
                *((ushort*)(Pointer + 10)) = MAttack.Y;
                *((ushort*)(Pointer + 12)) = MAttack.SpellID;
                *((ushort*)(Pointer + 14)) = MAttack.Level;
                *((uint*)(Pointer + 16)) = (uint)MAttack.Targets.Count;
                ushort ax = 0;
                if (MAttack.Targets != null)
                {
                    foreach (KeyValuePair<IBaseEntity, Damage> DE in MAttack.Targets)
                    {
                        *((uint*)(Pointer + 20 + ax)) = DE.Key.UID;
                        *((int*)(Pointer + 24 + ax)) = DE.Value.Show;
                        ax += 12;
                    }
                }
                PacketBuilder.AppendTQServer(Pointer, Packet.Length);
            }
            return Packet;
        }
    }
}
