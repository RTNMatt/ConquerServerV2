using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Attack_Processor
{
    public struct Damage
    {
        public int Experience;
        public int Show;
        public Damage(int show, int exp)
        {
            Show = show;
            Experience = exp;
        }
    }
}
