using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Client;

namespace ConquerServer_v2.Core
{
    public enum EntityFlag : byte
    {
        Player = 1,
        Monster = 2,
        Pet = 3,
        GuildGate = 5,
        GuildPole = 6
    }

    public struct StatusFlag
    {
        public const uint
            None = 0x00,
            BlueName = 0x01,
            Poisoned = 0x02,
            XPSkills = 0x10,
            Ghost = 0x20,
            TeamLeader = 0x40,
            PurpleShield = 0x100,
            Stigma = 0x200,
            Dead = 0x400,
            RedName = 0x4000,
            BlackName = 0x8000,
            Superman = 0x40000,
            Invisible = 0x400000,
            Cyclone = 0x800000,
            Fly = 0x8000000;
    }

    public interface IBaseEntity
    {
        bool Dead { get; }
        int Hitpoints { get; set; }
        int MaxHitpoints { get; }
        object Owner { get; }
        int MinAttack { get; }
        int MaxAttack { get; }
        int MagicAttack { get; }
        int Defence { get; }
        int MDefence { get; }
        int PlusMDefence { get; }
        int Dodge { get; }
        EntityFlag EntityFlag { get; }
        MapID MapID { get; }
        uint UID { get; }
        ushort X { get; }
        ushort Y { get; }
        ushort Level { get; }
        string Name { get; }
        ulong StatusFlag { get; set; }
        ServerFlags ServerFlags { get; }
        void SendSpawn(GameClient Client);
    }

    public interface IEntityOwner
    {
        IBaseEntity GetEntityOwner();
    }

    public unsafe class CommonEntity : IBaseEntity, IMapObject
    {
        public SpawnEntityPacket Spawn;
        private string m_Name;
        private bool m_Dead;
        private EntityFlag m_EntityFlag;
        private MapID m_MapID;
        private int m_Hitpoints;
        private int m_MaxHitpoints;
        private int m_MaxAttack;
        private int m_MinAttack;
        private int m_MagicAttack;
        private int m_Defence;
        private int m_MDefence;
        private int m_PlusMDefence;
        private int m_Dodge;
        private object m_Owner;
        private MapObjectType m_MapObjectType;
        private ushort m_Mesh;
        private ushort m_Avatar;
        private ushort m_OverlappingMesh;
        private GameClient m_ClientOwner;

        // Implement IBaseEntity, IMapObject
        public MapID MapID { get { return m_MapID; } set { m_MapID = value; } }
        public EntityFlag EntityFlag { get { return m_EntityFlag; } }
        public uint UID { get { return Spawn.UID; } set { Spawn.UID = value; } }
        public ushort X { get { return Spawn.X; } set { Spawn.X = value; } }
        public ushort Y { get { return Spawn.Y; } set { Spawn.Y = value; } }
        public object Owner { get { return m_Owner; } }
        public int MaxAttack { get { return m_MaxAttack; } set { m_MaxAttack = value; } }
        public int MinAttack { get { return m_MinAttack; } set { m_MinAttack = value; } }
        public int MagicAttack { get { return m_MagicAttack; } set { m_MagicAttack = value; } }
        public int Defence { get { return m_Defence; } set { m_Defence = value; } }
        public int MDefence { get { return m_MDefence; } set { m_MDefence = value; } }
        public int Dodge { get { return m_Dodge; } set { m_Dodge = value; } }
        public int PlusMDefence { get { return m_PlusMDefence; } set { m_PlusMDefence = value; } }
        public MapObjectType MapObjType { get { return m_MapObjectType; } }
        public ulong StatusFlag { get { return Spawn.StatusFlag; } set { Spawn.StatusFlag = value; } }
        public ushort Level { get { return Spawn.Level; } set { Spawn.Level = Spawn.LevelPotency = value; } }
        public byte Reborn { get { return Spawn.Reborn; } set { Spawn.Reborn = value; } }
        public ushort GuildID { get { return Spawn.GuildID; } set { Spawn.GuildID = value; } }
        public GuildRank GuildRank { get { return Spawn.GuildRank; } set { Spawn.GuildRank = value; } }
        public ushort Hairstyle { get { return Spawn.Hairstyle; } set { Spawn.Hairstyle = value; } }
        public NobilityID Nobility { get { return Spawn.Nobility; } set { Spawn.Nobility = value; } }
        public ConquerAngle Facing { get { return Spawn.Facing; } set { Spawn.Facing = value; } }
        public ConquerAction Action { get { return Spawn.Action; } set { Spawn.Action = value; } }
        public uint Model { get { return Spawn.Model; } set { Spawn.Model = value; } }
        public ServerFlags ServerFlags
        {
            get
            {
                if (m_ClientOwner != null)
                    return m_ClientOwner.ServerFlags;
                return ServerFlags.None;
            }
        }

