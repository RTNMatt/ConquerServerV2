using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ConquerServer_v2.Client;
using ConquerServer_v2.GuildWar;
using ConquerServer_v2.Core;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Monster_AI;

namespace ConquerServer_v2.Database
{
    public unsafe partial class ServerDatabase
    {
        private static string m_Path;
        private static string m_Startup;
        public static string Path { get { return m_Path; } }
        public static string Startup { get { return m_Startup; } }

        private static IniFile AuthSystem;
        private static IniFile Items;
        private static IniFile GameMap;
        public static IniFile RevivePoints;
        public static IniFile Shop;
        public static IniFile LevelExp;
        public static IniFile Transform;
        public static IniFile Portals;
        public static IniFile RegisteredNames;
        public static IniFile Settings;
        public static IniFile Pets;
        public static IniFile Mining;
        public static Dictionary<uint, MonsterFamily> MonsterFamilies;

        public static void IncPlayerOnline()
        {
            Settings.Write<int>("Config", "PlayersOnline", Settings.ReadInt32("Config", "PlayersOnline", 0) + 1);
        }
        public static void DecPlayerOnline()
        {
            Settings.Write<int>("Config", "PlayersOnline", Settings.ReadInt32("Config", "PlayersOnline", 1) - 1);
        }

        /// <summary>
        /// Force a call to the static ctor
        /// </summary>
        public static void Init()
        {
            m_Startup = System.Windows.Forms.Application.StartupPath;
            string[] path = m_Startup.Split('\\');
            // CODE_DEBUG:
            // This symbol should be defined if the ConquerServer_v2 solution is deployed on the machine
            // with the source code / compiler. If the application is running on a dedicated computer,
            // the application, and the database-folder, should be located in the same path
            // i.e.:
            // c:\ConquerServer_v2.exe
            // c:\Database\
#if CODE_DEBUG
            m_Path = "";
            for (int i = 0; i < path.Length - 3; i++)
                m_Path += path[i] + "\\";
#else
            m_Path = m_Startup;
#endif
            m_Path += "Database";
            
            AuthSystem = new IniFile(Path + @"\Misc\AuthSystem.ini");
            Items = new IniFile(Path + @"\Misc\Items.ini");
            RevivePoints = new IniFile(Path + @"\Misc\RevivePoints.ini");
            Shop = new IniFile(Path + @"\Misc\Shop.ini");
            GameMap = new IniFile(Path + @"\Misc\gamemap.ini");
            LevelExp = new IniFile(Path + @"\Misc\LevelExp.ini");
            Transform = new IniFile(Path + @"\Misc\Transform.ini");
            Portals = new IniFile(Path + @"\Misc\Portals.ini");
            RegisteredNames = new IniFile(Path + @"\Misc\RegisteredNames.ini");
            Pets = new IniFile(Path + @"\Misc\Pets.ini");
            Mining = new IniFile(Path + @"\Misc\Mining.ini");
            Settings = new IniFile(Path + @"\Misc\Settings.ini");
            Settings.WriteString("Config", "PlayersOnline", "0");
            File.Delete(Path + @"\Misc\AuthSystem.ini");

            Tournaments.Init();
            Warehouse.Init();
            NobilityScoreBoard.Init();
            Console.WriteLine("\tLoaded {0} Lottery Items.", Lottery.Init());

            IniFile ini = new IniFile();
            int num, num2;

            // DMaps
            num = 0;
            if (Directory.Exists(Path + "\\DMaps\\"))
            {
                string[] Entries = GameMap.GetSection("Maps", 8192);
                Kernel.DMaps = new Dictionary<uint, DataMap>();
                foreach (string entry in Entries)
                {
                    int index = entry.IndexOf('=');
                    ushort MapID = ushort.Parse(entry.Substring(0, index));
                    string DMapFile = entry.Substring(index + 1, entry.Length - index - 1).Replace("map/map/", Path + "\\DMaps\\");

                    DataMap DMap = new DataMap(DMapFile, MapID);
                    Kernel.DMaps.Add(MapID, DMap);
                    num++;
                }
            }   
            Console.WriteLine("\tLoaded {0} Compressed Data Maps.", num);

            // Npcs
            num = 0;
            foreach (string npc_file in Directory.GetFiles(Path + @"\Npc Spawns\"))
            {
                ini.FileName = npc_file;
                MapID mapID = ini.ReadUInt32("Npc", "MapID", 0);
                if (mapID == 0)
                    throw new ArgumentException("File corrupt (MapID = 0) : " + npc_file, "npc_file");

                DictionaryV2<uint, NpcEntity> npcs;
                DataMap map;
                if (Kernel.DMaps.TryGetValue(mapID, out map))
                {
                    npcs = map.Npcs;
                    uint UID = ini.ReadUInt32("Npc", "NpcID", 0);
                    if (UID == 0)
                        throw new ArgumentException("File corrupt (UID = 0) : " + npc_file, "npc_file");

                    NpcEntity npc = new NpcEntity();
                    npc.UID = UID;
                    npc.MapID = mapID;
                    npc.NpcType = ini.ReadUInt16("Npc", "NpcType", 0);
                    npc.X = ini.ReadUInt16("Npc", "X", 0);
                    npc.Y = ini.ReadUInt16("Npc", "Y", 0);
                    npc.Interaction = ini.ReadUInt16("Npc", "Interaction", 0);
                    npc.Flag = ini.ReadUInt32("Npc", "Flag", 0);

                    npcs.Add(npc.UID, npc);
                }
                num++;
            }
            Console.WriteLine("\tLoaded {0} Npcs Spawns.", num);

            // Monsters
            MonsterFamilies = new Dictionary<uint, MonsterFamily>();
            num = 0;
            num2 = 0;
            // Monster Family (cq_monstertype)
            foreach (string fname in Directory.GetFiles(Path + "\\Monsters\\"))
            {
#if SPAWN_NO_MOBS
                continue;
#endif
                ini.FileName = fname;
                MonsterFamily Family = new MonsterFamily();
                Family.ID = ini.ReadUInt32("cq_monstertype", "id", 0);
                Family.Name = ini.ReadString("cq_monstertype", "name", "INVALID_MOB");
                Family.Level = ini.ReadUInt16("cq_monstertype", "level", 0);
#if SPAWN_GUARDS_ONLY
                        if (!Family.Name.StartsWith("Guard") && !Family.Name.Contains("Reviver"))
                            continue;
#endif
                Family.MaxAttack = ini.ReadInt32("cq_monstertype", "attack_max", 0);
                Family.MinAttack = ini.ReadInt32("cq_monstertype", "attack_min", 0);
                if (Family.Name == "INVALID_MOB" || Family.Level == 0 || Family.ID == 0 || Family.MinAttack > Family.MaxAttack)
                {
                    Console.WriteLine("MONSTER FILE CORRUPT: \r\n" + fname + "\r\n");
                    continue;
                }
                Family.Defense = ini.ReadUInt16("cq_monstertype", "defence", 0);
                Family.Mesh = ini.ReadUInt16("cq_monstertype", "lookface", 0);
                Family.MaxHealth = ini.ReadInt32("cq_monstertype", "life", 0);
                Family.ViewRange = 16;
                Family.AttackRange = ini.ReadSByte("cq_monstertype", "attack_range", 0);
                Family.Dodge = ini.ReadSByte("cq_monstertype", "dodge", 0);
                Family.DropBoots = ini.ReadByte("cq_monstertype", "drop_shoes", 0);
                Family.DropNecklace = ini.ReadByte("cq_monstertype", "drop_necklace", 0);
                Family.DropRing = ini.ReadByte("cq_monstertype", "drop_ring", 0);
                Family.DropArmet = ini.ReadByte("cq_monstertype", "drop_armet", 0);
                Family.DropArmor = ini.ReadByte("cq_monstertype", "drop_armor", 0);
                Family.DropShield = ini.ReadByte("cq_monstertype", "drop_shield", 0);
                Family.DropWeapon = ini.ReadByte("cq_monstertype", "drop_weapon", 0);
                Family.DropMoney = ini.ReadUInt16("cq_monstertype", "drop_money", 0);
                Family.DropHPItem = ini.ReadUInt32("cq_monstertype", "drop_hp", 0);
                Family.DropMPItem = ini.ReadUInt32("cq_monstertype", "drop_mp", 0);

                Family.DropSpecials = new SpecialItemWatcher[ini.ReadInt32("SpecialDrop", "Count", 0)];
                for (int i = 0; i < Family.DropSpecials.Length; i++)
                {
                    string[] Data = ini.ReadString("SpecialDrop", i.ToString(), "", 32).Split(',');
                    Family.DropSpecials[i] = new SpecialItemWatcher(uint.Parse(Data[0]), int.Parse(Data[1]));
                }

                Family.CreateItemGenerator();
                Family.CreateMonsterSettings();
                
                MonsterFamilies.Add(Family.ID, Family);
                num++;
            }
            // Monster Spawns (cq_generator)
            foreach (string fmap in Directory.GetDirectories(Path + "\\MonsterSpawns\\"))
            {
                uint tMapID;
                if (!uint.TryParse(fmap.Remove(0, (Path + "\\MonsterSpawns\\").Length), out tMapID))
                    continue;
                if (tMapID == MapID.TrainingGrounds.Id)
                    continue;

                MobCollection Linker = new MobCollection(tMapID, 5000);
                if (!Kernel.DMaps.TryGetValue(tMapID, out Linker.DMap))
                    throw new ArgumentException(string.Format("Attempting to spawn monsters on mapid {0}, with no data-map provided.", tMapID));
                Linker.DMap.Mobs = Linker;

                foreach (string fmobtype in Directory.GetDirectories(fmap))
                {
                    foreach (string ffile in Directory.GetFiles(fmobtype))
                    {
                        ini.FileName = ffile;
                        MonsterSpawn Spawn = new MonsterSpawn();
                        Spawn.CollectionOwner = Linker;
                        uint ID = ini.ReadUInt32("cq_generator", "npctype", 0);
                        if (!MonsterFamilies.TryGetValue(ID, out Spawn.Family))
                        {
                            continue;
                        }
                        Spawn.SpawnX = ini.ReadUInt16("cq_generator", "bound_x", 0);
                        Spawn.SpawnY = ini.ReadUInt16("cq_generator", "bound_y", 0);
                        Spawn.MaxSpawnX = (ushort)(Spawn.SpawnX + ini.ReadUInt16("cq_generator", "bound_cx", 0));
                        Spawn.MaxSpawnY = (ushort)(Spawn.SpawnY + ini.ReadUInt16("cq_generator", "bound_cy", 0));
                        Spawn.MapID = ini.ReadUInt32("cq_generator", "mapid", 0);
                        Spawn.SpawnCount = ini.ReadByte("cq_generator", "max_per_gen", 0);
                        Linker.AddSpawn(Spawn);
                    }
                }
                num2 += Linker.FinalizeCollection();
            }
            MonsterSpawn.StartSingleChildAI();
            Console.WriteLine("\tLoaded {0} Monsters.\r\n\tLoaded {1} Monster Families.", num2, num);
        }

        private static void GemAlgorithm(GameClient Client, byte SocketOne, byte SocketTwo, bool Add)
        {
            byte Gem = SocketOne;
            bool FirstGem = true;
            byte GemID;
            int GemQ;

        AnalyzeGem:
            GemID = (byte)(Gem / 10);
            if (GemID >= 0 && GemID <= 7)
            {
                GemQ = (short)Math.Min(Gem - (GemID * 10), 3);
                switch (GemID)
                {
                    case GemsConst.RainbowGem:
                        {
                            if (GemQ == 3)
                                GemQ = 25;
                            else
                                GemQ = ((GemQ + 1) * 5);
                            break;
                        }
                    case GemsConst.MoonGem:
                        {
                            if (GemQ == 3)
                                GemQ = 50;
                            else
                                GemQ *= 15;
                            break;
                        }
                    case GemsConst.TortoiseGem:
                        {
                            GemQ *= 2;
                            break;
                        }
                    case GemsConst.VioletGem:
                        {
                           
                            if (GemQ == 0)
                                GemQ = 30;
                            else
                                GemQ *= 50;
                            break;
                        }
                    case GemsConst.ThunderGem:
                    case GemsConst.GloryGem:
                        {
                            // these large numbers are due to gemq*0.01
                            if (GemQ == 0)
                                GemQ = 10000;
                            else if (GemQ == 1)
                                GemQ = 30000;
                            else if (GemQ == 2)
                                GemQ = 50000;
                            break;
                        }
                    default: GemQ *= 5; break;
                }

                if (Add)
                    Client.Gems[GemID] += ((double)GemQ * 0.01);
                else
                    Client.Gems[GemID] -= ((double)GemQ * 0.01);
            }
            if (FirstGem)
            {
                Gem = SocketTwo;
                FirstGem = false;
                goto AnalyzeGem;
            }
        }
        public static bool FindPortal(uint MapID, ushort X, ushort Y, out uint DestMapID, out ushort DestX, out ushort DestY)
        {
            DestMapID = DestX = DestY = 0;
            try
            {
                string strMapID = MapID.ToString();
                string query = strMapID + " " + X.ToString() + " " + Y.ToString();
                short count = Portals.ReadInt16(strMapID, "Count", 0);
                for (short i = 0; i < count; i++)
                {
                    string chk_query = Portals.ReadString(strMapID, "PortalEnter" + i.ToString(), "", 20);
                    if (chk_query == query)
                    {
                        string[] exit = Portals.ReadString(strMapID, "PortalExit" + i.ToString(), "", 20).Split(' ');
                        DestMapID = ushort.Parse(exit[0]);
                        DestX = ushort.Parse(exit[1]);
                        DestY = ushort.Parse(exit[2]);
                        return true;
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
            }
            return false;
        }
        public static bool ValidItemID(uint ID)
        {
            return File.Exists(Path + "\\Items\\" + ID.ToString() + ".ini");
        }
        public static bool NpcDistanceCheck(uint UID, DataMap Map, ushort X, ushort Y)
        {
            if (UID == 2888) // Shopping Mall
                return true;
            if (Map.MapID == MapID.GuildWar)
            {
                SOBMonster mob = GuildWarKernel.Search(UID);
                if (mob != null)
                {
                    X = mob.X;
                    Y = mob.Y;
                    return true;
                }
            }
            NpcEntity npc;
            if (Map.Npcs.TryGetValue(UID, out npc))
            {
                return (Kernel.GetDistance(npc.X, npc.Y, X, Y) <= Kernel.ViewDistance);
            }
            return false;
        }
        public static void GetMAttackData(ushort SpellID, ushort Level, MAttackData* lpData)
        {
            IniFile rdr = new IniFile(Path + "\\Spells\\" + SpellID.ToString() + "[" + Level.ToString() + "].ini");
            lpData->SpellID = rdr.ReadUInt16("SpellInformation", "ID", 0);
            lpData->SpellLevel = rdr.ReadByte("SpellInformation", "Level", 0);
            lpData->Stamina = rdr.ReadSByte("SpellInformation", "Stamina", 0);
            lpData->Range = rdr.ReadSByte("SpellInformation", "Range", 0);
            lpData->Distance = rdr.ReadSByte("SpellInformation", "Distance", 0);
            lpData->Mana = rdr.ReadUInt16("SpellInformation", "Mana", 0);
            lpData->BaseDamage = rdr.ReadInt32("SpellInformation", "MDamage", 0);
            lpData->SuccessRate = rdr.ReadByte("SpellInformation", "SucessRate", 0);
            lpData->Aggressive = (rdr.ReadByte("SpellInformation", "Aggressive", 0) == 1);
            lpData->Weapon = rdr.ReadUInt16("SpellInformation", "Weapon", 0);
            lpData->TargetType = (MAttackTargetType)rdr.ReadByte("SpellInformation", "TargetType", 0);
            lpData->Sort = (MagicSort)rdr.ReadByte("SpellInformation", "Unknown0", (byte)MagicSort.ATTACK);
            lpData->MultipleTargets = (rdr.ReadByte("SpellInformation", "MultipleTargets", 0) == 1);
            lpData->IsXPSkill = (rdr.ReadByte("SpellInformation", "IsXPSkill", 0) == 1);
            lpData->SecondsTimer = rdr.ReadUInt16("SpellInformation", "Timer", 0);
            lpData->NextSpellID = rdr.ReadUInt16("SpellInformation", "Delay2", 0);
            lpData->Experience = rdr.ReadUInt32("SpellInformation", "Experience", 0);
            lpData->GroundAttack = (rdr.ReadByte("SpellInformation", "Ground", 0) == 1);
        }
        public static bool AccountOnline(string Account)
        {
            IniFile Player = new IniFile();
            Player.FileName = ServerDatabase.Path + "\\Accounts\\" + Account + ".ini";
            return (Player.ReadString("Character", "Logged", "0", 2) == "1");
        }

        private static void GetStats(byte bJob, ushort wLvl, StatData* pData)
        {
            string Job;
            if (bJob >= 10 && bJob <= 15)
                Job = "Trojan";
            else if (bJob >= 20 && bJob <= 25)
                Job = "Warrior";
            else if (bJob >= 40 && bJob <= 45)
                Job = "Archer";
            else if (bJob >= 50 && bJob <= 55)
                Job = "Ninja";
            else
                Job = "Taoist";
            string Lvl = Math.Min(wLvl, (ushort)120).ToString();
            IniFile rdr = new IniFile(ServerDatabase.Path + "\\Misc\\" + Job + ".ini");
            pData->Spirit = rdr.ReadUInt16(Lvl, "Spirit", 0);
            pData->Strength = rdr.ReadUInt16(Lvl, "Strength", 0);
            pData->Agility = rdr.ReadUInt16(Lvl, "Agility", 0);
            pData->Vitality = rdr.ReadUInt16(Lvl, "Vitality", 0);
        }
        public static void GetStats(GameClient Client)
        {
            fixed (StatData* lpStat = &Client.Stats)
                GetStats(Client.Job, Client.Entity.Level, lpStat);
        }
        public static uint ItemNameToID(string Name, string Quality, out byte AdminFlag)
        {
            AdminFlag = 0;
            string str = Items.ReadString("Items", Name.ToUpper() + Quality.ToUpper(), "0", IniFile.Int32_Size + 5);
            string[] strs = str.Split(',');
            if (strs.Length > 1)
                AdminFlag = byte.Parse(strs[1]);
            return uint.Parse(strs[0]);
        }
        public unsafe static void LoadItemStats(GameClient Client, Item Item)
        {
            IniFile rdr;
            StanderdItemStats standerd = new StanderdItemStats(Item.ID, out rdr);

            if (Item.Position == ItemPosition.DefenceTalisman)
            {
                Client.TalismenDefence = standerd.PhysicalDefence;
                Client.TalismenMDefence = standerd.MDefence;
            }
            else
            {
                if (Client.InTransformation)
                {
                    Client.Transform.Defence += standerd.PhysicalDefence;
                    Client.Transform.MDefence += standerd.MDefence;
                    Client.Transform.Dodge += standerd.Dodge;
                }
                else
                {
                    Client.Entity.Defence += standerd.PhysicalDefence;
                    Client.Entity.MDefence += standerd.MDefence;
                    Client.Entity.Dodge += standerd.Dodge;
                }
            }

            Client.BaseMagicAttack += standerd.MAttack;
            Client.ItemHP += standerd.HP;
            Client.ItemMP += standerd.MP;

            if (Item.Position == ItemPosition.Right)
            {
                Client.AutoAttackSpeed = standerd.Frequency;
                Client.AttackRange += standerd.AttackRange;
            }

            if (Item.Position == ItemPosition.AttackTalisman)
            {
                Client.TalismenAttack = standerd.MaxAttack;
                Client.TalismenMAttack = standerd.MAttack;
            }
            else
            {
                if (Item.Position == ItemPosition.Left)
                {
                    Client.BaseMinAttack += (int)(standerd.MinAttack * 0.5F);
                    Client.BaseMaxAttack += (int)(standerd.MaxAttack * 0.5F);
                }
                else
                {
                    Client.BaseMinAttack += standerd.MinAttack;
                    Client.BaseMaxAttack += standerd.MaxAttack;
                }
            }

            if (Item.Plus != 0)
            {
                PlusItemStats plus = new PlusItemStats(Item.ID, Item.Plus, rdr);
                Client.BaseMinAttack += plus.MinAttack;
                Client.BaseMaxAttack += plus.MaxAttack;
                Client.BaseMagicAttack += plus.MAttack;
                if (Client.InTransformation)
                {
                    Client.Transform.Defence += plus.PhysicalDefence;
                    Client.Transform.Dodge += plus.Dodge;
                }
                else
                {
                    Client.Entity.Defence += plus.PhysicalDefence;
                    Client.Entity.Dodge += plus.Dodge;
                }
                Client.Entity.PlusMDefence += plus.PlusMDefence;
                Client.ItemHP += plus.HP;
            }
            if (Item.Position != ItemPosition.Garment && // Ignore these stats on these slots
                Item.Position != ItemPosition.Bottle)
            {
                Client.ItemHP += Item.Enchant;
                Client.BlessPercent += Item.Bless;
                GemAlgorithm(Client, Item.SocketOne, Item.SocketTwo, true);
            }
        }
        public unsafe static void UnloadItemStats(GameClient Client, Item Item)
        {
            IniFile rdr;
            StanderdItemStats standerd = new StanderdItemStats(Item.ID, out rdr);

            if (Item.Position == ItemPosition.DefenceTalisman)
            {
                Client.TalismenDefence = 0;
                Client.TalismenMDefence = 0;
            }
            else
            {
                if (Client.InTransformation)
                {
                    Client.Transform.Defence -= standerd.PhysicalDefence;
                    Client.Transform.MDefence -= standerd.MDefence;
                    Client.Transform.Dodge -= standerd.Dodge;
                }
                else
                {
                    Client.Entity.Defence -= standerd.PhysicalDefence;
                    Client.Entity.MDefence -= standerd.MDefence;
                    Client.Entity.Dodge -= standerd.Dodge;
                }
            }

            Client.BaseMagicAttack -= standerd.MAttack;
            Client.ItemHP -= standerd.HP;
            Client.ItemMP -= standerd.MP;
            if (Item.Position == ItemPosition.Right)
            {
                Client.AutoAttackSpeed = 1000;
                Client.AttackRange = 0;
            }

            if (Item.Position == ItemPosition.AttackTalisman)
            {
                Client.TalismenAttack = 0;
                Client.TalismenMAttack = 0;
            }
            else
            {
                if (Item.Position == ItemPosition.Left)
                {
                    Client.BaseMinAttack -= (int)(standerd.MinAttack * 0.5F);
                    Client.BaseMaxAttack -= (int)(standerd.MaxAttack * 0.5F);
                }
                else
                {
                    Client.BaseMinAttack -= standerd.MinAttack;
                    Client.BaseMaxAttack -= standerd.MaxAttack;
                }
            }

            if (Item.Plus != 0)
            {
                PlusItemStats plus = new PlusItemStats(Item.ID, Item.Plus, rdr);
                Client.BaseMinAttack -= plus.MinAttack;
                Client.BaseMaxAttack -= plus.MaxAttack;
                Client.BaseMagicAttack -= plus.MAttack;
                if (Client.InTransformation)
                {
                    Client.Transform.Defence -= plus.PhysicalDefence;
                    Client.Transform.Dodge -= plus.Dodge;
                }
                else
                {
                    Client.Entity.Defence -= plus.PhysicalDefence;
                    Client.Entity.Dodge -= plus.Dodge;
                }
                Client.Entity.PlusMDefence -= plus.PlusMDefence;
                Client.ItemHP -= plus.HP;
            }
            if (Item.Position != ItemPosition.Garment &&  // Ignore these stats on these slots
                Item.Position != ItemPosition.Bottle)
            {
                Client.ItemHP -= Item.Enchant;
                Client.BlessPercent -= Item.Bless;
                GemAlgorithm(Client, Item.SocketOne, Item.SocketTwo, false);
            }
        }

        public static void SaveMentorStudents(string Account, ref FlexibleArray<MentorStudent> Students)
        {
            BinaryFile binary = new BinaryFile();
            if (binary.Open(Path + "\\UserMentorStudents\\" + Account + ".bin", FileMode.Create))
            {
                DatabaseMentorStudent db_student = new DatabaseMentorStudent();
                int student_count = Students.Length;
                binary.Write(&student_count, sizeof(int));
                for (int i = 0; i < student_count; i++)
                {
                    db_student.FromStudent(Students.Elements[i]);
                    binary.Read(&db_student, sizeof(DatabaseMentorStudent));
                }
                binary.Close();
            }
        }
        private static void SaveMentorStudents(GameClient Client)
        {
            SaveMentorStudents(Client.Account, ref Client.Students);
        }
        public static void SaveAssociates(string Account, ref FlexibleArray<IAssociate> Friends, ref FlexibleArray<IAssociate> Enemies)
        {
            BinaryFile binary = new BinaryFile();
            if (binary.Open(Path + "\\UserAssociates\\" + Account + ".bin", FileMode.Create))
            {
                DatabaseAssociate db_associate = new DatabaseAssociate();

                int associate_count;
                //friends
                associate_count = Friends.Length;
                binary.Write(&associate_count, sizeof(int));
                for (int i = 0; i < Friends.Length; i++)
                {
                    db_associate.FromAssociate(Friends.Elements[i]);
                    binary.Write(&db_associate, sizeof(DatabaseAssociate));
                }

                // TO-DO:
                // enemies

                binary.Close();
            }
        }
        private static void SaveAssociates(GameClient Client)
        {
            SaveAssociates(Client.Account, ref Client.Friends, ref Client.Enemies);
        }
        private static void SaveItems(GameClient Client)
        {
            BinaryFile binary = new BinaryFile();
            if (binary.Open(Path + "\\UserItems\\" + Client.Account + ".bin", FileMode.Create))
            {
                DatabaseItem db_item = new DatabaseItem();
                int item_count;
                // inventory
                lock (Client.Inventory)
                {
                    item_count = 0;
                    binary.Write(&item_count, sizeof(int));
                    for (int i = 0; i < Client.Inventory.MaxPossibleItems; i++)
                    {
                        if (Client.Inventory[i] != null)
                        {
                            db_item.FromItem(Client.Inventory[i]);
                            binary.Write(&db_item, sizeof(DatabaseItem));
                            item_count++;
                        }
                    }
                    int pos = binary.Position;
                    binary.Position = 0;
                    binary.Write(&item_count, sizeof(int));
                    binary.Position = pos;
                }
                // equipment
                item_count = Client.Equipment.ItemCount;
                binary.Write(&item_count, sizeof(int));
                for (ItemPosition p = Item.FirstSlot; p <= Item.LastSlot; p++)
                {
                    if (Client.Equipment[p] == null)
                    {
                        if (db_item.ID != 0)
                            db_item.FromItem(Item.Blank);
                    }
                    else
                    {
                        db_item.FromItem(Client.Equipment[p]);
                    }
                    binary.Write(&db_item, sizeof(DatabaseItem));
                }
                binary.Close();
            }
        }
        private static void SaveSkills(GameClient Client)
        {
            BinaryFile binary = new BinaryFile();
            if (binary.Open(Path + "\\UserSkills\\" + Client.Account + ".bin", FileMode.Create))
            {
                DatabaseSkill db_skill = new DatabaseSkill();
                int skill_count;
                // spells
                skill_count = Client.Spells.Length;
                binary.Write(&skill_count, sizeof(int));
                for (int i = 0; i < Client.Spells.Length; i++)
                {
                    db_skill.FromSkill(Client.Spells.Elements[i]);
                    binary.Write(&db_skill, sizeof(DatabaseSkill));
                }
                // profs
                skill_count = Client.Proficiencies.Length;
                binary.Write(&skill_count, sizeof(int));
                for (int i = 0; i < Client.Proficiencies.Length; i++)
                {
                    db_skill.FromSkill(Client.Proficiencies.Elements[i]);
                    binary.Write(&db_skill, sizeof(DatabaseSkill));
                }
                binary.Close();
            }
        }
        public static void SavePlayer(GameClient Client)
        {
            IniFile wrtr = new IniFile(Path + "\\Accounts\\" + Client.Account + ".ini");
            wrtr.Write<byte>("Account", "Banned", Client.BannedFlag);
            wrtr.WriteString("Account", "LastIP", Client.NetworkSocket.IP);
            wrtr.Write<byte>("Character", "Logged", 0);
            wrtr.WriteString("Character", "Name", Client.Entity.Name);
            wrtr.WriteString("Character", "Spouse", Client.SpouseAccount);
            if (new MapSettings(Client.Entity.MapID).Status.CanSaveLocation)
            {
                wrtr.Write<uint>("Character", "MapID", Client.Entity.MapID);
                wrtr.Write<ushort>("Character", "X", Client.Entity.X);
                wrtr.Write<ushort>("Character", "Y", Client.Entity.Y);
            }
            else
            {
                wrtr.Write<uint>("Character", "MapID", Client.LastMapID);
                wrtr.Write<ushort>("Character", "X", Client.LastX);
                wrtr.Write<ushort>("Character", "Y", Client.LastY);
            }
            wrtr.Write<ushort>("Character", "Level", Client.Entity.Level);
            wrtr.Write<ushort>("Character", "Mesh", Client.Entity.Mesh);
            wrtr.Write<ushort>("Character", "Avatar", Client.Entity.Avatar);
            wrtr.Write<ushort>("Character", "RebornCount", Client.Entity.Reborn);
            wrtr.Write<uint>("Character", "NobilityID", (uint)Client.Entity.Nobility);
            wrtr.Write<ushort>("Character", "Hairstyle", Client.Entity.Hairstyle);
            wrtr.Write<int>("Character", "Health", Client.Entity.Hitpoints);
            wrtr.Write<ushort>("Character", "PKPoints", Client.PKPoints);
            wrtr.Write<byte>("Character", "Job", Client.Job);
            wrtr.Write("Character", "OriginalJob", Client.OriginalJob);
            wrtr.Write<int>("Character", "Money", Client.Money);
            wrtr.Write<int>("Character", "ConquerPoints", Client.ConquerPoints);
            wrtr.Write<int>("Character", "Mana", Client.Manapoints);
            wrtr.Write<ushort>("Character", "GuildID", Client.Guild.ID);
            wrtr.Write<uint>("Character", "GuildWarTime", Client.TimeStamps.GuildWarTime.Time);
            wrtr.Write<byte>("Character", "GuildRank", (byte)Client.Guild.Rank);
            wrtr.Write<uint>("Character", "Experience", Client.Experience);
            wrtr.Write<ushort>("Character", "Strength", Client.Stats.Strength);
            wrtr.Write<ushort>("Character", "Vitality", Client.Stats.Vitality);
            wrtr.Write<ushort>("Character", "StatPoints", Client.Stats.StatPoints);
            wrtr.Write<ushort>("Character", "Spirit", Client.Stats.Spirit);
            wrtr.Write<ushort>("Character", "Agility", Client.Stats.Agility);

            SaveItems(Client);
            SaveSkills(Client);
            SaveAssociates(Client);
            SaveMentorStudents(Client);
            Client.Guild.SaveMemberInfo();
        }

        public static void LoadMentorStudents(string Account, ref FlexibleArray<MentorStudent> Students)
        {
            BinaryFile binary = new BinaryFile();
            if (binary.Open(Path + "\\UserMentorStudents\\" + Account + ".bin", FileMode.Open))
            {
                DatabaseMentorStudent db_student;
                int student_count;
                binary.Read(&student_count, sizeof(int));
                Students.SetCapacity(student_count);
                for (int i = 0; i < student_count; i++)
                {
                    binary.Read(&db_student, sizeof(DatabaseMentorStudent));
                    Students.Add(db_student.GetStudent());
                }
                binary.Close();
            }
        }
        private static void LoadMentorStudents(GameClient Client)
        {
            LoadMentorStudents(Client.Account, ref Client.Students);
        }
        public static void LoadAssociates(string Account, ref FlexibleArray<IAssociate> Friends, ref FlexibleArray<IAssociate> Enemies)
        {
            BinaryFile binary = new BinaryFile();
            if (binary.Open(Path + "\\UserAssociates\\" + Account + ".bin", FileMode.Open))
            {
                DatabaseAssociate db_associate;
                int associate_count;
                //friends
                binary.Read(&associate_count, sizeof(int));
                Friends.SetCapacity(associate_count);
                for (int i = 0; i < associate_count; i++)
                {
                    binary.Read(&db_associate, sizeof(DatabaseAssociate));
                    Friends.Add(db_associate.GetAssociate());
                }

                // TO-DO:
                // enemies

                binary.Close();
            }
        }
        private static void LoadAssociates(GameClient Client)
        {
            LoadAssociates(Client.Account, ref Client.Friends, ref Client.Enemies);
        }
        private static void LoadItems(GameClient Client)
        {
            BinaryFile binary = new BinaryFile();
            if (binary.Open(Path + "\\UserItems\\" + Client.Account + ".bin", FileMode.Open))
            {
                ItemUsuagePacket Remove = ItemUsuagePacket.Create();
                Remove.ID = ItemUsuageID.RemoveInventory;

                DatabaseItem db_item;
                int item_count;
                uint item_old_uid;
                // inventory
                binary.Read(&item_count, sizeof(int));
                for (int i = 0; i < item_count; i++)
                {
                    binary.Read(&db_item, sizeof(DatabaseItem));
                    Item item = db_item.GetItem(out item_old_uid);
                    Remove.UID = item_old_uid;
                    Client.Send(&Remove);
                    Client.Inventory.Add(item);
                }
                // equipment
                binary.Read(&item_count, sizeof(int));
                for (ItemPosition p = Item.FirstSlot; p <= Item.LastSlot; p++)
                {
                    binary.Read(&db_item, sizeof(DatabaseItem));
                    if (db_item.ID != 0)
                        Client.Equipment[p] = db_item.GetItem();
                }
                binary.Close();
                Client.Equipment.Initialize();
            }
        }
        private static void LoadSkills(GameClient Client)
        {
            BinaryFile binary = new BinaryFile();
            if (binary.Open(Path + "\\UserSkills\\" + Client.Account + ".bin", FileMode.Open))
            {
                DatabaseSkill skill;
                int skill_count;
                //spells
                binary.Read(&skill_count, sizeof(int));
                Client.Spells.SetCapacity(skill_count);
                for (int i = 0; i < skill_count; i++)
                {
                    binary.Read(&skill, sizeof(DatabaseSkill));
                    Client.Spells.Add(skill.GetSpell());
                }
                // profs
                binary.Read(&skill_count, sizeof(int));
                Client.Proficiencies.SetCapacity(skill_count);
                for (int i = 0; i < skill_count; i++)
                {
                    binary.Read(&skill, sizeof(DatabaseSkill));
                    Client.Proficiencies.Add(skill.GetProficiency());
                }
                binary.Close();
            }
        }
        public static bool LoadPlayer(GameClient Client, int PasswordCheckSum, out bool NewCharacter)
        {
            NewCharacter = false;
            bool failed_load = true;

            Client.Account = AuthSystem.ReadString("AuthSystem", Client.Entity.UID.ToString(), "", 16);
            IniFile rdr = new IniFile(ServerDatabase.Path + "\\Accounts\\" + Client.Account + ".ini");
            string Password = rdr.ReadString("Account", "Password", "", 16);

            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(Password);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string newpassword = s.ToString().Remove(8);
            string newnewnumber = "";

            byte[] data = new byte[newpassword.Length * 2];
            for (int i = 0; i < 2; ++i)
            {
                char ch = newpassword[i];
                data[i * 2] = (byte)(ch & 0xFF);
                data[i * 2 + 1] = (byte)((ch & 0xFF00) >> 8);
                string number = data[i * 2].ToString() + data[i * 2 + 1].ToString();
                newnewnumber = newnewnumber + number;
            }

            Console.WriteLine("Password Check Sum = " + PasswordCheckSum + "        " + Password.GetHashCode());
            if (newnewnumber.Equals(PasswordCheckSum.ToString()))
            {
                if (true)
                {
                    try
                    {
                        DateTime LastLogin = DateTime.Parse(rdr.ReadString("Account", "LastLoginServer", DateTime.Now.AddMinutes(-10).ToString()));
                        if (LastLogin > DateTime.Now.AddSeconds(10))
                            throw new Exception("");

                        string rdrName = rdr.ReadString("Character", "Name", "INVALIDNAME", 16);
                        if (rdrName != "INVALIDNAME")
                        {
                            rdr.Write<byte>("Character", "Logged", 1);
                            Client.BannedFlag = rdr.ReadByte("Account", "Banned", 0);
                            Client.AdminFlag = rdr.ReadByte("Character", "GM", 0);
                            Client.Entity.Name = rdrName;
                            Client.Entity.UID = rdr.ReadUInt32("Character", "UID", 0);
                            Client.Spouse = "None";
                            Client.SpouseAccount = rdr.ReadString("Character", "Spouse", "", 16);
                            if (Client.SpouseAccount != "")
                            {
                                rdrName = rdr.FileName;
                                rdr.FileName = Path + @"\Accounts\" + Client.SpouseAccount + ".ini";
                                if (rdr.ReadString("Character", "Spouse", "", 16).Equals(Client.Account, StringComparison.OrdinalIgnoreCase))
                                {
                                    Client.Spouse = rdr.ReadString("Character", "Name", "", 16);
                                    if (Client.Spouse == "")
                                        Client.Spouse = "None";
                                }
                                rdr.FileName = rdrName;
                            }

                            Client.Entity.MapID = rdr.ReadUInt16("Character", "MapID", 1002);
                            Client.Entity.X = rdr.ReadUInt16("Character", "X", 400);
                            Client.Entity.Y = rdr.ReadUInt16("Character", "Y", 400);
                            if (Client.CurrentDMap == null)
                            {
                                Client.Entity.MapID = 1002;
                                Client.Entity.X = 400;
                                Client.Entity.Y = 400;
                            }

                            Client.Entity.Level = Math.Min((ushort)130, rdr.ReadUInt16("Character", "Level", 1));
                            Client.Entity.Mesh = rdr.ReadUInt16("Character", "Mesh", 1003);
                            Client.Entity.Avatar = rdr.ReadUInt16("Character", "Avatar", 0);
                            Client.Entity.Reborn = rdr.ReadByte("Character", "RebornCount", 0);
                            Client.Entity.Hairstyle = rdr.ReadUInt16("Character", "Hairstyle", 421);
                            Client.Entity.Hitpoints = rdr.ReadInt32("Character", "Health", Client.Entity.MaxHitpoints);//xXxhp (fixed so you start with your max hp)  [Client.Entity.MaxHitpoints]
                            Client.Entity.Nobility = (NobilityID)rdr.ReadUInt32("Character", "Nobility", 0);

                            Client.AddPKPoints(rdr.ReadUInt16("Character", "PKPoints", 0));
                            Client.Job = rdr.ReadByte("Character", "Job", 10);
                            Client.OriginalJob = rdr.ReadByte("Character", "OriginalJob", Client.Job);
                            Client.Money = rdr.ReadInt32("Character", "Money", 100);
                            Client.ConquerPoints = rdr.ReadInt32("Character", "ConquerPoints", 0);
                            Client.Manapoints = rdr.ReadUInt16("Character", "Mana", 0);//xXxmp
                            Client.Experience = rdr.ReadUInt32("Character", "Experience", 0);

                            Client.Guild.ID = rdr.ReadUInt16("Character", "GuildID", 0);
                            Client.Guild.Rank = (GuildRank)rdr.ReadByte("Character", "GuildRank", 0);
                            Client.TimeStamps.GuildWarTime = new TIME(rdr.ReadUInt32("Character", "GuildWarTime", 0));

                            if (Client.Entity.Reborn > 0)
                            {
                                Client.Stats.Strength = rdr.ReadUInt16("Character", "Strength", 0);
                                Client.Stats.Vitality = rdr.ReadUInt16("Character", "Vitality", 0);
                                Client.Stats.StatPoints = rdr.ReadUInt16("Character", "StatPoints", 0);
                                Client.Stats.Spirit = rdr.ReadUInt16("Character", "Spirit", 0);
                                Client.Stats.Agility = rdr.ReadUInt16("Character", "Agility", 0);
                                if (Client.Stats.Strength + Client.Stats.Vitality + Client.Stats.StatPoints + Client.Stats.Spirit + Client.Stats.Agility > 422)
                                {
                                    Client.Stats.Strength = Client.Stats.Vitality = Client.Stats.Spirit = Client.Stats.Agility = 0;
                                    Client.Stats.StatPoints = 422;
                                }
                            }
                            else
                            {
                                fixed (StatData* pStats = &Client.Stats)
                                    GetStats(Client.Job, Client.Entity.Level, pStats);
                            }

                            LoadItems(Client);
                            LoadSkills(Client);
                            LoadAssociates(Client);
                            LoadMentorStudents(Client);

                            failed_load = false;
                        }
                        else
                        {
                            NewCharacter = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Kernel.NotifyDebugMsg("[Database - Load Character]", e.ToString(), true);
                        Client.Send(MessageConst.FAILED_LOAD_CHARACTER);
                    }
                }
                if (!failed_load && !NewCharacter)
                {
                    Client.ServerFlags |= ServerFlags.LoadedCharacter;
                    return true;
                }
            }
            return false;
        }
        public static bool ValidCharacterName(string CharacterName, bool SquareBrackets)
        {
            const byte Dash = 45; // "-"
            const byte Underscore = 95; // "_"
            const byte NumStart = 48; // "0"
            const byte NumEnd = 57; // "9"
            const byte UpperStart = 65; // "A"
            const byte UpperEnd = 90; // "Z"
            const byte LowerStart = 97; // "a"
            const byte LowerEnd = 122; // "z"
            const byte LeftSquare = (byte)'[';
            const byte RightSquare = (byte)']';
            const byte VerticleLine = (byte)'|';

            for (byte i = 0; i < CharacterName.Length; i++)
            { 
                byte c = (byte)CharacterName[i];
                if (c == Dash || c == Underscore || c == VerticleLine)
                    continue;
                else if (c >= NumStart && c <= NumEnd)  
                    continue;
                else if (c >= UpperStart && c <= UpperEnd)
                    continue;
                else if (c >= LowerStart && c <= LowerEnd)
                    continue;
                else
                {
                    if (SquareBrackets)
                    {
                        if (c == LeftSquare)
                            continue;
                        else if (c == RightSquare)
                            continue;
                    }
                }
                return false;
            }
            return true;
        }
        public static bool CharacterExists(string CharacterName)
        {
            if (File.Exists(RegisteredNames.FileName))
            {
                string search = "=" + CharacterName;
                using (StreamReader reader = new StreamReader(RegisteredNames.FileName, Encoding.UTF8))
                {
                    string str;
                    while ((str = reader.ReadLine()) != null)
                    {
                        if (str.EndsWith(search))
                            return true;
                    }
                }
            }
            return false;
        }
        public static void CreateAccount(string CharacterName, ushort Mesh, ushort Job, string Account)
        {
            lock (RegisteredNames)
            {
                RegisteredNames.WriteString("Registry", Account.ToLower(), CharacterName);
            }
            IniFile sql = new IniFile(Path + "\\Accounts\\" + Account + ".ini");
            if (File.Exists(sql.FileName))
            {
                sql.WriteString("Character", "Name", CharacterName);
                sql.Write<ushort>("Character", "Mesh", Mesh);
                if (((int)(Mesh / 1000)) == 2)
                    sql.Write<byte>("Character", "Avatar", 201);
                else
                    sql.Write<byte>("Character", "Avatar", 1);
                sql.Write<ushort>("Character", "Job", Job);
                sql.Write<ushort>("Character", "MapID", 1002);
                sql.Write<ushort>("Character", "X", 439);
                sql.Write<ushort>("Character", "Y", 387);
            }
        }
    }
}
