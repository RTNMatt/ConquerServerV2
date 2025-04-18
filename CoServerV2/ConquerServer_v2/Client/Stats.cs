using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ConquerServer_v2.Client
{
    [StructLayout(LayoutKind.Sequential, Size = 10)]
    public struct StatData
    {
        public ushort Strength;
        public ushort Agility;
        public ushort Vitality;
        public ushort Spirit;
        public ushort StatPoints;
    }
}
