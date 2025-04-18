using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;

namespace ConquerServer_v2.Packet_Structures
{
    public unsafe struct SpawnNpcPacket 
    {
        public ushort Size;
        public ushort Type;
        public uint UID;
        public uint UID2;
        public ushort X;
        public ushort Y;
        public ushort NpcType;
        public ushort Interaction;
        public uint Flag;
        public byte NameLength
        {
            get { fixed (uint* lp_Flag = &Flag) { return *(((byte*)lp_Flag) + 3); } }
            set { fixed (uint* lp_Flag = &Flag) { *(((byte*)lp_Flag) + 3) = value; } }
        }
        public bool IsVendor
        {
            get { fixed (uint* lp_Flag = &Flag) { return *(((bool*)lp_Flag) + 2); } }
            set { fixed (uint* lp_Flag = &Flag) { *(((bool*)lp_Flag) + 2) = value; } }
        }
        private fixed byte Strings[24];

        public static SpawnNpcPacket Create()
        {
            SpawnNpcPacket packet = new SpawnNpcPacket();
            packet.Size = 0x18;
            packet.Type = 0x7EE;
            PacketBuilder.AppendTQServer(packet.Strings, 8);
            return packet;
        }
        public void ConvertToVendor(string Name)
        {
            Size = (ushort)(0x15 + Name.Length);
            Flag = 0;
            NameLength = (byte)Name.Length;
            IsVendor = true;
            NpcType = 0x196;
            Interaction = 0x0E;
            X += 3;
            fixed (byte* _Strings = Strings)
            {
                MSVCRT.memset(_Strings, 0, 24);
                Name.CopyTo(_Strings);
                PacketBuilder.AppendTQServer(_Strings + Name.Length + 1, 8);
            }
        }
        public void ConvertToStandard()
        {
            Size = 0x14;
            Flag = 0;
            NpcType = 0x43E;
            X -= 3;
            Interaction = 0x10;
            fixed (byte* _Strings = Strings)
            {
                MSVCRT.memset(_Strings, 0, 24);
                PacketBuilder.AppendTQServer(_Strings, 8);
            }
        }
    }
}
