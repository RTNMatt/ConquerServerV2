using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Database
{
    public unsafe struct DatabaseSkill
    {
        public ushort ID;
        public ushort Level;
        public int Experience;

        public ISkill GetSpell()
        {
            Spell skill = new Spell();
            skill.ID = ID;
            skill.Level = Level;
            skill.Experience = Experience;
            return skill;
        }
        public ISkill GetProficiency()
        {
            Proficiency skill = new Proficiency();
            skill.ID = ID;
            skill.Level = Level;
            skill.Experience = Experience;
            return skill;
        }
        public void FromSkill(ISkill skill)
        {
            ID = skill.ID;
            Level = skill.Level;
            Experience = skill.Experience;
        }
    }
}
