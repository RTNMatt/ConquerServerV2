using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Core;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;
using ConquerServer_v2.Client;

namespace ConquerServer_v2.GuildWar
{
    public delegate void SOBMonsterEvent(GameClient Client, SOBMonster Sender, int Param);

    public unsafe class SOBMonster : IBaseEntity, IMapObject
    {
        public SpawnSOBPacket Spawn;
        public SOBMonsterEvent Attacked;
        public SOBMonsterEvent Killed;
        private string m_Name;
        private EntityFlag m_Flag;
        private bool m_Dead;
        private MapID m_MapID;

        // Required for IBaseEntity
        public object Owner { get { return this; } }
        public int MinAttack { get { return 0; } }
        public int MaxAttack { get { return 0; } }
        public int MaxHitpoints { get { return Spawn.MaxHitpoints; } set { Spawn.MaxHitpoints = value; }  }
        public int Defence { get { return 0; } }
        public MapID MapID { get { return m_MapID; } set { m_MapID = value; } }
        public int MDefence { get { return 0; } }
        public int PlusMDefence { get { return 0; } }
        public int Dodge { get { return 0; } }
        public EntityFlag EntityFlag { get { return m_Flag; } }
        public uint UID { get { return Spawn.UID; } set { Spawn.UID = value; } }
        public ushort X { get { return Spawn.X; } set { Spawn.X = value; } }
        public ushort Y { get { return Spawn.Y; } set { Spawn.Y = value; } }
        public ulong StatusFlag { get { return 0; } set { } }
        public int MagicAttack { get { return 0; } }
        public int Hitpoints { get { return Spawn.Hitpoints; } set { Spawn.Hitpoints = value; } }
        public ushort Level { get { return 70; } }
        public bool Dead
        {
            get 
            {
                if (Hitpoints <= 0 && !m_Dead)
                {
                    Dead = true;
                }
                return m_Dead; 
            }
            set
            {
                if (value != m_Dead)
                {
                    if (value)
                    {
                        Hitpoints = 0;
                    }
                    else
                    {
                        Hitpoints = MaxHitpoints;
                    }
                    m_Dead = value;
                }
            }
        }
        public string Name
        {
            get { return m_Name; }
            set
            {
                if (value.Length > 15)
                    value = value.Substring(0, 15);
                m_Name = value;
                Spawn.ShowName = true;
                Spawn.NameLength = (byte)value.Length;
                fixed (byte* lpStrings = Spawn.Strings)
                {
                    MSVCRT.memset(lpStrings, 0, 24);
                    m_Name.CopyTo(lpStrings);
                    PacketBuilder.AppendTQServer((byte*)lpStrings + Spawn.NameLength + 1, 8);
                }
                Spawn.Size = (ushort)(0x1C + Spawn.NameLength + 1);
            }
        }

        // Required for IMapObject
        public ServerFlags ServerFlags { get { return ServerFlags.None; } }
        public MapObjectType MapObjType { get { return MapObjectType.SOB; } }
        public void SendSpawn(GameClient Client)
        {
            if (Client.Screen.Add(this))
            {
                fixed (SpawnSOBPacket* pSpawn = &Spawn)
                {
                    Client.Send(pSpawn);
                }
            }
        }

        public SOBMonster(EntityFlag EntityType)
        {
            Spawn = SpawnSOBPacket.Create();
            m_Flag = EntityType;
        }
    }
}
