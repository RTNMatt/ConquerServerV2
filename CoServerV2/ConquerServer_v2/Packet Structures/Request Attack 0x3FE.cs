using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Structures
{
    public enum AttackID : uint
    {
        None = 0x00,
        Physical = 0x02,
        Magic = 0x18,
        Archer = 0x1C,
        RequestMarriage = 0x08,
        AcceptMarriage = 0x09,
        Death = 0x0E,
    }

    public unsafe struct RequestAttackPacket
    {
        public ushort Size;
        public ushort Type;
        public TIME TimeStamp;
        public uint UID;
        public uint OpponentUID;
        public ushort X;
        public ushort Y;
        public AttackID AtkType;
        public ushort SpellID;
        public bool KilledMonster
        {
            get { return (SpellID == 1); }
            set { SpellID = (ushort)(value ? 1 : 0); }
        }
        public ushort SpellLevel;
        public ushort KillCounter
        {
            get { return SpellLevel; }
            set { SpellLevel = value; }
        }
        public int Damage
        {
            get { fixed (void* ptr = &SpellID) { return *((int*)ptr); } }
            set { fixed (void* ptr = &SpellID) { *((int*)ptr) = value; } }
        }
        public fixed sbyte TQServer[8];

        // Extra Informaiton used in re-casting (autoattack)
        public ushort AttackerX;
        public ushort AttackerY;
        public bool Aggressive;
        public bool AutoStepped;

        private uint ror32(uint value, int amount)
        {
            return ((value >> amount) | (value << (32 - amount)));
        }

        private uint rol32(uint value, int amount)
        {
            return ((value << amount) | (value >> (32 - amount)));
        }

        private ushort ror16(ushort value, int amount)
        {
            return (ushort)((value >> amount) | (value << (16 - amount)));
        }

        private ushort rol16(ushort value, int amount)
        {
            return (ushort)((value << amount) | (value >> (16 - amount)));
        }

        public void Decrypt(uint Seed, SpellCrypto Crypt)
        {

            uint uID = this.UID;
            ushort x = this.X;
            ushort y = this.Y;
            ushort spellID = this.SpellID;
            this.X = (ushort)(this.rol16((ushort)((x ^ 11990) ^ ((ushort)uID)), 1) - 8942);
            this.Y = (ushort)(this.rol16((ushort)((y ^ 47515) ^ ((ushort)uID)), 5) - 35106);
            this.SpellID = (ushort)(this.rol16((ushort)((spellID ^ 37213) ^ ((ushort)uID)), 3) - 60226);
            OpponentUID = (uint)Assembler.RollRight(OpponentUID, 13, 32);
            OpponentUID = (OpponentUID ^ 0x5F2D2463 ^ Seed) - 0x746F4AE6;
            // Crypt.Decrypt(ref UID, ref SpellID, ref X, ref Y);
        }
        public static void MoveData(void* New, void* Old)
        {
            MSVCRT.memcpy(New, Old, *((ushort*)Old) + 8);
        }
        public static RequestAttackPacket Create()
        {
            RequestAttackPacket retn = new RequestAttackPacket();
            retn.Size = 0x1C;
            retn.Type = 0x3FE;
            PacketBuilder.AppendTQServer((byte*)retn.TQServer, 8);
            return retn;
        }
    }
}