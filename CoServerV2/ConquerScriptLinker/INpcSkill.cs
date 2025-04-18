using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerScriptLinker
{
    public unsafe interface INpcSkill
    {
        ushort Level { get; set; }
        ushort ID { get; }
        int Experience { get; set; }
        bool MaxLevel { get; }
        void Send(INpcPlayer Player);
    }
}
