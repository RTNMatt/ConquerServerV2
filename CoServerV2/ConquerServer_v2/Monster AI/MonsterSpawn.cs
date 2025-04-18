using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Monster_AI
{
    public class MonsterSpawn
    {
        public const int Multiplier = 3;
        
        private static int m_PendingThreads;
        public static int PendingThreads { get { return m_PendingThreads; } }
        
        public ushort SpawnX, SpawnY;
        public ushort MaxSpawnX, MaxSpawnY;
        public byte SpawnCount;
        public MapID MapID;
        
        public MonsterFamily Family;
        public MobCollection CollectionOwner;
        public uint UIDStart, UIDEnd;
        private Monster[] m_Monsters;
        public Monster[] Monsters { get { return m_Monsters; } }

        private Thread AIThread;
        public int MembersDead;

        /// <summary>
        /// This is used to create the first generation, and should only be called
        /// by the owning MobCollection class.
        /// </summary>
        /// <param name="UIDStartRef">Where to start the UID assignment at.</param>
        public int CreateFirstGeneration(ref uint UIDStartRef)
        {
            int Total = SpawnCount;
            if (Total > 1)
                Total *= Multiplier;
            m_Monsters = new Monster[Total];
            UIDStart = UIDStartRef;
            UIDEnd = (uint)(UIDStartRef + m_Monsters.Length - 1);

            for (int i = 0; i < m_Monsters.Length; i++)
            {
                Monster monster = new Monster(this, Family, CollectionOwner, Family.Settings);
                monster.Target = null;
                monster.Status = MonsterStatus.Roam;
                monster.Entity.MapID = CollectionOwner.MapID;
                monster.Entity.Name = Family.Name;
                monster.Entity.Facing = (ConquerAngle)(Kernel.Random.Next(1000) % 9);
                monster.Entity.Mesh = Family.Mesh;
                monster.Entity.MaxHitpoints = Family.MaxHealth;
                monster.Entity.Hitpoints = Family.MaxHealth;
                monster.Entity.MaxAttack = Family.MaxAttack;
                monster.Entity.MinAttack = Family.MinAttack;
                monster.Entity.Defence = Family.Defense;
                monster.Entity.Level = Family.Level;
                monster.Entity.Dodge = Family.Dodge;
                monster.Entity.UID = UIDStartRef;
                TryObtainSpawnXY(out monster.Entity.Spawn.X, out monster.Entity.Spawn.Y);
                monster.SpawnX = monster.Entity.X;
                monster.SpawnY = monster.Entity.Y;

                if ((Family.Settings & MonsterSettings.Messenger) == MonsterSettings.Messenger)
                {
                    monster.OverrideSpell = 1001;
                    monster.OverrideSpellLevel = 0;
                }
                m_Monsters[i] = monster;
                UIDStartRef++;
            }
            return m_Monsters.Length;
        }
        /// <summary>
        /// Revives every member of the current existing generation that can be revived.
        /// </summary>
        public unsafe void ReviveGeneration()
        {
            TIME Now = TIME.Now;
            foreach (Monster monster in m_Monsters)
            {
                if (monster.Entity.Dead)
                {
                    if (monster.RemoveTime.Time < Now.Time)
                    {
                        monster.TryRespawn(true);
                    }
                }
            }
        }
        /// <summary>
        /// Create a single thread for all families that have only one child.
        /// </summary>
        public static void StartSingleChildAI()
        {
            Thread AI = new Thread(ArtificialIntelligence);
            List<MonsterSpawn> SingleChildren = new List<MonsterSpawn>();
            foreach (KeyValuePair<uint, DataMap> DE in Kernel.DMaps)
            {
                if (DE.Value.HasMobs)
                {
                    foreach (MonsterSpawn spawn in DE.Value.Mobs.Spawns)
                    {
                        if (spawn.SpawnCount == 1)
                        {
                            spawn.AIThread = AI;
                            SingleChildren.Add(spawn);
                        }
                    }
                }
            }
            AI.Start(SingleChildren.ToArray());
        }
        /// <summary>
        /// Requests an AI for this family to be created an ran.
        /// If an AI is already running, nothing will happen.
        /// </summary>
        public void RunAI()
        {
            if (AIThread == null)
            {
                AIThread = new Thread(ArtificialIntelligence);
                AIThread.Priority = ThreadPriority.Lowest;
                AIThread.Start(this);
            }
        }

        private static void ArtificialIntelligence(object Argument)
        {
            m_PendingThreads++;
            try
            {
                bool CanDie;
                bool KeepAlive;
                int Children;
                MonsterSpawn[] Spawns;
                if (Argument is MonsterSpawn)
                {
                    Spawns = new MonsterSpawn[1];
                    Spawns[0] = Argument as MonsterSpawn;
                    CanDie = true;
                }
                else
                {
                    Spawns = Argument as MonsterSpawn[];
                    CanDie = false;
                }

                while (true)
                {
                    Children = 0;
                    KeepAlive = false;
                    foreach (MonsterSpawn Spawn in Spawns)
                    {
                        foreach (Monster monster in Spawn.m_Monsters)
                        {
                            if (monster.Target != null)
                                KeepAlive = true;
                            monster.QueryAction();
                            Thread.Sleep(1);
                        }
                        Children += Spawn.m_Monsters.Length;
                    }
                    Thread.Sleep(Math.Max(10000 / Children, 1));
                    if (!KeepAlive && CanDie)
                    {
                        foreach (MonsterSpawn Spawn in Spawns)
                        {
                            lock (Spawn)
                            {
                                foreach (Monster monster in Spawn.m_Monsters)
                                    monster.Target = null;
                                Spawn.AIThread = null;
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Kernel.NotifyDebugMsg("[Monster AI Exception]", e.ToString(), true);
            }
            m_PendingThreads--;
        }
        /// <summary>
        /// Attemps to obtain a point where the monster can be re-spawned.
        /// </summary>
        /// <param name="X">The x-coordinate point.</param>
        /// <param name="Y">The y-coordinate point.</param>
        public void TryObtainSpawnXY(out ushort X, out ushort Y)
        {
            X = (ushort)Kernel.Random.Next(this.SpawnX, this.MaxSpawnX);
            Y = (ushort)Kernel.Random.Next(this.SpawnY, this.MaxSpawnY);
            for (byte i = 0; i < 10; i++)
            {
                if (CollectionOwner.DMap == null)
                    break;
                if (!CollectionOwner.DMap.Invalid(X, Y) && !CollectionOwner.DMap.MonsterOnTile(X, Y))
                    break;

                X = (ushort)Kernel.Random.Next(this.SpawnX, this.MaxSpawnX);
                Y = (ushort)Kernel.Random.Next(this.SpawnY, this.MaxSpawnY);
            }
        }
    }
}
