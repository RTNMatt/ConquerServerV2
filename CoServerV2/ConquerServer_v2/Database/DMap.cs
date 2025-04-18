using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ConquerServer_v2.Core;
using ConquerServer_v2.Monster_AI;

namespace ConquerServer_v2.Database
{
    [Flags]
    public enum DMapCellFlag : ushort
    {
        Invalid = 0x01,
        MonsterOnTile = 0x02,
        ItemOnTile = 0x04
    }

    public unsafe class DataMap
    {
        private MapID m_MapID;
        private DMapCellFlag[,] BinaryData;
        private int m_MaxX;
        private int m_MaxY;
        
        public DictionaryV2<uint, NpcEntity> Npcs;
        public MobCollection Mobs;
        public DictionaryV2<uint, IDroppedItem> DroppedItems;

        public int MaxX { get { return m_MaxX; } }
        public int MaxY { get { return m_MaxY; } }
        public MapID MapID { get { return m_MapID; } }
        public bool HasMobs { get { return Mobs != null; } }

        private void Init()
        {
            Npcs = new DictionaryV2<uint, NpcEntity>();
            DroppedItems = new DictionaryV2<uint, IDroppedItem>();
        }

        public DataMap(string FilePath, uint MapID)
        {
            if (File.Exists(FilePath))
            {
                using (BinaryReader rdr = new BinaryReader(new FileStream(FilePath, FileMode.Open)))
                {
                    m_MaxX = rdr.ReadInt32();
                    m_MaxY = rdr.ReadInt32();
                    BinaryData = new DMapCellFlag[MaxX, MaxY];
                    for (int x = 0; x < MaxX; x++)
                    {
                        for (int y = 0; y < MaxY; y++)
                        {
                            if (rdr.ReadBoolean())
                                BinaryData[x, y] |= DMapCellFlag.Invalid;
                        }
                    }
                }
                m_MapID = MapID;
                Init();
            }
            else
            {
                throw new ArgumentException("DMap not found", "FilePath");
            }
        }
        public void SetInvalid(ushort X, ushort Y, bool Value)
        {
            if (Value)
                BinaryData[X, Y] |= DMapCellFlag.Invalid;
            else
                BinaryData[X, Y] &= ~DMapCellFlag.Invalid;
        }
        public bool InvalidNoHandicap(ushort X, ushort Y)
        {
            if (MaxX > X && MaxY > Y)
            {
                return (BinaryData[X, Y] & DMapCellFlag.Invalid) == DMapCellFlag.Invalid;
            }
            return true;
        }
        public bool Invalid(ushort X, ushort Y)
        {
            if (MaxX > X && MaxY > Y)
            {
                if (((BinaryData[X, Y] & DMapCellFlag.Invalid) == DMapCellFlag.Invalid))
                {
                    int tryx = X - 2;
                    int tryy = Y - 2;
                    if (tryx > 0 && tryy > 0)
                    {
                        for (; tryx < X + 2; tryx++)
                        {
                            for (; tryy < Y + 2; tryy++)
                            {
                                if (tryx > MaxX)
                                    return true;
                                if (tryy > MaxY)
                                    return true;
                                if ((BinaryData[tryx, tryy] & DMapCellFlag.Invalid) != DMapCellFlag.Invalid)
                                    return false;
                            }
                        }
                    }
                    return true;
                }
                return false;
            }
            return true;
        }
        public void SetMonsterOnTile(ushort X, ushort Y, bool Value)
        {
            if (Value)
                BinaryData[X, Y] |= DMapCellFlag.MonsterOnTile;
            else
                BinaryData[X, Y] &= ~DMapCellFlag.MonsterOnTile;
        }
        public bool MonsterOnTile(ushort X, ushort Y)
        {
            if (MaxX > X && MaxY > Y)
            {
                return (BinaryData[X, Y] & DMapCellFlag.MonsterOnTile) == DMapCellFlag.MonsterOnTile;
            }
            return false;
        }
        public void SetItemOnTile(ushort X, ushort Y, bool Value)
        {
            if (Value)
                BinaryData[X, Y] |= DMapCellFlag.ItemOnTile;
            else
                BinaryData[X, Y] &= ~DMapCellFlag.ItemOnTile;
        }
        public bool ItemOnTile(ushort X, ushort Y)
        {
            if (MaxX > X && MaxY > Y)
            {
                return (BinaryData[X, Y] & DMapCellFlag.ItemOnTile) == DMapCellFlag.ItemOnTile;
            }
            return false;
        }
        public bool FindValidDropLocation(ref ushort X, ref ushort Y, ushort Limit)
        {
            short offset_x = 1;
            short offset_y = 1;
            while (true)
            {
                ushort tryx = (ushort)(X - offset_x);
                ushort tryy = (ushort)(Y - offset_y);
                for (; tryx < X + offset_x; tryx++)
                {
                    for (; tryy < Y + offset_y; tryy++)
                    {
                        if (!this.ItemOnTile(tryx, tryy))
                        {
                            if (!this.InvalidNoHandicap(tryx, tryy))
                            {
                                X = tryx;
                                Y = tryy;
                                return true;
                            }
                        }
                    }
                }
                if (X + offset_x < this.MaxX && 
                    Y + offset_y < this.MaxY)
                {
                    offset_x += 1;
                    offset_y += 1;
                    if (offset_x >= X || offset_y >= Y)
                        return false;
                    else if (offset_x >= Limit || offset_y >= Limit)
                        return false;
                }
            }
        }
    }
}
