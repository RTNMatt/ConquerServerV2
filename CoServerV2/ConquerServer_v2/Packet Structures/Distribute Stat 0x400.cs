using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public unsafe struct DistributeStatPacket
    {
        public ushort Size;
        public ushort Type;
        public bool Strength;
        public bool Agility;
        public bool Vitality;
        public bool Spirit;
    }
}