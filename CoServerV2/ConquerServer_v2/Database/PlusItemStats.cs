using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Database
{
    public struct PlusItemStats
    {
        public static string GetBaseID(uint ID)
        {
            int itemtype = (int)(ID / 1000);
            switch ((byte)(ID / 10000))
            {
                case 11:
                case 90:
                case 13:
                    {
                        if (itemtype == 135)
                            goto case 12; // ninja armor's use this algorithm

                        ID = (uint)(
                                (((uint)(ID / 1000)) * 1000) + // [3] = 0
                                ((ID % 100) - (ID % 10)) // [5] = 0
                            );
                        break;
                    }
                case 12:
                case 15:
                case 16:
                case 50:
                    {
                        ID = (uint)(
                                ID - (ID % 10) // [5] = 0
                            );
                        break;
                    }
                default:
                    {
                        if (itemtype == ItemTypeConst.BackswordID || itemtype == ItemTypeConst.NinjaSwordID)
                        {
                            ID = (uint)(
                                ID - (ID % 10) // [5] = 0
                            );
                        }
                        else
                        {
                            byte head = (byte)(ID / 100000);
                            ID = (uint)(
                                    ((head * 100000) + (head * 10000) + (head * 1000)) + // [1] = [0], [2] = [0]
                                    ((ID % 1000) - (ID % 10)) // [5] = 0
                                );
                        }
                        break;
                    }
            }
            return ID.ToString();
        }

        public const string Section = "ItemInformation";
        private IniFile ini;
        public PlusItemStats(uint ItemID, byte Plus)
        {
            ini = new IniFile(ServerDatabase.Path + "\\PItems\\" + GetBaseID(ItemID) + "[" + Plus.ToString() + "].ini");
        }
        public PlusItemStats(uint ItemID, byte Plus, IniFile rdr)
        {
            ini = rdr;
            ini.FileName = ServerDatabase.Path + "\\PItems\\" + GetBaseID(ItemID) + "[" + Plus.ToString() + "].ini";
        }
        public int MinAttack { get { return ini.ReadInt32("ItemInformation", "MinAttack", 0); } }
        public int MaxAttack { get { return ini.ReadInt32("ItemInformation", "MaxAttack", 0); } }
        public int MAttack { get { return ini.ReadInt32("ItemInformation", "MAttack", 0); } }
        public short PhysicalDefence { get { return ini.ReadInt16("ItemInformation", "PhysDefence", 0); } }
        public sbyte Dodge { get { return ini.ReadSByte("ItemInformation", "Dodge", 0); } }
        public short PlusMDefence { get { return ini.ReadInt16("ItemInformation", "MDefence", 0); } }
        public short HP { get { return ini.ReadInt16("ItemInformation", "HP", 0); } }
    }
}
