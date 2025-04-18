using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConquerServer_v2.Core;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Monster_AI
{
    /// <summary>
    /// A monster family is used to represent a certain spawn, not only one type of monster.
    /// Monsters of this single spawn are considered as a family.
    /// </summary>
    public class MonsterFamily
    {
        public string Name;
        public int MaxAttack, MinAttack;
        public int MaxHealth;
        public ushort Defense;
        public ushort Mesh;
        public ushort Level;
        public byte ViewRange;
        public sbyte AttackRange;
        public sbyte Dodge;
        public uint ID;

        public byte DropBoots;
        public byte DropArmor;
        public byte DropShield;
        public byte DropWeapon;
        public byte DropArmet;
        public byte DropRing;
        public byte DropNecklace;
        public ushort DropMoney;
        public uint DropHPItem;
        public uint DropMPItem;
        public SpecialItemWatcher[] DropSpecials;

        public MonsterSettings Settings;
        public MobItemGenerator ItemGenerator;
        private static Dictionary<uint, MobItemGenerator> ItemGeneratorLinker;
        static MonsterFamily()
        {
            ItemGeneratorLinker = new Dictionary<uint, MobItemGenerator>();
        }

        /// <summary>
        /// Creates the settings for this family based off the name
        /// </summary>
        public void CreateMonsterSettings()
        {
            Settings = MonsterSettings.Standard;
            if (Name.Contains("Guard"))
                Settings = MonsterSettings.Guard;
            else if (Name.Contains("Reviver"))
                Settings = MonsterSettings.Reviver;
            else if (Name.Contains("King"))
                Settings = MonsterSettings.King;
            else if (Name.Contains("Messenger"))
                Settings = MonsterSettings.Messenger;
        }
        /// <summary>
        /// Creates, or obtains a item-generator instance by the ID variable of this generation.
        /// </summary>
        public void CreateItemGenerator()
        {
            if (!ItemGeneratorLinker.TryGetValue(ID, out ItemGenerator))
            {
                ItemGenerator = new MobItemGenerator(this);
                ItemGeneratorLinker.Add(ID, ItemGenerator);
            }
        }
    }
}