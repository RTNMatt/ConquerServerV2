using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Database
{
    public struct StanderdItemStats
    {
        private IniFile ini;
        public StanderdItemStats(uint ItemID)
        {
            ini = new IniFile(ServerDatabase.Path + "\\Items\\" + ItemID.ToString() + ".ini");
        }
        public StanderdItemStats(uint ItemID, IniFile rdr)
        {
            ini = rdr;
            ini.FileName = ServerDatabase.Path + "\\Items\\" + ItemID.ToString() + ".ini";
        }
        public StanderdItemStats(uint ItemID, out IniFile rdr)
        {
            ini = new IniFile(ServerDatabase.Path + "\\Items\\" + ItemID.ToString() + ".ini");
            rdr = ini;
        }

        public uint ItemID { get { return ini.ReadUInt32("ItemInformation", "ItemID", 0); } }
        public string Name { get { return ini.ReadString("ItemInformation", "ItemName", "INVALID_ITEM", 16); } }
        public int MinAttack { get { return ini.ReadInt32("ItemInformation", "MinPhysAtk", 0); } }
        public int MaxAttack { get { return ini.ReadInt32("ItemInformation", "MaxPhysAtk", 0); } }
        public ushort PhysicalDefence { get { return ini.ReadUInt16("ItemInformation", "PhysDefence", 0); } }
        public ushort MDefence { get { return ini.ReadUInt16("ItemInformation", "MDefence", 0); } }
        public sbyte Dodge { get { return ini.ReadSByte("ItemInformation", "Dodge", 0); } }
        public int MAttack { get { return ini.ReadInt32("ItemInformation", "MAttack", 0); } }
        public short HP { get { return ini.ReadInt16("ItemInformation", "PotAddHP", 0); } }
        public short MP { get { return ini.ReadInt16("ItemInformation", "PotAddMP", 0); } }
        public int Frequency { get { return ini.ReadInt32("ItemInformation", "Frequency", 0); } }
        public sbyte AttackRange { get { return ini.ReadSByte("ItemInformation", "Range", 0); } }
        public ushort Dexerity { get { return ini.ReadUInt16("ItemInformation", "Dexerity", 0); } }
        public short Durability { get { return ini.ReadInt16("ItemInformation", "Durability", 0); } }
        public short ReqProfLvl { get { return ini.ReadInt16("ItemInformation", "ReqProfLvl", 0); } }
        public short ReqStr { get { return ini.ReadInt16("ItemInformation", "ReqStr", 0); } }
        public short ReqAgi { get { return ini.ReadInt16("ItemInformation", "ReqAgi", 0); } }
        public short ReqLvl { get { return ini.ReadInt16("ItemInformation", "ReqLvl", 0); } }
        public short ReqJob { get { return ini.ReadInt16("ItemInformation", "ReqJob", 0); } }
        public short ReqSex { get { return ini.ReadInt16("ItemInformation", "ReqSex", 0); } }
        public int MoneyPrice { get { return ini.ReadInt32("ItemInformation", "ShopBuyPrice", 0); } }
        public int ConquerPointsPrice { get { return ini.ReadInt32("ItemInformation", "ShopCPPrice", 0); } }
    }
}
