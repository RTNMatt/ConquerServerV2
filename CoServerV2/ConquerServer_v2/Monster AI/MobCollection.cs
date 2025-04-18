using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Monster_AI
{
    public class MobCollection
    {
        private const uint MonsterUIDCounterStart = 900000;

        private FlexibleArray<MonsterSpawn> m_SpawnsBuffer;
        private MonsterSpawn[] m_Spawns;
        private int Capacity;
        private MapID m_MapID;
        public DataMap DMap; 

        public MonsterSpawn[] Spawns { get { return m_Spawns; } }
        public MapID MapID { get { return m_MapID; } }

        /// <summary>
        /// Creates a new instance of a monster collection
        /// </summary>
        /// <param name="capacity">This is the ABSOLUTE capacity of this collection (in monsters). It cannot be exceeded, so please account for pets when passing this parameter.</param>
        /// <param name="mapid">This is the ID of the map this collection will represent.</param>
        public MobCollection(MapID mapid, int capacity)
        {
            m_MapID = mapid;
            Capacity = capacity;
            m_SpawnsBuffer = new FlexibleArray<MonsterSpawn>();
        }
        /// <summary>
        /// Searchs for a monster in this collection by the UID. Returns null if
        /// no monster is found in the collection.
        /// </summary>
        /// <param name="MonsterUID">The UID to search for.</param>
        /// <returns></returns>
        public Monster Search(uint MonsterUID)
        {
            foreach (MonsterSpawn spawn in m_Spawns)
            {
                if (MonsterUID >= spawn.UIDStart && MonsterUID <= spawn.UIDEnd)
                {
                    int index = (int)(MonsterUID - spawn.UIDStart);
                    return spawn.Monsters[index];
                }
            }
            return null;
        }

        /// <summary>
        /// Adds a family to the collection, this is done fastest before
        /// FinalizeCollection() is called
        /// </summary>
        /// <param name="Family">The family to add.</param>
        public void AddSpawn(MonsterSpawn Spawn)
        {
            lock (this)
            {
                Spawn.CollectionOwner = this;
                if (m_SpawnsBuffer == null)
                {
                    MonsterSpawn[] temp = new MonsterSpawn[m_Spawns.Length + 1];
                    Array.Copy(m_Spawns, temp, m_Spawns.Length);
                    temp[m_Spawns.Length] = Spawn;
                    m_Spawns = temp;
                }
                else
                {
                    m_SpawnsBuffer.Add(Spawn);
                }
            }
        }
        /// <summary>
        /// Copies the inital buffer into the monsters array, and frees it.
        /// </summary>
        public int FinalizeCollection()
        {
            int monsters = 0;
            m_Spawns = m_SpawnsBuffer.ToTrimmedArray();
            m_SpawnsBuffer = null;

            uint UIDStartRef = MonsterUIDCounterStart;
            foreach (MonsterSpawn Spawn in m_Spawns)
            {
                monsters += Spawn.CreateFirstGeneration(ref UIDStartRef);
                if (UIDStartRef >= 1000000)
                    throw new Exception("UID limit exceeded, use a higher capacity when allocating this instance.");
            }
            return monsters;
        }
    }
}
