using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;
using ConquerScriptLinker;

namespace ConquerServer_v2.Client
{
    public interface ISkill
    {
        ushort ID { get; set; }
        ushort Level { get; set; }
        int Experience { get; set; }
        int NeededExperience { get; }
        bool MaxLevel { get; }

        void Send(GameClient Client);
    }

    public unsafe class Spell : ISkill, INpcSkill
    {
        private SpellPacket Skill;
        private ushort m_MaxLevel;
        private ushort m_NeededLevel;
        private int m_NeededExperience;

        private void UpdateSkillInformation()
        {
            IniFile rdr = new IniFile(ServerDatabase.Path + "\\Spells\\" + ID.ToString() + "[" + Level.ToString() + "].ini");
            m_NeededExperience = rdr.ReadInt32("SpellInformation", "Experience", 0);
            m_NeededLevel = rdr.ReadUInt16("SpellInformation", "ReqLevel", 0);
            if (m_NeededExperience != 0)
                m_MaxLevel = (ushort)(Level + 1);
        }
        public ushort ID { get { return (ushort)Skill.ID; } set { Skill.ID = value; } }
        public ushort Level { get { return (ushort)Skill.Level; } set { Skill.Level = value; UpdateSkillInformation(); } }
        public int Experience { get { return Skill.Experience; } set { Skill.Experience = value; } }
        public bool MaxLevel { get { return (m_MaxLevel <= Skill.Level); } }
        public int NeededExperience { get { return m_NeededExperience; } }
        public ushort NeededLevel { get { return m_NeededLevel; } }
        public void Send(GameClient Client)
        {
            fixed (SpellPacket* ptr = &Skill)
            {
                Client.Send(ptr);
            }
        }
        public void Send(INpcPlayer Client)
        {
            fixed (SpellPacket* ptr = &Skill)
            {
                Client.Send(ptr);
            }
        }

        public Spell() { Skill = SpellPacket.Create(); }


        public static ushort[] GetRebornSpells(byte Job, byte RebornInto)
        {
            ushort[] Skills = null;
            Job /= 10;
            RebornInto /= 10;
            switch (Job)
            {
                case 1: // trojan
                    switch (RebornInto)
                    {
                        case 5:
                        case 4: Skills = new ushort[] { 1110, 1190 }; break;
                        case 14: Skills = new ushort[] { 1110, 1190, 1270 }; break;
                        case 1: Skills = new ushort[] { 3050 }; break;
                        case 2: Skills = new ushort[] { 1110, 1190, 5100 }; break;
                    }
                    break;
                case 2: // warrior
                    switch (RebornInto)
                    {
                        case 4:
                        case 14: Skills = new ushort[] { 1020, 1040 }; break;
                        case 5:
                        case 1: Skills = new ushort[] { 1040, 1015, 1320 }; break;
                        case 2: Skills = new ushort[] { 3060 }; break;
                        case 13: Skills = new ushort[] { 1020, 1040, 1025 }; break;
                    }
                    break;
                case 4: // archer
                    switch (RebornInto)
                    {
                        case 4: Skills = new ushort[] { 5000 }; break;
                    }
                    break;
                case 5: // ninja
                    switch (RebornInto)
                    {
                        case 5: Skills = new ushort[] { 6000, 6001, 6002, 6003, 6010, 6011 }; break;
                        default: Skills = new ushort[] { 6001 }; break;
                    }
                    break;
                case 13: // water tao
                    switch (RebornInto)
                    {
                        case 13: Skills = new ushort[] { 3090 }; break;
                        case 2:
                        case 5:
                        case 1: Skills = new ushort[] { 1005, 1090, 1095, 1195, 1085 }; break;
                        case 14: Skills = new ushort[] { 1050, 1175, 1075, 1055 }; break;
                    }
                    break;
                case 14: // fire tao
                    switch (RebornInto)
                    {
                        case 14: Skills = new ushort[] { 3080 }; break;
                        case 13: Skills = new ushort[] { 1120 }; break;
                        case 1:
                        case 2:
                        case 5:
                        case 4: Skills = new ushort[] { 1000, 1001, 1005, 1195 }; break;
                    }
                    break;
            }
            return Skills;
        }
        private static ushort[] Get2ndSurplus(byte Original, byte OldJob, byte RebornInto)
        {
            switch (OldJob)
            {
                case 2:
                    return new ushort[] { 3060, 9876 };
            }
            return new ushort[] { 9876 };
        }
        public static ushort[] Get2ndRebornSpells(byte Original, byte OldJob, byte RebornInto)
        {
            ushort[] Skills;
            ushort[] head = GetRebornSpells(Original, OldJob);
            ushort[] foot = GetRebornSpells(OldJob, RebornInto);
            ushort[] surp = Get2ndSurplus(Original, OldJob, RebornInto);

            Skills = new ushort[head.Length + foot.Length + surp.Length];
            head.CopyTo(Skills, 0);
            foot.CopyTo(Skills, head.Length);
            surp.CopyTo(Skills, head.Length + foot.Length);

            return Skills;
        }
    }

    public unsafe class Proficiency : ISkill, INpcSkill
    {
        private static int[] m_NeededExperience = {
            0,
            1200,
            68000, 
            250000, 
            640000,
            1600000, 
            4000000, 
            10000000, 
            22000000, 
            40000000, 
            90000000, 
            95000000, 
            142500000, 
            213750000, 
            320625000, 
            480937500, 
            721406250, 
            1082109375, 
            1623164063, 
            2100000000 
        };

        private ProficiencyPacket Skill;
        public ushort ID { get { return (ushort)Skill.ID; } set { Skill.ID = value; } }
        public ushort Level { get { return (ushort)Skill.Level; } set { Skill.Level = value; } }
        public bool MaxLevel { get { return Level >= 20; } }
        public int Experience { get { return Skill.Experience; } set { Skill.Experience = value; } }
        public int NeededExperience 
        { 
            get 
            {
                if (Level < 20)
                    return m_NeededExperience[Level];
                return 0;
            } 
        }
        public void Send(GameClient Client)
        {
            fixed (ProficiencyPacket* ptr = &Skill)
            {
                Client.Send(ptr);
            }
        }
        public void Send(INpcPlayer Client)
        {
            fixed (ProficiencyPacket* ptr = &Skill)
            {
                Client.Send(ptr);
            }
        }

        public Proficiency() { Skill = ProficiencyPacket.Create(); }
    }

    public static class SkillExtensions
    {
        public static bool GetSkill(this FlexibleArray<ISkill> Skills, ushort ID, out ISkill find)
        {
            return (GetSkillIdx(Skills, ID, out find) != -1);
        }
        public static int GetSkillIdx(this FlexibleArray<ISkill> Skills, ushort ID, out ISkill find)
        {
            find = null;
            for (int i = 0; i < Skills.Length; i++)
            {
                if (Skills.Elements[i].ID == ID)
                {
                    find = Skills.Elements[i];
                    return i;
                }
            }
            return -1;
        }
    }
}
