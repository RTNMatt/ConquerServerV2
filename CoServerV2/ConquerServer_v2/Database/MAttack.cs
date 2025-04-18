using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Database
{
    /* Special Thanks TQ (: */
    public enum MagicSort
    {
        ATTACK = 1,
        RECRUIT = 2,			// support auto active.
        CROSS = 3,
        FAN = 4,			// support auto active(random).
        BOMB = 5,
        ATTACHSTATUS = 6,
        DETACHSTATUS = 7,
        SQUARE = 8,
        JUMPATTACK = 9,			// move, a-lock
        RANDOMTRANS = 10,			// move, a-lockf
        DISPATCHXP = 11,
        COLLIDE = 12,			// move, a-lock & b-synchro
        SERIALCUT = 13,			// auto active only.
        LINE = 14,			// support auto active(random).
        ATKRANGE = 15,			// auto active only, forever active.
        ATKSTATUS = 16,			// support auto active, random active.
        CALLTEAMMEMBER = 17,
        RECORDTRANSSPELL = 18,
        TRANSFORM = 19,
        ADDMANA = 20,			// support self target only.
        LAYTRAP = 21,
        DANCE = 22,			// ÌøÎè(only use for client)
        CALLPET = 23,			// ÕÙ»½ÊÞ
        VAMPIRE = 24,			// ÎüÑª£¬power is percent award. use for call pet
        INSTEAD = 25,			// ÌæÉí. use for call pet
        DECLIFE = 26,			// ¿ÛÑª(µ±Ç°ÑªµÄ±ÈÀý)
        GROUNDSTING = 27,			// µØ´Ì
        REBORN = 28,			// ¸´»î -- zlong 2004.5.14
        TEAM_MAGIC = 29,			// ½ç½áÄ§·¨¡ª¡ª ÓëATTACHSTATUSÏàÍ¬´¦Àí£¬
        //				ÕâÀï¶ÀÁ¢·ÖÀàÖ»ÊÇÎªÁË·½±ã¿Í»§¶ËÊ¶±ð
        BOMB_LOCKALL = 30,			// ÓëBOMB´¦ÀíÏàÍ¬£¬Ö»ÊÇËø¶¨È«²¿Ä¿±ê
        SORB_SOUL = 31,			// Îü»êÄ§·¨
        STEAL = 32,			// ÍµµÁ£¬Ëæ»ú´ÓÄ¿±êÉíÉÏÍµÈ¡power¸öÎïÆ·
        LINE_PENETRABLE = 33,			// ¹¥»÷Õß¹ì¼£¿ÉÒÔ´©ÈËµÄÏßÐÔ¹¥»÷

        //////////////////////////////////////////////
        // ÐÂÔö»ÃÊÞÄ§·¨ÀàÐÍ
        BLAST_THUNDER = 34,			// Ä§À×
        MULTI_ATTACHSTATUS = 35,			// ÈºÌåÊ©¼Ó×´Ì¬
        MULTI_DETACHSTATUS = 36,			// ÈºÌå½â³ý×´Ì¬
        MULTI_CURE = 37,			// ÈºÌå²¹Ñª
        STEAL_MONEY = 38,			// ÍµÇ®
        KO = 39,			// ±ØÉ±¼¼£¬Ä¿±êÑªÐ¡ÓÚ15%×Ô¶¯´¥·¢
        ESCAPE = 40,			// ÌÓÅÜ/¾ÈÖú
        //		FLASH_ATTACK			= 41,			// ÒÆÐÎ»»Î»		
    }

    public enum MAttackTargetType : byte
    {
        WeaponSkill = 8,
        Physical = 4,
        BombMagic = 2,
        MagicHeal = 1,
        Magic = 0
    }

    public unsafe struct MAttackData
    {
        public ushort SpellID;
        public ushort Mana;
        public ushort Weapon;
        public ushort SpellLevel;
        public sbyte Stamina;
        public sbyte Range;
        public sbyte Distance;
        public byte SuccessRate;
        public MAttackTargetType TargetType;
        public MagicSort Sort;
        public int BaseDamage;
        public bool Aggressive;
        public bool MultipleTargets;
        public bool IsXPSkill;
        public ushort SecondsTimer;
        public ushort NextSpellID;
        public uint Experience;
        public bool GroundAttack;
        public double BaseDamagePercent
        {
            get { return Math.Max(BaseDamage - 30000, 0) * 0.01; }
        }
        public string GetName()
        {
            IniFile rdr = new IniFile(ServerDatabase.Path + "\\Spells\\" + SpellID.ToString() + "[" + SpellLevel.ToString() + "].ini");
            return rdr.ReadString("SpellInformation", "Name", "", 32);
        }
    }
}
