using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ConquerServer_v2.Client;

namespace ConquerServer_v2.Core
{
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    public struct MapID
    {
        public static readonly MapID TrainingGrounds = 1039;
        public static readonly MapID GuildWar = 1038;
        public static readonly MapID Market = 1036;

        private const uint DYNAMIC_MapID = 10000;
        private static uint Dynamic_Counter = DYNAMIC_MapID;

        private uint Value;
        public bool Dynamic { get { return Value > DYNAMIC_MapID; } }
        public uint StanderdId { get { return Dynamic ? (Value % DYNAMIC_MapID) : Value; } }
        public uint Id { get { return Value; } }
        public void MakeDynamic()
        {
            const uint reset_counter = 4000000000;
            if (Dynamic_Counter > reset_counter)
            {
                Dynamic_Counter = DYNAMIC_MapID;
            }
            Value += Dynamic_Counter;
            Dynamic_Counter += DYNAMIC_MapID;
        }

        public static implicit operator uint(MapID id)
        {
            return id.StanderdId;
        }
        public static implicit operator MapID(uint id)
        {
            MapID map = new MapID();
            map.Value = id;
            return map;
        }
        public static bool operator ==(MapID id, MapID id2)
        {
            return (id.StanderdId == id2.StanderdId);
        }
        public static bool operator !=(MapID id, MapID id2)
        {
            return (id.StanderdId != id2.StanderdId);
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }
        public override bool Equals(object obj)
        {
            if (obj is MapID)
                return ((MapID)obj) == this;
            else if (obj is uint)
                return ((uint)obj) == this;

            return obj.Equals(this);
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public enum MapObjectType
    {
        Player = 1,
        Monster = 2,
        Item = 3,
        Npc = 4,
        SOB = 5
    }

    public interface IMapObject
    {
        ushort X { get; }
        ushort Y { get; }
        MapID MapID { get; }
        uint UID { get; }
        object Owner { get; }
        MapObjectType MapObjType { get; }
        void SendSpawn(GameClient Client);
    }
}
