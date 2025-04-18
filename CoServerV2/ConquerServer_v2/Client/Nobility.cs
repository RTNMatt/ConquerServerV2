using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Client
{
    public enum NobilityID : uint
    {
        None = 0x00,
        Knight = 0x01,
        Baron = 0x03,
        Earl = 0x05,
        Duke = 0x07,
        Prince = 0x09,
        King = 0x0C
    }
}
