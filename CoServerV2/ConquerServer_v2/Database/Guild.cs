using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Database
{
    public unsafe class Guild
    {
        private class GuildMemberListComparer : IComparer<string>
        {
            public static GuildMemberListComparer Cmp = new GuildMemberListComparer();
            public int Compare(string s1, string s2)
            {
                bool s1_Online = s1.EndsWith("1");
                bool s2_Online = s2.EndsWith("1");

                if (s1_Online == s2_Online)
                {
                    byte s1_Level = byte.Parse(s1.Split(' ')[1]);
                    byte s2_Level = byte.Parse(s2.Split(' ')[1]);
                    if (s1_Level == s2_Level)
                        return 0;
                    return (s1_Level > s2_Level) ? -1 : 1;
                }

                return s1_Online ? -1 : 1;
            }
        }

        private string m_Name;
        private GameClient Client;
        private object m_SyncObject;
        private IniFile GuildIni;

        private const string Bulletin_Default = "No message.";
        private static Dictionary<ushort, byte[]> NamesDictionary;
        private static Dictionary<ushort, object> SyncDictionary;
        private static bool IsInGuild(string Account, ushort ID)
        {
            IniFile file = new IniFile(ServerDatabase.Path + "\\Accounts\\" + Account + ".ini");
            return (file.ReadUInt16("Character", "GuildID", 0) == ID && ID != 0);
        }
        private static void InitGuild(ushort ID)
        {
            string ini_path = ServerDatabase.Path + @"\Guilds\" + ID.ToString() + ".ini";
            if (File.Exists(ini_path))
            {
                IniFile GuildIni = new IniFile(ini_path);
                int checksum = GuildIni.ReadInt32("Main", "MemberCount", 1);
                int EstimatedSize = 6 + checksum * (IniFile.Int32_Size * 2);
                if (EstimatedSize > 0)
                {
                    string[] Sections = GuildIni.GetSectionNames(EstimatedSize);
                    if (Sections.Length > 3)
                    {
                        if (Sections.Length-3 != checksum)
                        {
                            for (int i = 3; i < Sections.Length; i++)
                            {
                                string Account = GuildIni.ReadString(Sections[i], "Account", "", 16);
                                if (!IsInGuild(Account, ID))
                                {
                                    GuildIni.DeleteSection(Sections[i]);
                                }
                            }
                        }
                    }
                }
            }
        }
        private static object GetSyncObject(ushort ID)
        {
            object result;
            lock (SyncDictionary)
            {
                if (!SyncDictionary.TryGetValue(ID, out result))
                {
                    InitGuild(ID);
                    result = new object();
                    SyncDictionary.Add(ID, result);
                }
            }
            return result;
        }

        static Guild()
        {
            NamesDictionary = new Dictionary<ushort, byte[]>();
            SyncDictionary = new Dictionary<ushort, object>();
        }

        private static ushort NextGuildID
        {
            get
            {
                IniFile GuildCounter = new IniFile(ServerDatabase.Path + @"\Guilds\Settings.ini");
                ushort Amount = GuildCounter.ReadUInt16("Settings", "Counter", 1);
                GuildCounter.Write<ushort>("Settings", "Counter", (ushort)(Amount + 1));
                return Amount;
            }
        }
        private static void RegisterGuildName(string Name, ushort GuildID)
        {
            IniFile RegisteredGuildNames = new IniFile(ServerDatabase.Path + @"\Guilds\RegisteredGuildNames.ini");
            RegisteredGuildNames.WriteString("Registry", GuildID.ToString(), Name);
            RegisteredGuildNames.Write<ushort>("RegistryV2", Name, GuildID);
        }
        private static void UnregisterGuildName(string Name, ushort GuildID)
        {
            IniFile RegisteredGuildNames = new IniFile(ServerDatabase.Path + @"\Guilds\RegisteredGuildNames.ini");
            RegisteredGuildNames.DeleteKey("Registry", GuildID.ToString());
            RegisteredGuildNames.DeleteKey("RegistryV2", Name);
        }
        private static bool GuildNameExists(string Name)
        {
            string search = "=" + Name;
            using (StreamReader reader = new StreamReader(ServerDatabase.Path + @"\Guilds\RegisteredGuildNames.ini", System.Text.Encoding.UTF8))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    if (str.EndsWith(search))
                        return true;
                    if (str.StartsWith("[RegistryV2]"))
                        break;
                }
            }
            return false;
        }
        public static bool GetGuildName(ushort GuildID, out string Name)
        {
            Name = "None";
            IniFile guild = new IniFile(ServerDatabase.Path + @"\Guilds\" + GuildID.ToString() + ".ini");
            if (File.Exists(guild.FileName))
            {
                Name = guild.ReadString("Main", "Name", "None", 16);
                return true;
            }
            return false;
        }
        private static bool GetGuildID(string Name, out ushort GuildID)
        {
            IniFile RegisteredGuildNames = new IniFile(ServerDatabase.Path + @"\Guilds\RegisteredGuildNames.ini");
            GuildID = RegisteredGuildNames.ReadUInt16("RegistryV2", Name, 0);
            return (GuildID != 0);
        }
        private static uint FindPlayerSection(ushort GuildID, string Name)
        {
            IniFile GuildIni = new IniFile(ServerDatabase.Path + @"\Guilds\" + GuildID.ToString() + ".ini");
            int EstimatedSize = 6 + (GuildIni.ReadInt32("Main", "MemberCount", 1) * (IniFile.Int32_Size * 2));
            string[] Sections = GuildIni.GetSectionNames(EstimatedSize);
            for (int i = 1; i < Sections.Length; i++)
            {
                if (GuildIni.ReadString(Sections[i], "Name", "", 16) == Name)
                {
                    return GuildIni.ReadUInt32(Sections[i], "UID", 0);
                }
            }
            return 0;
        }

        public ushort ID
        {
            get { return Client.Entity.GuildID; }
            set
            {
                if (value != 0)
                {
                    string ini_path = ServerDatabase.Path + @"\Guilds\" + value.ToString() + ".ini";
                    if (File.Exists(ini_path))
                    {
                        Client.Entity.GuildID = value;
                        GuildIni.FileName = ini_path;
                        m_Name = GuildIni.ReadString("Main", "Name", "", 16);
                    }
                }
                else
                {
                    Client.Entity.Spawn.GuildID = 0;
                    m_Name = null;
                }
                m_SyncObject = GetSyncObject(ID);
            }
        }
        public string Name { get { return m_Name; } }
        public GuildRank Rank
        {
            get
            {
                if (ID == 0)
                    return GuildRank.None;
                return Client.Entity.Spawn.GuildRank;
            }
            set { Client.Entity.GuildRank = value; }
        }

        public Guild(GameClient owner)
        {
            Client = owner;
            GuildIni = new IniFile();
        }

        /// <summary>
        /// Creates a new guild, this should not be called if ID is not zero. 
        /// This function will fail if the name is already in use.
        /// This function will fail if the name's length is greater than 15.
        /// This function should be called on a new thread to ensure speed.
        /// </summary>
        /// <param name="Name">The name of the new guild</param>
        public bool CreateNew(string Name)
        {
            if (ID == 0)
            {
                if (Name.Length <= 15)
                {
                    if (!GuildNameExists(Name))
                    {
                        if (Client.Money >= 1000000)
                        {
                            Client.Money -= 1000000;
                            UpdatePacket Update = UpdatePacket.Create();
                            Update.UID = Client.Entity.UID;
                            Update.ID = UpdateID.Money;
                            Update.Value = (uint)Client.Money;
                            Client.Send(&Update);

                            lock (m_SyncObject)
                            {
                                ushort tempID = NextGuildID;
                                RegisterGuildName(Name, tempID);
                                GuildIni.FileName = ServerDatabase.Path + @"\Guilds\" + tempID.ToString() + ".ini";
                                GuildIni.WriteString("Main", "Name", Name);
                                GuildIni.WriteString("Main", "Leader", Client.Entity.Name);
                                GuildIni.Write<uint>("Main", "LeaderUID", Client.Entity.UID);
                                GuildIni.Write<uint>("Main", "Fund", 500000);
                                GuildIni.Write<int>("Main", "MemberCount", 1);
                                GuildIni.Write<ushort>("Main", "ID", tempID);
                                GuildIni.Write<int>("Main", "Deputies", 0);
                                GuildIni.WriteString("Main", "Bulletin", Bulletin_Default);
                                this.Client.Entity.GuildID = tempID;
                                this.Client.Entity.GuildRank = GuildRank.Leader;
                                this.m_Name = Name;
                                for (int i = 0; i < 5; i++)
                                {
                                    string istr = i.ToString();
                                    GuildIni.WriteString("Ally", istr, "0");
                                    GuildIni.WriteString("Enemy", istr, "0");
                                }
                                SaveMemberInfo();
                            }

                            GuildInfoPacket Info = GuildInfoPacket.Create();
                            QueryInfo(&Info);
                            Client.Send(&Info);
                            SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, QueryName(), null);
                            Client.Respawn();
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// This function will delete a guild and kick all members out of he guild.
        /// The ID should not be zero.
        /// Warning: Guild Leader is not checked.
        /// This should be ran on a new thread to ensure speed.
        /// </summary>
        public bool Delete()
        {
            if (ID != 0)
            {
                lock (m_SyncObject)
                {
                    ushort old_id = ID;
                    File.Delete(GuildIni.FileName);
                    UnregisterGuildName(Name, ID);

                    ID = 0;
                    Rank = GuildRank.None;
                    GuildInfoPacket Info = GuildInfoPacket.Create();
                    QueryInfo(&Info);
                    Client.Send(&Info);

                    foreach (GameClient eClient in Kernel.Clients)
                    {
                        if (eClient.Guild.ID == old_id)
                        {
                            eClient.Guild.ID = 0;
                            eClient.Guild.Rank = GuildRank.None;
                            eClient.Send(&Info);
                            eClient.Respawn();
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Donates a certain amount of gold from this player to the guild.
        /// </summary>
        /// <param name="Amount">The amount, this should be above zero.</param>
        public void Donate(int Amount)
        {
            if (Client.Money >= Amount && Amount > 0)
            {
                lock (m_SyncObject)
                {
                    Client.Money -= Amount;
                    UpdatePacket update = UpdatePacket.Create();
                    update.ID = UpdateID.Money;
                    update.UID = Client.Entity.UID;
                    update.Value = (uint)Client.Money;
                    Client.Send(&update);

                    string Section = Client.Entity.UID.ToString();
                    uint Donation = GuildIni.ReadUInt32(Section, "Donation", 0);
                    Donation = Math.Min(int.MaxValue, (uint)(Donation + Amount));
                    GuildIni.Write<uint>(Section, "Donation", Donation);
                    uint Fund = GuildIni.ReadUInt32("Main", "Fund", 0);
                    Fund += (uint)Donation;
                    GuildIni.Write<uint>("Main", "Fund", Fund);

                    GuildInfoPacket Info = GuildInfoPacket.Create();
                    QueryInfo(&Info);
                    Client.Send(&Info);
                }
            }
        }
        /// <summary>
        /// Leaves a guild if ID doesn't equal zero.
        /// This function will not be processed if the person owning this class is the leader.
        /// </summary>
        public void Leave()
        {
            if (ID != 0)
            {
                lock (m_SyncObject)
                {
                    if (Rank != GuildRank.Leader)
                    {
                        if (Rank == GuildRank.DeputyLeader)
                        {
                            GuildIni.Write<int>("Main", "Deputies", GuildIni.ReadInt32("Main", "Deputies", 1) - 1);
                        }
                        GuildIni.DeleteSection(Client.Entity.UID.ToString());
                        GuildIni.Write<int>("Main", "MemberCount", GuildIni.ReadInt32("Main", "MemberCount", 1) - 1);


                        ID = 0;
                        Rank = GuildRank.None;

                        GuildInfoPacket Info = GuildInfoPacket.Create();
                        QueryInfo(&Info);
                        Client.Send(&Info);
                        Client.Respawn();
                    }
                }
            }
        }
        /// <summary>
        /// Kicks somebody out of the guild.
        /// This should be a last resort, as in, if they're offline.
        /// </summary>
        /// <param name="Name">The name of the person to kick out.</param>
        public void KickOut(string Name)
        {
            if (ID != 0)
            {
                if (Rank == GuildRank.Leader)
                {
                    lock (m_SyncObject)
                    {
                        uint ExileSectionUID = FindPlayerSection(ID, Name);
                        if (ExileSectionUID != 0)
                        {
                            string ExileSection = ExileSectionUID.ToString();
                            GuildRank ExileRank = (GuildRank)GuildIni.ReadByte(ExileSection, "Rank", (byte)GuildRank.None);
                            if (ExileRank == GuildRank.DeputyLeader)
                            {
                                GuildIni.Write<int>("Main", "Deputies", GuildIni.ReadInt32("Main", "Deputies", 1) - 1);
                            }

                            IniFile ExileFile = new IniFile(ServerDatabase.Path + @"\Accounts\" + GuildIni.ReadString(ExileSection, "Account", "", 16) + ".ini");
                            ExileFile.Write<ushort>("Character", "GuildID", 0);
                            ExileFile.Write<byte>("Character", "GuildRank", 0);

                            GuildIni.DeleteSection(ExileSection);
                            GuildIni.Write<int>("Main", "MemberCount", GuildIni.ReadInt32("Main", "MemberCount", 1) - 1);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Joins a guild, ID must be zero for this action to take place.
        /// </summary>
        /// <param name="GuildID">The ID of the guild to join.</param>
        public void Join(ushort GuildID)
        {
            ID = GuildID;
            if (ID != 0)
            {
                lock (m_SyncObject)
                {
                    Rank = GuildRank.Member;
                    GuildIni.Write<int>("Main", "MemberCount", GuildIni.ReadInt32("Main", "MemberCount", 0) + 1);
                    SaveMemberInfo();

                    GuildInfoPacket Info = GuildInfoPacket.Create();
                    QueryInfo(&Info);
                    Client.Send(&Info);
                    Client.Send(QueryName());
                    Client.Respawn();
                }
            }
        }
        /// <summary>
        /// Saves the owner of this classes memory info about a guild.
        /// Nothing happens if ID is equal to zero.
        /// </summary>
        public void SaveMemberInfo()
        {
            lock (m_SyncObject)
            {
                string Section = Client.Entity.UID.ToString();
                GuildIni.Write<uint>(Section, "UID", Client.Entity.UID);
                GuildIni.Write<ushort>(Section, "Level", Client.Entity.Level);
                GuildIni.Write<byte>(Section, "Rank", (byte)Rank);
                GuildIni.WriteString(Section, "Name", Client.Entity.Name);
                GuildIni.WriteString(Section, "Account", Client.Account);
            }
        }
        /// <summary>
        /// Adds a new deputy to the lot (if Deputies are less than 5).
        /// Warning: this does not check whether the owner of this class is the guild leader,
        /// if if the person you're giving deputy status from is in the same guild.
        /// </summary>
        /// <param name="NewDeputy">The person to make a deputy.</param>
        public void AddDeputy(GameClient NewDeputy)
        {
            lock (m_SyncObject)
            {
                int d_count = GuildIni.ReadInt32("Main", "Deputies", 0);
                if (d_count < 5)
                {
                    GuildIni.Write<int>("Main", "Deputies", d_count + 1);
                    NewDeputy.Guild.Rank = GuildRank.DeputyLeader;
                    NewDeputy.Guild.SaveMemberInfo();

                    GuildInfoPacket Info = GuildInfoPacket.Create();
                    NewDeputy.Guild.QueryInfo(&Info);
                    NewDeputy.Send(&Info);
                    NewDeputy.Respawn();
                }
            }
        }
        /// <summary>
        /// Strips a person of their deputy leader status.
        /// Warning: this does not check whether the owner of this class is the guild leader,
        /// if if the person you're removing deputy status from is in the same guild.
        /// </summary>
        /// <param name="OldDeputy">The old leader, if this field is null, this function will take longer to process.</param>
        /// <param name="DeputyName">If OldDeputy is null, this field must not be null, otherwise this field can be null.</param>
        public void RemoveDeputy(GameClient OldDeputy, string DeputyName)
        {
            lock (m_SyncObject)
            {
                if (OldDeputy != null)
                {
                    if (OldDeputy.Guild.Rank == GuildRank.DeputyLeader)
                    {
                        GuildIni.Write<int>("Main", "Deputies", GuildIni.ReadInt32("Main", "Deputies", 0) - 1);
                        OldDeputy.Guild.Rank = GuildRank.Member;
                    }
                    OldDeputy.Guild.SaveMemberInfo();

                    GuildInfoPacket Info = GuildInfoPacket.Create();
                    OldDeputy.Guild.QueryInfo(&Info);
                    OldDeputy.Send(&Info);
                    OldDeputy.Respawn();
                }
                else
                {
                    uint SectionID = FindPlayerSection(ID, DeputyName);
                    if (SectionID != 0)
                    {
                        string Section = SectionID.ToString();
                        GuildRank OldRank = (GuildRank)GuildIni.ReadByte(Section, "Rank", (byte)GuildRank.None);
                        if (OldRank == GuildRank.DeputyLeader)
                        {
                            GuildIni.Write<int>("Main", "Deputies", GuildIni.ReadInt32("Main", "Deputies", 0) - 1);
                            GuildIni.Write<byte>(Section, "Rank", (byte)GuildRank.Member);

                            IniFile OldDeputyAccount = new IniFile(ServerDatabase.Path + @"\Accounts\" + GuildIni.ReadString(Section, "Account", "", 16) + ".ini");
                            OldDeputyAccount.Write<byte>("Character", "GuildRank", (byte)GuildRank.Member);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Queries information about the owner of this class pertaining to the guild.
        /// </summary>
        /// <param name="pInfo">A pointer to a structure that should receive the information.</param>
        public void QueryMemberInfo(GuildMemberInfoPacket* pInfo)
        {
            string Section = Client.Entity.UID.ToString();
            pInfo->Donation = GuildIni.ReadInt32(Section, "Donation", 0);
            pInfo->GuildRank = Rank;
            GuildIni.ReadString(Section, "Name", "", 16).CopyTo(pInfo->MemberName); 
        }
        /// <summary>
        /// Queries the guild about it's information.
        /// </summary>
        /// <param name="pInfo">A pointer to the structure that should receive the information.</param>
        public void QueryInfo(GuildInfoPacket* pInfo)
        {
            string Section = Client.Entity.UID.ToString();
            pInfo->ID = ID;
            if (ID != 0)
            {
                pInfo->Donation = GuildIni.ReadUInt32(Section, "Donation", 0);
                pInfo->Rank = Rank;
                pInfo->Fund = GuildIni.ReadUInt32("Main", "Fund", 0);
                pInfo->MemberCount = GuildIni.ReadUInt32("Main", "MemberCount", 0);
                GuildIni.ReadString("Main", "Leader", "", 16).CopyTo(pInfo->Leader);
            }
        }
        /// <summary>
        /// Queries the guild for it's name in a packet format (StringPacket) to be sent to the client.
        /// If ID is zero, the return will be null.
        /// </summary>
        public byte[] QueryName()
        {
            byte[] buffer = null;
            if (ID != 0)
            {
                if (!NamesDictionary.TryGetValue(ID, out buffer))
                {
                    StringPacket temp = new StringPacket();
                    temp.ID = StringID.GuildName;
                    temp.UID = ID;
                    temp.Strings = new string[1];
                    temp.Strings[0] = Name + " Penis 2 " + ID;
                    temp.StringsLength = (byte)temp.Strings[0].Length;
                    buffer = temp;
                    NamesDictionary.Add(ID, buffer);
                }
            } 
            return buffer;
        }
        /// <summary>
        /// Queries the guild for it's bulletin is packet format (MessagePacket) to be sent to the client.
        /// If ID is zero, the return will be null.
        /// </summary>
        /// <param name="NewBulletin">The new bulletin, if this is null, no new bulletin is set</param>
        public byte[] QueryBulletin(string NewBulletin)
        {
            byte[] buffer = null;
            if (ID != 0)
            {
                MessagePacket msg = new MessagePacket();
                msg.ChatType = ChatID.Bulletin;
                msg.To = "ALL";
                msg.From = "SYSTEM";
                if (NewBulletin == null)
                    msg.Message = GuildIni.ReadString("Main", "Bulletin", Bulletin_Default, 255);
                else
                {
                    lock (m_SyncObject)
                    {
                        GuildIni.WriteString("Main", "Bulletin", NewBulletin);
                        msg.Message = NewBulletin;
                    }
                }
                msg.Color = 0x00FFFFFF;
                buffer = msg;
            }
            return buffer;
        }
        /// <summary>
        /// Queries the member list and gets the next 10 (if possible, or less) members.
        /// </summary>
        /// <param name="StartAt">Where to start getting the values at</param>
        /// <returns></returns>
        public byte[] QueryMemberList(int StartAt)
        {
            StringPacket Packet = new StringPacket();
            Packet.ID = StringID.GuildMemberList;
            Packet.StringsLength = 0;
            Packet.Strings = new string[0];

            StartAt = Math.Max(StartAt, 0);
            if (StartAt % 10 == 0)
            {
                int EstimatedSize = 6 + (GuildIni.ReadInt32("Main", "MemberCount", 1) * (IniFile.Int32_Size * 2));
                if (EstimatedSize > 0)
                {
                    string[] Sections = GuildIni.GetSectionNames(EstimatedSize);
                    if (Sections.Length > 3)
                    {
                        string[] MemberList = new string[Sections.Length - 3];
                        if (MemberList.Length > 0)
                        {
                            for (int i = 3; i < Sections.Length; i++)
                            {
                                string Name = GuildIni.ReadString(Sections[i], "Name", "INVALID", 16);
                                string Level = GuildIni.ReadString(Sections[i], "Level", "1", 4);
                                string Account = GuildIni.ReadString(Sections[i], "Account", "", 16);
                                string Online = (ServerDatabase.AccountOnline(Account) ? "1" : "0");
                                MemberList[i - 3] = Name + " " + Level + " " + Online;
                            }
                            Array.Sort(MemberList, GuildMemberListComparer.Cmp);

                            int Count = Math.Max(Math.Min(MemberList.Length - StartAt, 10), 0);
                            Packet.Strings = new string[Count];
                            Packet.UID = (uint)(StartAt + Count);
                            for (int i = 0; i < Count; i++)
                            {
                                Packet.Strings[i] = MemberList[StartAt + i];
                                Packet.StringsLength += (byte)Packet.Strings[i].Length;
                            }
                        }
                    }
                }
            }

            return Packet;
        }
        /// <summary>
        /// Queries the guild information at runtime to check if someone is an ally.
        /// If GuildID is zero, false is automatically returned.
        /// </summary>
        /// <param name="GuildID">The guild ID to test if they're an ally.</param>
        public bool IsAlly(ushort GuildID)
        {
            if (GuildID != 0)
            {
                for (byte i = 0; i < 5; i++)
                {
                    if (GuildIni.ReadUInt16("Ally", i.ToString(), 0) == GuildID)
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Queries the guild information at runtime to check if someone is an enemy.
        /// If the GuildID is zero, false is automatically returned.
        /// </summary>
        /// <param name="GuildID"></param>  
        /// <returns></returns>
        public bool IsEnemy(ushort GuildID)
        {
            if (GuildID != 0)
            {
                for (byte i = 0; i < 5; i++)
                {
                    if (GuildIni.ReadUInt16("Enemy", i.ToString(), 0) == GuildID)
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Adds an ally to this guild.
        /// Warning: Guild leader is not checked.
        /// This function will fail if all ally slots are full, or
        /// if the guild is already an ally, or GuildID is equal to zero.
        /// </summary>
        /// <param name="GuildID">The Guild ID of the ally to add.</param>
        /// <returns></returns>
        public bool AddAlly(string Guild)
        {
            ushort GuildID;
            GetGuildID(Guild, out GuildID);
            if (GuildID != 0)
            {
                if (!IsAlly(GuildID))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        string istr = i.ToString();
                        if (GuildIni.ReadUInt16("Ally", istr, 0) == 0)
                        {
                            GuildIni.Write<ushort>("Ally", istr, GuildID);

                            StringPacket packet = new StringPacket();
                            packet.Strings = new string[1];
                            packet.ID = StringID.AllyGuild;
                            packet.UID = GuildID;
                            GetGuildName(GuildID, out  packet.Strings[0]);
                            packet.StringsLength = (byte)packet.Strings[0].Length;
                            SendGuild(packet, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Removes an ally, if the GuildID passed is not an ally, nothing is done.
        /// </summary>
        /// <param name="GuildID">The existing ally Guild ID</param>
        public void RemoveAlly(string Guild)
        {
            ushort GuildID;
            GetGuildID(Guild, out GuildID);
            if (GuildID != 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    string istr = i.ToString();
                    if (GuildIni.ReadUInt16("Ally", istr, 0) == GuildID)
                    {
                        lock (m_SyncObject)
                        {
                            GuildIni.Write<ushort>("Ally", istr, 0);
                        }
                        // TO-DO:
                        // Send Neutral packet to everyone online
                        Client.Send(MessageConst.SUCCESS_REMOVE_GUILD);
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// Adds an Enemy to this guild.
        /// Warning: Guild leader is not checked.
        /// This function will fail if all Enemy slots are full, or
        /// if the guild is already an Enemy, or GuildID is equal to zero.
        /// </summary>
        /// <param name="GuildID">The Guild ID of the Enemy to add.</param>
        /// <returns></returns>
        public bool AddEnemy(string Guild)
        {
            ushort GuildID;
            GetGuildID(Guild, out GuildID);
            if (GuildID != 0)
            {
                if (!IsEnemy(GuildID))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        string istr = i.ToString();
                        if (GuildIni.ReadUInt16("Enemy", istr, 0) == 0)
                        {
                            GuildIni.Write<ushort>("Enemy", istr, GuildID);

                            StringPacket packet = new StringPacket();
                            packet.Strings = new string[1];
                            packet.ID = StringID.EnemyGuild;
                            packet.UID = GuildID;
                            GetGuildName(GuildID, out  packet.Strings[0]);
                            packet.StringsLength = (byte)packet.Strings[0].Length;
                            SendGuild(packet, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Removes an Enemy, if the GuildID passed is not an Enemy, nothing is done.
        /// </summary>
        /// <param name="GuildID">The existing Enemy Guild ID</param>
        public void RemoveEnemy(string Guild)
        {
            ushort GuildID;
            GetGuildID(Guild, out GuildID);
            if (GuildID != 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    string istr = i.ToString();
                    if (GuildIni.ReadUInt16("Enemy", istr, 0) == GuildID)
                    {
                        lock (m_SyncObject)
                        {
                            GuildIni.Write<ushort>("Enemy", istr, 0);
                        }
                        // TO-DO:
                        // Send Neutral packet to everyone online
                        Client.Send(MessageConst.SUCCESS_REMOVE_GUILD);
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// Sends the ally-packet for every guild allied to this guild to this class' owner.
        /// </summary>
        public void SendAllies()
        {
            StringPacket packet = new StringPacket();
            packet.Strings = new string[1];
            for (byte i = 0; i < 5; i++)
            {
                ushort AllyID = GuildIni.ReadUInt16("Ally", i.ToString(), 0);
                if (AllyID != 0)
                {
                    packet.ID = StringID.AllyGuild;
                    packet.UID = AllyID;
                    if (!GetGuildName(AllyID, out  packet.Strings[0]))
                    {
                        GuildIni.Write<ushort>("Ally", i.ToString(), 0);
                        continue;
                    }
                    packet.StringsLength = (byte)packet.Strings[0].Length;
                    Client.Send(packet);
                }
            }
        }
        /// <summary>
        /// Sends the enemy-packet for every guild enemied to this guild to this class' owner.
        /// </summary>
        public void SendEnemies()
        {
            StringPacket packet = new StringPacket();
            packet.Strings = new string[1];
            for (byte i = 0; i < 5; i++)
            {
                ushort enemyID = GuildIni.ReadUInt16("Enemy", i.ToString(), 0);
                if (enemyID != 0)
                {
                    packet.ID = StringID.EnemyGuild;
                    packet.UID = enemyID;
                    if (!GetGuildName(enemyID, out  packet.Strings[0]))
                    {
                        GuildIni.Write<ushort>("Enemy", i.ToString(), 0);
                        continue;
                    }
                    packet.StringsLength = (byte)packet.Strings[0].Length;
                    Client.Send(packet);
                }
            }
        }
        /// <summary>
        /// Send a packet to everyone inside of the guild.
        /// If ID is zero, then nothing happens.
        /// </summary>
        /// <param name="Packet">The packet to send.</param>
        public void SendGuild(byte[] Packet, bool SendSelf)
        {
            if (ID != 0)
            {
                foreach (GameClient eClient in Kernel.Clients)
                {
                    if (eClient.Guild.ID == Client.Guild.ID)
                    {
                        if (eClient.Entity.UID != Client.Entity.UID || SendSelf)
                            eClient.Send(Packet);
                    }
                }
            }
        }
        /// <summary>
        /// Passes the leader ship of this guild.
        /// Warning: Does not check if the caller is the leader
        /// </summary>
        /// <param name="NewOwner">The new owner's name</param>
        public void PassLeadership(string NewOwner)
        {
            if (ID != 0)
            {
                uint NewLeaderUID = FindPlayerSection(ID, NewOwner);
                if (NewLeaderUID != 0)
                {
                    lock (m_SyncObject)
                    {
                        string NewLeaderSection = NewLeaderUID.ToString();
                        GuildIni.WriteString("Main", "Leader", GuildIni.ReadString(NewLeaderSection, "Name", "", 16));
                        GuildIni.Write<uint>("Main", "LeaderUID", NewLeaderUID);
                        if ((GuildRank)GuildIni.ReadByte(NewLeaderSection, "Rank", (byte)GuildRank.Member) == GuildRank.DeputyLeader)
                        {
                            GuildIni.Write<int>("Main", "Deputies", GuildIni.ReadInt32("Main", "Deputies", 0) - 1);
                        }
                        GameClient NewLeader = Kernel.FindClientByUID(NewLeaderUID);
                        if (NewLeader != null)
                        {
                            NewLeader.Guild.Rank = GuildRank.Leader;
                            NewLeader.Guild.SaveMemberInfo();

                            GuildInfoPacket info = GuildInfoPacket.Create();
                            NewLeader.Guild.QueryInfo(&info);
                            NewLeader.Send(&info);

                            NewLeader.Respawn();
                        }
                        else
                        {
                            GuildIni.Write<byte>(NewLeaderSection, "Rank", (byte)GuildRank.Leader);
                            IniFile player = new IniFile(ServerDatabase.Path + @"\Accounts\" + GuildIni.ReadString(NewLeaderSection, "Account", "", 16) + ".ini");
                            player.Write<byte>("Character", "GuildRank", (byte)GuildRank.Leader);
                        }

                        Rank = GuildRank.Member;
                        Leave();
                    }
                }
            }
        }
    }
}
