using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Database
{

    public struct MapStatusFlag
    {
        private const uint
            C_Normal = 0,
            C_PKField = 1,
            C_ChangeMapDisabled = 2,
            C_SaveLocationDisabled = 4,
            C_PKDisabled = 8,
            C_Vending = 16,
            C_TeamDisabled = 32,
            C_TeleportDisabled = 64,
            C_GuildWarMap = 128,
            C_PrisonMap = 256,
            C_FlyDisabled = 512,
            C_HouseMap = 1024,
            C_Mining = 2048,
            C_NeverWound = 8192,
            C_DeadIslandMap = 16384; // ?

        private IniFile ini;
        private uint m_Bitfield;
        public MapStatusFlag(IniFile linker)
        {
            ini = linker;
            m_Bitfield = ini.ReadUInt32("Standerd", "Status", 0);
        }
        public uint Bitfield
        {
            get { return m_Bitfield; }
            set
            {
                m_Bitfield = value;
                ini.Write<uint>("Standerd", "Status", m_Bitfield);
            }
        }
        public bool PKing
        {
            get { return !(Bitfield.CheckBitFlag(C_PKDisabled)); }
            set
            {
                const uint val = C_PKDisabled;
                if (!value) Bitfield |= val;
                else Bitfield &= ~val;
            }
        }
        public bool CanSaveLocation
        {
            get { return !Bitfield.CheckBitFlag(C_SaveLocationDisabled); }
            set
            {
                const uint val = C_SaveLocationDisabled;
                if (!value) Bitfield |= val;
                else Bitfield &= ~val;
            }
        }
        public bool PKProtection
        {
            get { return Bitfield.CheckBitFlag(C_NeverWound); }
            set
            {
                const uint val = C_NeverWound;
                if (value) Bitfield |= val;
                else Bitfield &= ~val;
            }
        }
        public bool FlyDisabled
        {
            get { return Bitfield.CheckBitFlag(C_FlyDisabled); }
            set
            {
                const uint val = C_FlyDisabled;
                if (value) Bitfield |= val;
                else Bitfield &= ~val;
            }
        }
        public bool CanVend
        {
            get { return Bitfield.CheckBitFlag(C_Vending); }
            set
            {
                const uint val = C_Vending;
                if (value) Bitfield |= val;
                else Bitfield &= ~val;
            }
        }
        public bool CanGainPKPoints
        {
            get
            {
                return ((Bitfield & C_PKField) != C_PKField) && // not a PK Map
                        ((Bitfield & C_PrisonMap) != C_PrisonMap) && // not a Prison
                        ((Bitfield & C_GuildWarMap) != C_GuildWarMap); // not GW Map
            }
        }

        public static implicit operator MapStatusFlag(uint value)
        {
            MapStatusFlag flag = new MapStatusFlag();
            flag.Bitfield = value;
            return flag;
        }
        public static implicit operator uint(MapStatusFlag value)
        {
            return value.Bitfield;
        }
    }

    public struct MapSettings
    {
        private const string Std = "Standerd";
        private const string RevivePt = "RevivePoint";

        private IniFile ini;
        public MapStatusFlag Status;

        public MapSettings(uint MapID)
        {
            ini = new IniFile(ServerDatabase.Path + "\\Map Settings\\" + MapID.ToString() + ".ini");
            Status = new MapStatusFlag(ini);
        }
        public MapSettings(uint MapID, out IniFile rdr)
        {
            ini = new IniFile(ServerDatabase.Path + "\\Map Settings\\" + MapID.ToString() + ".ini");
            rdr = ini;
            Status = new MapStatusFlag(ini);
        }
        public MapSettings(ushort MapID, IniFile rdr)
        {
            ini = rdr;
            ini.FileName = ServerDatabase.Path + "\\Map Settings\\" + MapID.ToString() + ".ini";
            Status = new MapStatusFlag(ini);
        }

        // --- CQ_Map ---
        public ushort MapID
        {
            get { return ini.ReadUInt16(Std, "ID", 0); }
        }
        public uint Weather
        {
            get { return ini.ReadUInt32(Std, "Weather", 0); }
            set { ini.Write<uint>(Std, "Weather", value); }
        }
        public byte ReqEnterLvl
        {
            get { return ini.ReadByte(Std, "ReqEnterLvl", 0); }
            set { ini.Write<byte>(Std, "ReqEnterLvl", value); }
        }
        public uint Color
        {
            get { return ini.ReadUInt32(Std, "Color", 0); }
            set { ini.Write<uint>(Std, "Color", value); }
        }

        /// <summary>
        /// Returns a 3-indexed (0,1,2) ushort array containing in the following sequence: 
        /// MapID, X, Y.
        /// </summary>
        public uint[] RevivePoint
        {
            get
            {
                uint[] location = new uint[3];
                string[] query = null;
                uint target_map = MapID;
                do
                {
                    const int str_BufferSize = (IniFile.Int16_Size * 3) + 3;
                    const string str_Default = "1002 400 400";

                    query = ServerDatabase.RevivePoints.ReadString(target_map.ToString(), "Value", str_Default, str_BufferSize).Split(' ');
                    if (query[0] == "LINKED")
                    {
                        query = ServerDatabase.RevivePoints.ReadString(query[1], "Value", str_Default, str_BufferSize).Split(' ');
                    }
                }
                while (query[0] == "LINKED");

                try
                {
                    location[0] = uint.Parse(query[0]);
                    location[1] = ushort.Parse(query[1]);
                    location[2] = ushort.Parse(query[2]);
                }
                catch (IndexOutOfRangeException)
                {
                    location = new uint[] { 1002, 400, 400 };
                }
                catch (FormatException)
                {
                    location = new uint[] { 1002, 400, 400 };
                }
                return location;
            }
        }
    }
}