        public int Hitpoints
        {
            get { return m_Hitpoints; }
            set
            {
                m_Hitpoints = value;
                Spawn.Hitpoints = (ushort)Math.Min(m_Hitpoints, ushort.MaxValue); 
                if (m_ClientOwner != null)
                {
                    if (m_ClientOwner.InTeam)
                    {
                        UpdatePacket update = UpdatePacket.Create();
                        update.ID = UpdateID.Hitpoints;
                        update.Value = (uint)m_Hitpoints;
                        update.UID = Spawn.UID;
                        m_ClientOwner.Team.SendTeamPacket(&update, false);
                    }
                }
            }
        }
        public int MaxHitpoints
        {
            get { return m_MaxHitpoints; }
            set 
            { 
                m_MaxHitpoints = value;
                if (m_ClientOwner != null)
                {
                    if (m_ClientOwner.InTeam)
                    {
                        UpdatePacket update = UpdatePacket.Create();
                        update.ID = UpdateID.MaxHitpoints;
                        update.Value = (uint)m_MaxHitpoints;
                        update.UID = Spawn.UID;
                        m_ClientOwner.Team.SendTeamPacket(&update, false);
                    }
                }
            }
        }
        public string Name
        {
            get { return m_Name; }
            set
            {
                m_Name = value;
                Spawn.SetName(value);
            }
        }
        public bool Dead
        {
            get
            {
                if (m_Hitpoints <= 0)
                {
                    Dead = true;
                }
                return m_Dead;
            }
            set
            {
                if (m_Dead != value)
                {
                    m_Dead = value;
                    if (!m_Dead)
                    {
                        Hitpoints = MaxHitpoints;
                        if (EntityFlag == EntityFlag.Player)
                            OverlappingMesh = 0;
                    }
                    else
                    {
                        Hitpoints = 0;
                        byte overlappingMesh = (byte)(97 + ((byte)(Mesh / 1000)));
                        OverlappingMesh = overlappingMesh;
                    }
                }
            }
        }
        public ushort OverlappingMesh
        {
            get { return m_OverlappingMesh; }
            set
            {
                m_OverlappingMesh = value;
                Spawn.Model = (uint)((m_OverlappingMesh * 10000000) + (m_Avatar * 10000) + m_Mesh);
            }
        }
        public ushort Mesh
        {
            get { return m_Mesh; }
            set
            {
                m_Mesh = value;
                Spawn.Model = (uint)((m_OverlappingMesh * 10000000) + (m_Avatar * 10000) + m_Mesh);
            }
        }
        public ushort Avatar
        {
            get { return m_Avatar; }
            set
            {
                m_Avatar = value;
                Spawn.Model = (uint)((m_OverlappingMesh * 10000000) + (m_Avatar * 10000) + m_Mesh);
            }
        }
        public CommonEntity(object owner, EntityFlag type)
        {
            m_Owner = owner;
            Spawn = SpawnEntityPacket.Create();
            m_EntityFlag = type;    
            if (m_EntityFlag == EntityFlag.Player)
                m_ClientOwner = m_Owner as GameClient;
            switch (m_EntityFlag)
            {
                case EntityFlag.Monster: m_MapObjectType = MapObjectType.Monster; break;
                case EntityFlag.Player: m_MapObjectType = MapObjectType.Player; break;
                case EntityFlag.Pet: m_MapObjectType = MapObjectType.Monster; break;
                default: throw new ArgumentException("type");
            }
        }

        public void SendSpawn(GameClient Client)
        {
            if (Client.Screen.Add(this))
            {
                fixed (SpawnEntityPacket* lpSpawn = &Spawn)
                {
                    Client.Send(lpSpawn);
                }
            }
        }
    }
}
