using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;
using ConquerServer_v2.Core;
using ConquerServer_v2.Monster_AI;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        private static void FriendChat(object obj)
        {
            object[] box = obj as object[];
            GameClient Client = box[0] as GameClient;
            byte[] Packet = box[1] as byte[];

            foreach (GameClient iClient in Kernel.Clients)
            {
                if (iClient.Entity.UID != Client.Entity.UID)
                {
                    if (iClient.Friends.Search(Client.Entity.UID) != null &&
                        Client.Friends.Search(iClient.Entity.UID) != null)
                    {
                        iClient.Send(Packet);
                    }
                }
            }
        }

        public static void ProcessMessage(GameClient Client, MessagePacket Packet, byte[] Original)
        {
            TIME Now = TIME.Now;
            if (Now.Time - Client.TimeStamps.SendMessage.Time <= 500)
            {
                Client.Send(MessageConst.WAIT_MESSAGE);
                return;
            }

            Client.TimeStamps.SendMessage = Now;
            if (Packet.Message.StartsWith("@"))
            {
                ProcessServerCommand(Client, Packet.Message, true);
            }
            else
            {
                bool changed = false;
                if (Packet.From != Client.Entity.Name)
                {
                    Packet.From = Client.Entity.Name;
                    changed = true;
                }
                else if (Packet.ChatType == ChatID.Whisper)
                {
                    Packet.SenderModel = Client.Entity.Model;
                    changed = true;
                }

                if (changed)
                    Original = Packet;

                //
                // Handle Message/ChatTypes
                //

                switch (Packet.ChatType)
                {
                    case ChatID.Whisper:
                        {
                            if (!Client.Entity.Dead)
                            {
                                GameClient receiver = Kernel.FindClientByName(Packet.To);
                                if (receiver != null)
                                    receiver.Send(Original);
                                else
                                    Client.Send(MessageConst.PLAYER_OFFLINE);
                                Client.Send(Original);
                            }
                            break;
                        }
                    case ChatID.Ghost:
                    case ChatID.Talk:
                        {
                            SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, Client.Entity.UID, Original, null);
                            break;
                        }
                    case ChatID.Friends:
                        {
                            if (!Client.Entity.Dead)
                            {
                                new Thread(FriendChat).Start(new object[] { Client, Original });
                            }
                            break;
                        }
                    case ChatID.Team:
                        {
                            if (Client.InTeam)
                                Client.Team.SendTeamPacket(Original, false);
                            break;
                        }
                    case ChatID.Guild:
                        {
                            if (!Client.Entity.Dead)
                            {
                                if (Client.Guild.ID != 0)
                                    Client.Guild.SendGuild(Original, false);
                            }
                            break;
                        }
                    case ChatID.Bulletin:
                        {
                            if (!Client.Entity.Dead)
                            {
                                if (Client.Guild.ID != 0 && Client.Guild.Rank == GuildRank.Leader)
                                {
                                    byte[] Bulletin = Client.Guild.QueryBulletin(Packet.Message);
                                    if (Bulletin != null)
                                        Client.Guild.SendGuild(Bulletin, true);
                                }
                            }
                            break;
                        }
                    case ChatID.WorldChat:
                        {
                            if (!Client.Entity.Dead)
                            {
                                if (Client.TimeStamps.CanWorldChat.Time <= Now.Time)
                                {
                                    SendGlobalPacket.Add(Original);
                                    Client.TimeStamps.CanWorldChat = Now.AddSeconds(15);
                                }
                            }
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Handles an command (often sent by users via chat).
        /// </summary>
        /// <param name="Client">The client of the sending command.</param>
        /// <param name="Message">The command-message itself.</param>
        /// <param name="ClientSent">True if the client sent this request via a message packet.</param>
        public static void ProcessServerCommand(GameClient Client, string Message, bool ClientSent)
        {
            try
            {
                // Please #region all commands, specifying their parameters (and types)
                // in the regions name. If parameters are optional, please include them in square
                // parenthesis brackets, see @item for an example.
                // If you need to comment on a command in the #region,
                // use a semi-colon to do so, see @wrist for an example.

                string[] Commands = Message.Split(' ');
                Commands[0] = Commands[0].ToLower();
                // These are commands only accessible by npcs
                if (!ClientSent)
                {
                    switch (Commands[0])
                    {
                        #region @createguild
                        case "@createguild":
                            {
                                CreationThread.Guild(Client, Commands[1]);
                                break;
                            }
                        #endregion
                        #region @disbandguild
                        case "@disbandguild":
                            {
                                CreationThread.Guild(Client, null);
                                break;
                            }
                        #endregion
                        #region @kickguild
                        case "@kickguild":
                            {
                                if (Client.Entity.GuildRank == GuildRank.Leader)
                                {
                                    GameClient Exile = Kernel.FindClientByName(Commands[1]);
                                    if (Exile != null)
                                    {
                                        if (Exile.Entity.GuildID == Client.Entity.GuildID &&
                                            Exile.Entity.UID != Client.Entity.UID)
                                        {
                                            Exile.Guild.Leave();
                                        }
                                    }
                                    else
                                    {
                                        Client.Guild.KickOut(Commands[1]);
                                    }
                                }
                                break;
                            }
                        #endregion
                        #region @adddeputy
                        case "@adddeputy":
                            {
                                if (Client.Guild.Rank == GuildRank.Leader)
                                {
                                    GameClient deputy = Kernel.FindClientByName(Commands[1]);
                                    if (deputy != null)
                                    {
                                        if (deputy.Guild.ID == Client.Guild.ID &&
                                            deputy.Guild.Rank == GuildRank.Member)
                                        {
                                            Client.Guild.AddDeputy(deputy);
                                        }
                                    }
                                    else
                                    {
                                        Client.Send(MessageConst.PLAYER_OFFLINE);
                                    }
                                }
                                break;
                            }
                        #endregion
                        #region @removedeputy
                        case "@removedeputy":
                            {
                                if (Client.Guild.Rank == GuildRank.Leader)
                                {
                                    GameClient deputy = Kernel.FindClientByName(Commands[1]);
                                    if (deputy != null)
                                    {
                                        if (deputy.Guild.ID == Client.Guild.ID &&
                                            deputy.Guild.Rank == GuildRank.DeputyLeader)
                                        {
                                            Client.Guild.RemoveDeputy(deputy, null);
                                        }
                                    }
                                    else
                                    {
                                        Client.Guild.RemoveDeputy(null, Commands[1]);
                                    }
                                }
                                break;
                            }
                        #endregion
                        #region @addally
                        case "@addally":
                            {
                                if (Client.Guild.Rank == GuildRank.Leader)
                                {
                                    Client.Guild.AddAlly(Commands[1]);
                                }
                                break;
                            }
                        #endregion
                        #region @removeally
                        case "@removeally":
                            {
                                if (Client.Guild.Rank == GuildRank.Leader)
                                {
                                    Client.Guild.RemoveAlly(Commands[1]);
                                }
                                break;
                            }
                        #endregion
                        #region @addenemy
                        case "@addenemy":
                            {
                                if (Client.Guild.Rank == GuildRank.Leader)
                                {
                                    Client.Guild.AddEnemy(Commands[1]);
                                }
                                break;
                            }
                        #endregion
                        #region @removeenemy
                        case "@removeenemy":
                            {
                                if (Client.Guild.Rank == GuildRank.Leader)
                                {
                                    Client.Guild.RemoveEnemy(Commands[1]);
                                }
                                break;
                            }
                        #endregion
                        #region @passleader
                        case "@passleader":
                            {
                                if (Client.Guild.ID != 0 && Client.Guild.Rank == GuildRank.Leader)
                                {
                                    Client.Guild.PassLeadership(Commands[1]);
                                }
                                break;
                            }
                        #endregion
                        #region @2ndreborn (byte JobID)
                        case "@2ndreborn":
                            {
                                byte OldJob = Client.Job;
                                ProcessServerCommand(Client, "@job " + Commands[1], ClientSent);
                                ProcessServerCommand(Client, "@level 15", ClientSent);
                                ProcessServerCommand(Client, "@reallot", ClientSent);

                                /* Spells */
                                MAttackData tempAttack;
                                ushort[] RBSpells = Spell.Get2ndRebornSpells(Client.OriginalJob, OldJob, Client.Job);
                                foreach (ISkill spell in Client.Spells.ToTrimmedArray())
                                {
                                    ServerDatabase.GetMAttackData(spell.ID, spell.Level, &tempAttack);
                                    if (tempAttack.Weapon == 0)
                                        Client.UnlearnSpell(spell.ID);
                                    else
                                    {
                                        spell.Level = 0;
                                        spell.Experience = 0;
                                        spell.Send(Client);
                                    }
                                }
                                foreach (ushort spellid in RBSpells)
                                {
                                    ISkill spell = new Spell();
                                    spell.ID = spellid;
                                    spell.Level = 0;
                                    spell.Experience = 0;
                                    spell.Send(Client);
                                    Client.Spells.Add(spell);
                                }

                                Client.Entity.Reborn = 2;
                                UpdatePacket Update = UpdatePacket.Create();
                                Update.ID = UpdateID.RebornCount;
                                Update.Value = Client.Entity.Reborn;
                                Update.UID = Client.Entity.UID;
                                Client.Send(&Update);

                                /* Proficiencies */
                                foreach (ISkill prof in Client.Proficiencies.ToTrimmedArray())
                                {
                                    prof.Level = 0;
                                    prof.Send(Client);
                                }
                                /* Equipment */
                                for (ItemPosition p = Item.FirstSlot; p < Item.LastSlot; p++)
                                {
                                    if (p == ItemPosition.AttackTalisman || p == ItemPosition.DefenceTalisman ||
                                        p == ItemPosition.Bottle || p == ItemPosition.Garment)
                                        continue;

                                    Item gear = Client.Equipment[p];
                                    if (gear != null)
                                    {
                                        short level = (short)((gear.ID % 1000) / 10);
                                        if (level > 2)
                                        {
                                            level = 2;
                                            bool changed = false;
                                            StanderdItemStats std = new StanderdItemStats();
                                            uint baseid = ((gear.ID / 1000) * 1000) + (gear.ID % 10);
                                            while (!changed && level >= 0)
                                            {
                                                if (ServerDatabase.ValidItemID(baseid + (uint)(level * 10)))
                                                {
                                                    std = new StanderdItemStats(baseid + (uint)(level * 10));
                                                    changed = (std.ReqLvl <= 15);
                                                }
                                                level--;
                                            }
                                            if (changed)
                                            {
                                                ServerDatabase.UnloadItemStats(Client, gear);
                                                gear.ID = std.ItemID;
                                                ServerDatabase.LoadItemStats(Client, gear);
                                                Client.Equipment.SetSlot(gear.Position, gear.ID, gear.Color);
                                                gear.Send(Client);
                                            }
                                            else if (gear.IsItemType(ItemTypeConst.ShieldID))
                                            {
                                                Client.Unequip(gear.Position, false);
                                                gear.ID = baseid;
                                                Client.Inventory.Add(gear);
                                            }
                                        }
                                    }
                                }

                                Client.CalculateBonus();
                                Client.CalculateAttack();

                                fixed (SpawnEntityPacket* pSpawn = &Client.Entity.Spawn)
                                    SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(pSpawn), null);

                                SendGlobalPacket.Add(new MessagePacket(Client.Entity.Name + " has passed all tests and completed Second Rebirth!", 0x00FFFFFF, ChatID.Center));
                                break;
                            }
                        #endregion
                    }
                }

                // These are commands you can use if your not dead
                if (!Client.Entity.Dead)
                {
                    // GM+
                    if (Client.AdminFlag >= 1)
                    {
                        switch (Commands[0])
                        {
                            #region @xpfull
                            case "@xpfull":
                                {
                                    Client.XPSkillCounter = 100;
                                    break;
                                }
                            #endregion
                            #region @bringtome (string character); teleports character to you
                            case "@bringtome":
                                {
                                    GameClient Player = Kernel.FindClientByName(Commands[1]);
                                    if (Player != null)
                                        Player.Teleport(Client.Entity.MapID, Client.Entity.X, Client.Entity.Y);
                                    break;
                                }
                            #endregion
                            #region @bringmeto (string character); teleports you to character
                            case "@bringmeto":
                                {
                                    GameClient Player = Kernel.FindClientByName(Commands[1]);
                                    if (Player != null)
                                        Client.Teleport(Player.Entity.MapID, Player.Entity.X, Player.Entity.Y);
                                    break;
                                }
                            #endregion
                            #region @tournament (string option = create,start,end)
                            case "@tournament":
                                {
                                    switch (Commands[1].ToLower())
                                    {
                                        case "create": TournamentAI.Init(Client); break;
                                        case "start": TournamentAI.Fight(); break;
                                        case "end": TournamentAI.End(); break;
                                    }
                                    break;
                                } 
                            #endregion
                        }
                    }
                    if (Client.AdminFlag == 2)
                    {
                        //PM
                        switch (Commands[0])
                        {
                            #region @summonall; summons all players to you
                            case "@summonall":
                                {
                                    foreach (GameClient Player in Kernel.Clients)
                                    {
                                        if (Player.Entity.UID != Client.Entity.UID)
                                        {
                                            Player.Teleport(Client.Entity.MapID, Client.Entity.X, Client.Entity.Y);
                                        }
                                    }
                                    break;
                                }
                            #endregion
                        }
                    }
                    // Regular
                    switch (Commands[0])
                    {
                        #region @reborn (byte jobid)
                        case "@reborn":
                            {
                                byte TryBuffer;
                                if (Client.Entity.Reborn == 0 && byte.TryParse(Commands[1], out TryBuffer))
                                {
                                    byte OldJob = Client.Job;
                                    Client.Stats.StatPoints += Kernel.GetAttributePoints((byte)Client.Entity.Level);
                                    
                                    Client.Entity.Reborn = 1;
                                    UpdatePacket Update = UpdatePacket.Create();
                                    Update.ID = UpdateID.RebornCount;
                                    Update.Value = Client.Entity.Reborn;
                                    Update.UID = Client.Entity.UID;
                                    Client.Send(&Update);

                                    ProcessServerCommand(Client, "@job " + Commands[1], ClientSent);
                                    ProcessServerCommand(Client, "@level 15", ClientSent);
                                    ProcessServerCommand(Client, "@reallot", ClientSent);

                                    /* Spells */
                                    MAttackData tempAttack;
                                    ushort[] RBSpells = Spell.GetRebornSpells(OldJob, Client.Job);
                                    foreach (ISkill spell in Client.Spells.ToTrimmedArray())
                                    {
                                        ServerDatabase.GetMAttackData(spell.ID, spell.Level, &tempAttack);
                                        if (tempAttack.Weapon == 0)
                                            Client.UnlearnSpell(spell.ID);
                                        else
                                        {
                                            spell.Level = 0;
                                            spell.Experience = 0;
                                            spell.Send(Client);
                                        }
                                    }
                                    foreach (ushort spellid in RBSpells)
                                    {
                                        ISkill spell = new Spell();
                                        spell.ID = spellid;
                                        spell.Level = 0;
                                        spell.Experience = 0;
                                        spell.Send(Client);
                                        Client.Spells.Add(spell);
                                    }
                                    /* Proficiencies */
                                    foreach (ISkill prof in Client.Proficiencies.ToTrimmedArray())
                                    {
                                        prof.Level = 0;
                                        prof.Send(Client);
                                    }
                                    /* Equipment */
                                    for (ItemPosition p = Item.FirstSlot; p < Item.LastSlot; p++)
                                    {
                                        if (p == ItemPosition.AttackTalisman || p == ItemPosition.DefenceTalisman ||
                                            p == ItemPosition.Bottle || p == ItemPosition.Garment)
                                            continue;

                                        Item gear = Client.Equipment[p];
                                        if (gear != null)
                                        {
                                            short level = (short)((gear.ID % 1000) / 10);
                                            if (level > 2)
                                            {
                                                level = 2;
                                                bool changed = false;
                                                StanderdItemStats std = new StanderdItemStats();
                                                uint baseid = ((gear.ID / 1000) * 1000) + (gear.ID % 10);
                                                while (!changed && level >= 0)
                                                {
                                                    if (ServerDatabase.ValidItemID(baseid + (uint)(level * 10)))
                                                    {
                                                        std = new StanderdItemStats(baseid + (uint)(level * 10));
                                                        changed = (std.ReqLvl <= 15);
                                                    }
                                                    level--;
                                                }
                                                if (changed)
                                                {
                                                    ServerDatabase.UnloadItemStats(Client, gear);
                                                    gear.ID = std.ItemID;
                                                    ServerDatabase.LoadItemStats(Client, gear);
                                                    Client.Equipment.SetSlot(gear.Position, gear.ID, gear.Color);
                                                    gear.Send(Client);
                                                }
                                                else if (gear.IsItemType(ItemTypeConst.ShieldID))
                                                {
                                                    Client.Unequip(gear.Position, false);
                                                    gear.ID = baseid;
                                                    Client.Inventory.Add(gear);
                                                }
                                            }
                                        }
                                    }

                                    Client.CalculateBonus();
                                    Client.CalculateAttack();
                                    
                                    fixed (SpawnEntityPacket* pSpawn = &Client.Entity.Spawn)
                                        SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(pSpawn), null);
                                    SendGlobalPacket.Add(new MessagePacket("Congratulations " + Client.Entity.Name + " has been Reborn!", 0x00FFFFFF, ChatID.Center));
                                }
                                break;
                            }
                        #endregion
                        #region @str, @vit, @spi, @agi, @reallot
                        case "@str":
                        case "@vit":
                        case "@spi":
                        case "@agi":
                            {
                                ushort nValue = ushort.Parse(Commands[1]);
                                if (Client.Stats.StatPoints >= nValue)
                                {
                                    Client.Stats.StatPoints -= nValue;
                                    if (Commands[0] == "@str")
                                        Client.Stats.Strength += nValue;
                                    else if (Commands[0] == "@agi")
                                        Client.Stats.Agility += nValue;
                                    else if (Commands[0] == "@vit")
                                        Client.Stats.Vitality += nValue;
                                    else if (Commands[0] == "@spi")
                                        Client.Stats.Spirit += nValue;

                                    Client.CalculateBonus();
                                    Client.CalculateAttack();

                                    BigUpdatePacket big = new BigUpdatePacket(7);
                                    big.UID = Client.Entity.UID;
                                    big.AllStats(Client, 0);
                                    big.HitpointsAndMana(Client, 5);
                                    Client.Send(big);
                                }
                                break;
                            }
                        case "@reallot":
                            {
                                if (Client.Entity.Reborn > 0)
                                {
                                    ServerDatabase.GetStats(Client);

                                    Client.Stats.StatPoints = (ushort)(Client.Stats.Vitality + Client.Stats.Strength + Client.Stats.Spirit + Client.Stats.Agility);
                                    Client.Stats.StatPoints += Kernel.GetAttributePoints(Client.Entity.Level);
                                    Client.Stats.Vitality = Client.Stats.Strength = Client.Stats.Agility = Client.Stats.Spirit = 0;

                                    Client.CalculateBonus();
                                    Client.CalculateAttack();

                                    BigUpdatePacket big = new BigUpdatePacket(7);
                                    big.UID = Client.Entity.UID;
                                    big.AllStats(Client, 0);
                                    big.HitpointsAndMana(Client, 5);
                                    Client.Send(big);
                                }
                                break;
                            }
                        #endregion
                        #region @item (string name, string quality[, byte plus, byte bless, byte enchant, byte sockets])
                        case "@item":
                            {
                                if (Client.Inventory.ItemCount < 40)
                                {
                                    byte Flag;
                                    uint ID = ServerDatabase.ItemNameToID(Commands[1], Commands[2], out Flag);
                                    if (ID != 0 && Client.AdminFlag >= Flag)
                                    {
                                        StanderdItemStats item_stats = new StanderdItemStats(ID);
                                        Item item = new Item();
                                        item.ID = ID;
                                        item.Durability = item_stats.Durability;
                                        item.MaxDurability = item_stats.Durability;
                                        item.Position = ItemPosition.Inventory;
                                        if (Commands.Length > 3)
                                        {
                                            item.Plus = Math.Min(byte.Parse(Commands[3]), (byte)9);
                                            if (Commands.Length > 4)
                                            {
                                                item.Bless = Math.Min(byte.Parse(Commands[4]), (byte)7);
                                                if (Commands.Length > 5)
                                                {
                                                    item.Enchant = byte.Parse(Commands[5]);
                                                    if (Commands.Length > 6)
                                                    {
                                                        byte sockets = byte.Parse(Commands[6]);
                                                        if (sockets >= 1)
                                                            item.SocketOne = GemsConst.OpenSocket;
                                                        if (sockets >= 2)
                                                            item.SocketTwo = GemsConst.OpenSocket;
                                                    }
                                                }
                                            }
                                        }
                                        Client.Inventory.Add(item);
                                    }
                                }
                                else
                                {
                                    Client.Send(MessageConst.INVENTORY_FULL);
                                }
                                break;
                            }
                        #endregion
                        #region @mm (ushort mapid, ushort x, ushort y[, bool dynamic])
                        case "@mm":
                            {
                                MapID mapid = ushort.Parse(Commands[1]);
                                bool safe = true;
                                if (ClientSent)
                                {
                                    if (mapid == MapID.GuildWar)
                                        safe = false;
                                    if (mapid == 1070 || mapid == 1700) // 2ndrb quest
                                        safe = false;
                                }

                                if (safe)
                                {
                                    if (Commands.Length > 4 && Client.AdminFlag > 0)
                                        if (bool.Parse(Commands[4]))
                                            mapid.MakeDynamic();
                                    Client.Teleport(mapid, ushort.Parse(Commands[2]), ushort.Parse(Commands[3]), !ClientSent);
                                }
                                break;
                            }
                        #endregion
                        #region @level (ushort level)
                        case "@level":
                            {
                                ushort lvl = ushort.Parse(Commands[1]);
                                if (lvl >= Kernel.MinLevel && lvl <= Kernel.MaxLevel)
                                {
                                    Client.Entity.Level = lvl;

                                    /* TEMPORARY FIX */
                                    if (Client.Entity.Reborn == 0)
                                        ServerDatabase.GetStats(Client);
                                    else
                                        ProcessServerCommand(Client, "@reallot", ClientSent);

                                    DataPacket level = DataPacket.Create();
                                    level.ID = DataID.LevelUp;
                                    level.UID = Client.Entity.UID;
                                    level.dwParam1 = Client.Entity.Level;
                                    SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&level), null);

                                    BigUpdatePacket update = new BigUpdatePacket(6);
                                    update.UID = Client.Entity.UID;
                                    update.Append(0, UpdateID.Level, Client.Entity.Level);
                                    update.AllStats(Client, 1);
                                    Client.Send(update);
                                }
                                break;
                            }
                        #endregion
                        #region @job (byte jobid)
                        case "@job":
                            {
                                byte jobid = byte.Parse(Commands[1]);
                                int checksum = jobid / 10;
                                if (checksum == 1 || checksum == 2 || checksum == 4 || checksum == 5 ||
                                    checksum == 10 || checksum == 13 || checksum == 14)
                                {
                                    checksum = jobid % 10;
                                    if (checksum >= 0 && checksum <= 5)
                                    {
                                        Client.Job = jobid;
                                        /* TEMPORARY FIX AGAIN*/
                                        if (Client.Entity.Reborn == 0)
                                            ServerDatabase.GetStats(Client);

                                        BigUpdatePacket update = new BigUpdatePacket(6);
                                        update.UID = Client.Entity.UID;
                                        update.Append(0, UpdateID.Job, Client.Job);
                                        update.AllStats(Client, 1);
                                        Client.Send(update);
                                    }
                                }
                                break;
                            }
                        #endregion
                        #region @wrist, @tqed ; suicide
                        case "@wrist":
                        case "@tqed":
                            {
                                Client.KillPlayer(TIME.Now);
                                break;
                            }
                        #endregion
                        #region @scroll (string city_acronym) ; tc, pc, etc.
                        case "@scroll":
                            {
                                switch (Commands[1].ToLower())
                                {
                                    case "tc": Client.Teleport(1002, 430, 380); break;
                                    case "pc": Client.Teleport(1011, 195, 260); break;
                                    case "ac":
                                    case "am": Client.Teleport(1020, 566, 563); break;
                                    case "dc": Client.Teleport(1000, 500, 645); break;
                                    case "bi": Client.Teleport(1015, 723, 573); break;
                                    case "pka": Client.Teleport(1005, 050, 050); break;
                                    case "ma": Client.Teleport(1036, 211, 196); break;
                                    case "ja": Client.Teleport(6000, 100, 100); break;
                                }
                                break;
                            }
                        #endregion
                        #region @hairstyle
                        case "@hairstyle":
                            {
                                Client.Entity.Hairstyle = ushort.Parse(Commands[1]);
                                UpdatePacket Update = UpdatePacket.Create();
                                Update.UID = Client.Entity.UID;
                                Update.ID = UpdateID.Hairstyle;
                                Update.Value = Client.Entity.Hairstyle;
                                SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&Update), null);
                                break;
                            }
                        #endregion
                        #region @debug ; reports status on thread-usuage
                        case "@debug":
                            {
                                string[] debugStrings = new string[]
                                    {
                                        "Creation Events: " + CreationThread.PendingThreads.ToString(),
                                        "SRP Active Threads: " + SendRangePacket.PendingThreads.ToString(),
                                        "SGP Active Threads: " + SendGlobalPacket.PendingThreads.ToString(),
                                        "Attack Events: " + AttackSystem.PendingThreads,
                                        "Script Threads: " + ExecuteScriptThread.PendingThreads.ToString(),
                                        "Monster AI: " + MonsterSpawn.PendingThreads.ToString()
                                    };
                                Client.Send(MessageConst.CLEAR_TOP_RIGHT);
                                MessagePacket Dbg = new MessagePacket("", 0xCCCC00, ChatID.TopRight);
                                foreach (string DebugString in debugStrings)
                                {
                                    Dbg.Message = DebugString;
                                    Client.Send(Dbg);
                                }
                                break;
                            }
                        #endregion
                    }
                }
                // These are commands you can use whether your alive, or dead
                if (Client.AdminFlag == 2)
                {
                    switch (Commands[0])
                    {
                        #region @infov2 (string character)
                        case "@infov2":
                            {
                                GameClient player = Kernel.FindClientByName(Commands[1]);
                                if (player != null)
                                {
                                    MessagePacket Msg = new MessagePacket(null, 0x00FF0000, ChatID.TopLeft);
                                    Msg.Message = string.Format("Account: {0}, IP: {1}", player.Account, player.NetworkSocket.IP);
                                    Client.Send(Msg);
                                }
                                break;
                            }
                        #endregion
                        #region @permaban (string character)
                        case "@permaban":
                            {
                                GameClient player = Kernel.FindClientByName(Commands[1]);
                                if (player != null)
                                {
                                    player.BannedFlag = 2;
                                    player.NetworkSocket.Disconnect();
                                }
                                break;
                            }
                        #endregion
                        #region @permaunban (string account) ; account = login name
                        case "@permaunban":
                            {
                                IniFile ini = new IniFile(ServerDatabase.Path + @"\Accounts\" + Commands[1] + ".ini");
                                if (System.IO.File.Exists(ini.FileName))
                                {
                                    ini.Write<byte>("Account", "Banned", 4);
                                }
                                break;
                            }
                        #endregion
                        #region @ninjahax
                        case "@ninjahax":
                            {
                                new Thread(
                                    delegate()
                                    {
                                        DataPacket ninja = DataPacket.Create();
                                        ninja.ID = DataID.NinjaStep;
                                        ninja.UID = Client.Entity.UID;
                                        ninja.dwParam1 = Client.Entity.MapID;
                                        ninja.wParam1 = (ushort)Client.Entity.Facing;

                                        foreach (IMapObject obj in Client.Screen.Objects)
                                        {
                                            if (obj.MapObjType == MapObjectType.Player)
                                            {
                                                if (Kernel.GetDistance(Client.Entity.X, Client.Entity.Y, obj.X, obj.Y) <= 24)
                                                {
                                                    Client.TimeStamps.SpawnProtection = TIME.Now.AddSeconds(1);
                                                    IBaseEntity entity = obj as IBaseEntity;
                                                    if (!entity.Dead)
                                                    {
                                                        Client.Entity.X = entity.X;
                                                        Client.Entity.Y = entity.Y;
                                                        Attack_Processor.AttackProcessor.ProcessMeele(Client.Entity, entity, AttackID.Physical);

                                                        ninja.wParam1 = Client.Entity.X;
                                                        ninja.wParam2 = Client.Entity.Y;
                                                        SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&ninja), null);
                                                        Thread.Sleep(500);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                ).Start();
                                break;
                            }
                        #endregion
                        #region @test
                        case "@test":
                            {
                                /*DateTime Now = DateTime.Now;

                                MentorStudentInfoPacket mentor = new MentorStudentInfoPacket();
                                mentor.ID = StudentInfoID.StudentInfo;
                                mentor.UID = Client.Entity.UID;
                                mentor.Student.UID = 1937509;
                                mentor.Student.Model = 1061003;
                                mentor.Student._999999 = 999999;
                                mentor.Student.Level = 130;
                                mentor.Student.Job = 55;
                                mentor.Student.PKPoints = 99;
                                mentor.Student.PlusItem = 1.00f;
                                mentor.Student.GuildID = Client.Guild.ID;
                                mentor.Student.GuildRank = GuildRank.Member;
                                mentor.Student.BlessingHours = 22;
                                mentor.Student.Experience = 2.60f;
                                mentor.Student.Online = true;
                                mentor.WriteStrings(Client.Entity.Name, "Chris315", "Bitch");
                                mentor.Student.SetJoinDate(Now.Year, Now.Month, Now.Day);
                                Client.Send(mentor);
                                
                                mentor.ID = StudentInfoID.FinishedStudentInfo;
                                mentor.ClearStrings();
                                mentor.ClearStudent();
                                Client.Send(mentor);*/
                                break;
                            }
                        #endregion
                    }
                }
                // GM+
                if (Client.AdminFlag >= 1)
                {
                    switch (Commands[0])
                    {
                        #region @info (string character)
                        case "@info":
                            {
                                GameClient player = Kernel.FindClientByName(Commands[1]);
                                if (player != null)
                                {
                                    MessagePacket Msg = new MessagePacket(null, 0x00FF0000, ChatID.TopLeft);
                                    Msg.Message = 
                                        string.Format("Account: {0}, Str: {1}, Vit: {2}, Agi: {3}, Spi: {4}, HP: {5}, MaxHP: {6}", 
                                            player.Account, 
                                            player.Stats.Strength, player.Stats.Vitality, player.Stats.Agility, player.Stats.Spirit,
                                            player.Entity.Hitpoints, player.Entity.MaxHitpoints);
                                    Client.Send(Msg);
                                }
                                break;
                            }
                        #endregion
                        #region @ban (string character)
                        case "@ban":
                            {
                                GameClient player = Kernel.FindClientByName(Commands[1]);
                                if (player != null)
                                {
                                    player.BannedFlag = 1;
                                    player.NetworkSocket.Disconnect();
                                }
                                break;
                            }
                        #endregion
                        #region @unban (string accountname) ; accountname = their login name
                        case "@unban":
                            {
                                IniFile ini = new IniFile(ServerDatabase.Path + @"\Accounts\" + Commands[1] + ".ini");
                                if (System.IO.File.Exists(ini.FileName))
                                {
                                    ini.Write<byte>("Account", "Banned", 0);
                                }
                                break;
                            }
                        #endregion
                        #region @revive ; req: Client.Entity.Dead=true
                        case "@revive":
                            {
                                Client.RevivePlayer(true);
                                break;
                            }
                        #endregion
                        #region @kick (string name) ; kick a player offline
                        case "@kick":
                            {
                                GameClient Player = Kernel.FindClientByName(Commands[1]);
                                if (Player != null)
                                {
                                    Player.NetworkSocket.Disconnect();
                                }
                                break;
                            }
                        #endregion
                        #region @gm (string message); sends message to center screen
                        case "@gm":
                            {
                                string Text = Message.Remove(0, 3);
                                SendGlobalPacket.Add(new MessagePacket(Text, 0x00FFFFFF, ChatID.Center));
                                break;
                            }
                        #endregion
                        #region @bc (string message); sends message to broadcast channel
                        case "@bc":
                            {

                                string Text = Message.Remove(0, 3);
                                SendGlobalPacket.Add(new MessagePacket(Text, "ALL", Client.Entity.Name, 0x00FFFFFF, ChatID.Broadcast)); 
                                break;
                            }
                        #endregion
                        #region @pet (string pet_name) ; spawns a pet
                        case "@pet":
                            {
                                AssignPetPacket Assignment = AssignPetPacket.Create();
                                MonsterPet.GetAssignmentData(Client, Commands[1], &Assignment);
                                Client.Send(&Assignment);
                                
                                new MonsterPet(Client, Commands[1], Assignment);
                                Client.Pet.Attach();
                                break;
                            }
                        #endregion
                    }
                }
                // Regular
                switch (Commands[0])
                {
                    #region @killpet
                    case "@killpet":
                            {
                                if (Client.Pet != null)
                                    Client.Pet.Kill(null, TIME.Now);
                                break;
                            }
                    #endregion
                    #region @quit
                    case "@quit":
                        {
                            Client.NetworkSocket.Disconnect();
                            break;
                        }
                    #endregion
                    #region @spell (ushort id, ushort level)
                    case "@spell":
                        {
                            ISkill existing;
                            ushort skill_id = ushort.Parse(Commands[1]);
                            ushort skill_lvl = ushort.Parse(Commands[2]);
                            if (Client.Spells.GetSkill(skill_id, out existing))
                            {
                                existing.Level = skill_lvl;
                                existing.Experience = 0;
                            }
                            else
                            {
                                existing = new Spell();
                                existing.ID = skill_id;
                                existing.Level = skill_lvl;
                                Client.Spells.Add(existing);
                            }
                            existing.Send(Client);
                            break;
                        }
                    #endregion
                    #region @unspell (ushort id) ; unlearn a spell
                    case "@unspell":
                        {
                            Client.UnlearnSpell(ushort.Parse(Commands[1]));
                            break;
                        }
                    #endregion
                    #region @prof (ushort id, ushort level)
                    case "@prof":
                        {
                            ISkill existing;
                            ushort skill_id = ushort.Parse(Commands[1]);
                            ushort skill_lvl = ushort.Parse(Commands[2]);
                            if (Client.Proficiencies.GetSkill(skill_id, out existing))
                            {
                                existing.Level = skill_lvl;
                                existing.Experience = 0;
                            }
                            else
                            {
                                existing = new Proficiency();
                                existing.ID = skill_id;
                                existing.Level = skill_lvl;
                                Client.Proficiencies.Add(existing);
                            }
                            existing.Send(Client);
                            break;
                        }
                    #endregion
                    #region @gold
                    case "@gold":
                        {
                            Client.Money = Math.Max(Client.Money + int.Parse(Commands[1]), 0);
                            UpdatePacket update = UpdatePacket.Create();
                            update.ID = UpdateID.Money;
                            update.UID = Client.Entity.UID;
                            update.Value = (uint)Client.Money;
                            Client.Send(&update);
                            break;
                        }
                    #endregion
                    #region @cps
                    case "@cps":
                        {
                            Client.ConquerPoints = Math.Max(Client.ConquerPoints + int.Parse(Commands[1]), 0);
                            UpdatePacket update = UpdatePacket.Create();
                            update.ID = UpdateID.ConquerPoints;
                            update.UID = Client.Entity.UID;
                            update.Value = (uint)Client.ConquerPoints;
                            Client.Send(&update);
                            break;
                        }
                    #endregion
                    #region @clearinventory
                    case "@clearinventory":
                        {
                            ItemUsuagePacket Remove = ItemUsuagePacket.Create();
                            Remove.ID = ItemUsuageID.RemoveInventory;
                            for (byte i = 0; i < Client.Inventory.MaxPossibleItems; i++)
                            {
                                if (Client.Inventory[i] != null)
                                {
                                    Remove.UID = Client.Inventory[i].UID;
                                    Client.Send(&Remove);
                                    Client.Inventory[i] = null;
                                }
                            }
                            Client.Inventory.ItemCount = 0;
                            break;
                        }
                    #endregion
                    #region @forcerevive
                    case "@forcerevive":
                        {
                            Packet_Processor.PacketProcessor.Revive(Client);
                            break;
                        }
                    #endregion
                    #region @playercount
                    case "@playercount":
                        {
                            Client.Send(new MessagePacket("Players Online: " + Kernel.Clients.Length.ToString(), 0x00FFFFFF, ChatID.Center));
                            break;
                        }
                    #endregion
                }
            }
            catch (Exception e)
            {
                MessagePacket Msg = new MessagePacket(null, 0x00FF0000, ChatID.TopLeft);
                Msg.Message = "An error has occured: " + e.Message;
                Client.Send(Msg);
            }
        }
    }
}