using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Client
{
    public enum PKMode : byte
    {
        Kill = 0x00,
        Peace = 0x01,
        Team = 0x02,
        Capture = 0x03
    }
}
