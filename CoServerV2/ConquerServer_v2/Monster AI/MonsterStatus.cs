using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Monster_AI
{
    public enum MonsterStatus
    {
        Idle = 1,
        Roam = 2,
        Targetting = 3,
        Attacking = 4,
        Respawning = 5
    }
}
