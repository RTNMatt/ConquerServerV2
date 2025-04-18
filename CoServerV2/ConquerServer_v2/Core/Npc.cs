using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Core
{
    public unsafe class NpcEntity : IMapObject
    {
        public SpawnNpcPacket Spawn;
        private MapID m_MapID;

        // IMapObject
        public uint UID { get { return Spawn.UID; } set { Spawn.UID = value; Spawn.UID2 = value; } }
        public ushort X { get { return Spawn.X; } set { Spawn.X = value; } }
        public ushort Y { get { return Spawn.Y; } set { Spawn.Y = value; } }
        public MapID MapID { get { return m_MapID; } set { m_MapID = value; } }
        public MapObjectType MapObjType { get { return MapObjectType.Npc; } }
        public object Owner { get { return this; } }
        // NpcEntity
        public ushort Interaction { get { return Spawn.Interaction; } set { Spawn.Interaction = value; } }
        public ulong Flag { get { return Spawn.Flag; } set { Spawn.Flag = (uint)value; } }
        public bool IsVendor { get { return Spawn.IsVendor; } }
        public ushort NpcType { get { return Spawn.NpcType; } set { Spawn.NpcType = value; } }

        public void SendSpawn(GameClient Client)
        {
            if (Client.Screen.Add(this))
            {
                fixed (SpawnNpcPacket* p_npc = &Spawn)
                {
                    Client.Send(p_npc);
                }
            }
        }
        public void ConvertToVendor(string Name)
        {
            Spawn.ConvertToVendor(Name);
        }
        public void ConvertToStandard()
        {
            Spawn.ConvertToStandard();
        }

        public NpcEntity()
        {
            Spawn = SpawnNpcPacket.Create();
        }
    }
}