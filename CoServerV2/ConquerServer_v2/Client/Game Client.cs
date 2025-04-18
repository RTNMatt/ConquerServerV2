using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using ConquerServer_v2.Core;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;
using ConquerServer_v2.Monster_AI;
using ConquerServer_v2.Attack_Processor;
using ConquerScriptLinker;

namespace ConquerServer_v2.Client
{
    public unsafe class GameClient
    {
        public int PacketCount;
        public TIME PacketStart;
        public double PacketSpeed
        {
            get
            {
                double difference = TIME.Now.Time - PacketStart.Time;
                return Math.Round(PacketCount / difference, 5);
            }
        }

        public static int ClientInstances = 0;

        public NetworkClient NetworkSocket;
        public GameCryptography Crypto;
        public DHKeyExchange.ServerKeyExchange DHKeyExchange;
        public CommonEntity Entity;
        public ServerFlags ServerFlags;
        public StatData Stats;
        public ClientInventory Inventory;
        public ClientEquipment Equipment;
        public ClientScreen Screen;
        public ClientTradeSession Trade;
        public ClientVendor Vendor;
        public Team Team;
        public Guild Guild;
        public Transform Transform;
        public FlexibleArray<ISkill> Proficiencies;
        public FlexibleArray<ISkill> Spells;
        public FlexibleArray<IAssociate> Friends;
        public FlexibleArray<IAssociate> Enemies;
        public FlexibleArray<MentorStudent> Students;
        public TimeStampCollection TimeStamps;
        public INpcPlayer NpcLink;
        public SpellCrypto SpellCrypt;
        public SafePointer AutoAttackPtr;
        public SafePointer MAttackDataPtr;
        public IBaseEntity AutoAttackEntity;
        public MonsterPet Pet;
        public PlayerMiner Mine;
        private DataMap m_CurrentDMap;
        public CompressBitmapColor TileColor;
        public DataMap CurrentDMap
        {
            get
            {
                if (m_CurrentDMap == null)
                    Kernel.DMaps.TryGetValue(Entity.MapID, out m_CurrentDMap);
                else if (m_CurrentDMap.MapID.StanderdId != Entity.MapID.StanderdId)
                    Kernel.DMaps.TryGetValue(Entity.MapID, out m_CurrentDMap);
                return m_CurrentDMap;
            }
        }
        public bool InTransformation { get { return Transform.ID != 0; } }
        public bool InTeam { get { return (Team != null); } }
        public bool InTrade
        {
            get
            {
                if (Trade != null)
                    return Trade.WindowOpen;
                return false;
            }
        }
        public bool IsVendor
        {
            get
            {
                if (Vendor != null)
                    return Vendor.IsVending;
                return false;
            }
        }
        public bool IsMining
        {
            get
            {
                if (Mine != null)
                {
                    return (this.ServerFlags & ServerFlags.Mining) == ServerFlags.Mining;
                }
                return false;
            }
        }

        public string Account;
        public string Spouse;
        public string SpouseAccount;

        public int Money;
        public int ConquerPoints;

        public uint Experience;
        public int Manapoints;
        public int MaxManapoints;
        public ushort PKPoints;
        public byte Job, OriginalJob; /* Original job refers to their job before RBing */
        public byte AdminFlag;
        public sbyte AttackRange;
        public sbyte Stamina;
        public int AutoAttackSpeed;
        public double[] Gems;
        public PKMode PKMode;
        public uint ActiveNpcID;
        public uint ActiveWarehouseID;
        public byte XPSkillCounter;
        public short HeadKillCounter;
        public uint PendingFriendUID;
        public byte BannedFlag;

        // <Note>
        // These fields are set ONLY when the Teleport() function
        // is called. Therefore, it's their last location before being teleported.
        public ushort LastMapID; // Non-Dynamic
        public ushort LastX;
        public ushort LastY;
        // </Note>

        // <Note>
        // These shouldn't be used/modified outside the database
        // and other calculations being preformed internally.
        public short ItemHP;
        public short ItemMP;
        public short StatHP;
        public byte BlessPercent;
        public int BaseMagicAttack;
        public int BaseMaxAttack;
        public int BaseMinAttack;
        public int TalismenAttack;
        public int TalismenMAttack;
        public int TalismenDefence;
        public int TalismenMDefence;
        // </Note>
        
        public GameClient(NetworkClient Client)
        {
            PacketStart = TIME.Now;
            ClientInstances++;
            NetworkSocket = Client;
            Crypto = new GameCryptography("BC234xs45nme7HU9");
            DHKeyExchange = new ConquerServer_v2.Client.DHKeyExchange.ServerKeyExchange();
            Entity = new CommonEntity(this, EntityFlag.Player);
            Inventory = new ClientInventory(this);
            Equipment = new ClientEquipment(this);
            Screen = new ClientScreen(this);
            NpcLink = new ClientNpcLink(this);
            Guild = new Guild(this);
            Transform = new Transform(this);
            SpellCrypt = new SpellCrypto();
            TimeStamps = new TimeStampCollection();
            Proficiencies = new FlexibleArray<ISkill>();
            Spells = new FlexibleArray<ISkill>();
            Friends = new FlexibleArray<IAssociate>();
            Students = new FlexibleArray<MentorStudent>();
            Gems = new double[GemsConst.MaxGems];
            MAttackDataPtr = new SafePointer(sizeof(MAttackData));
            AutoAttackPtr = new SafePointer(sizeof(RequestAttackPacket));
            AutoAttackSpeed = 1000;
        }
        ~GameClient()
        {
            ClientInstances--;
        }
        private void FailedSend()
        {
            this.NetworkSocket.Disconnect();
            Console.Write("Dropped {0} ~ {1} - Laggy Connection", this.Account, Entity.Name);
        }
        public void Send(void* Ptr)
        {
            if (NetworkSocket.Alive)
            {
                bool fSend = false;
                try
                {
                    if (fSend = Monitor.TryEnter(this, 50))
                    {
                        byte[] Chunk = Kernel.ToBytes(Ptr);

                        Crypto.Encrypt(Chunk);
                        NetworkSocket.Send(Chunk);                       
                    }
                }
                finally
                {
                    if (!fSend)
                        FailedSend();
                    else
                        Monitor.Exit(this);
                }
            }
        }
        public void Send(byte[] Packet)
        {
            if (NetworkSocket.Alive)
            {
                bool fSend = false;
                try
                {
                    if (fSend = Monitor.TryEnter(this, 50))
                    {
                        Console.WriteLine("SNEDING!!!:\r\n" + Program.Dump(Packet));
                        Crypto.Encrypt(Packet);
                        NetworkSocket.Send(Packet);
                    }
                }
                finally
                {
                    if (!fSend)
                        FailedSend();
                    else
                        Monitor.Exit(this);
                }
            }
        }

        /// <summary>
        /// Adds PK Points to this user (and updates red/blackname), This function returns
        /// whether the statusflag has changed at all during the call.
        /// </summary>
        /// <param name="value">The value to add, this can be negative.</param>
        public bool AddPKPoints(int value)
        {
            if (value > 0)
            {
                if (Entity.MapID == TournamentAI.MapID && TournamentAI.Active)
                    return false;
            }

            ulong original = Entity.Spawn.StatusFlag;
            PKPoints = (ushort)Math.Max(PKPoints + value, 0);
            if (PKPoints >= 100)
            {
                Entity.Spawn.StatusFlag &= ~StatusFlag.RedName;
                Entity.Spawn.StatusFlag |= StatusFlag.BlackName;
            }
            else if (PKPoints >= 30)
            {
                Entity.Spawn.StatusFlag |= StatusFlag.RedName;
                Entity.Spawn.StatusFlag &= ~StatusFlag.BlackName;
            }
            else
            {
                Entity.Spawn.StatusFlag &= ~StatusFlag.RedName;
                Entity.Spawn.StatusFlag &= ~StatusFlag.BlackName;
            }

            UpdatePacket update = UpdatePacket.Create();
            update.ID = UpdateID.PKPoints;
            update.UID = this.Entity.UID;
            update.Value = this.PKPoints;
            this.Send(&update);

            return (original != Entity.Spawn.StatusFlag);
        }
        /// <summary>
        /// This should be called anytime one of the following fields change:
        /// Job, Stats (Any except 'StatPoints'), ItemHP, and ItemMP
        /// </summary>
        public void CalculateBonus()
        {
            byte ManaBoost = 5;
            const byte HitpointBoost = 24;
            sbyte JobID = (sbyte)(this.Job / 10);
            if (JobID == 13 || JobID == 14)
            {
                sbyte chksum = (sbyte)(this.Job % 10);
                if (chksum >= 0 && chksum <= 5)
                {
                    ManaBoost += (byte)(5 * (this.Job - (JobID * 10)));
                }
            }
            this.StatHP = (short)((this.Stats.Strength * 3) +
                                    (this.Stats.Agility * 3) +
                                    (this.Stats.Spirit * 3) +
                                    (this.Stats.Vitality * HitpointBoost) + 1);
            this.MaxManapoints = (this.Stats.Spirit * ManaBoost) + this.ItemMP;
            this.Manapoints = Math.Min(this.Manapoints, this.MaxManapoints);

            switch (this.Job)
            {
                case 11: this.Entity.MaxHitpoints = (int)(this.StatHP * 1.05F); break;
                case 12: this.Entity.MaxHitpoints = (int)(this.StatHP * 1.08F); break;
                case 13: this.Entity.MaxHitpoints = (int)(this.StatHP * 1.10F); break;
                case 14: this.Entity.MaxHitpoints = (int)(this.StatHP * 1.12F); break;
                case 15: this.Entity.MaxHitpoints = (int)(this.StatHP * 1.15F); break;
                default: this.Entity.MaxHitpoints = (int)this.StatHP; break;
            }
            this.Entity.MaxHitpoints += this.ItemHP;
            this.Entity.Hitpoints = Math.Min(this.Entity.Hitpoints, this.Entity.MaxHitpoints);
        }
        /// <summary>
        /// This should be called anytime the following fields change:
        /// Stats.Strength, Gems (Dragon), Gems(Phoenix), BaseMagicAttack
        /// </summary>
        public void CalculateAttack()
        {
            this.Entity.MaxAttack = (int)((this.Stats.Strength + this.BaseMaxAttack) * (1 + this.Gems[GemsConst.DragonGem]));
            this.Entity.MinAttack = (int)((this.Stats.Strength + this.BaseMinAttack) * (1 + this.Gems[GemsConst.DragonGem]));
            this.Entity.MagicAttack = (int)(this.BaseMagicAttack * (1 + this.Gems[GemsConst.PhoenixGem]));
        }
        /// <summary>
        /// Displays the users stats to them, consists of:
        /// Attack, MagicAttack, Defence, MDefence, Dodge, and Potency
        /// </summary>
        public void DisplayStats()
        {
            MessagePacket Msg = new MessagePacket(
                " Attack: {0}~{1} MagicAttack: {2} Defence: {3} MDefence: {4} Dodge: {5} | Potency: {6}",
                0x00FFFFFF, ChatID.Center);
            Msg.Message = string.Format(
                Msg.Message,
                this.Entity.MinAttack, this.Entity.MaxAttack,
                this.Entity.MagicAttack,
                this.Entity.Defence,
                this.Entity.MDefence + this.Entity.PlusMDefence,
                this.Entity.Dodge, "N/A"
            );
            this.Send(Msg);
        }
        /// <summary>
        /// Teleport this player to a specified destination.
        /// </summary>
        /// <param name="MapID">The map id to teleport the player to.</param>
        /// <param name="X">The x-coordinate to teleport the player to.</param>
        /// <param name="Y">The y-coordinate to teleport the player to.</param>
        /// <param name="GiveProtection">Whether the person should receive spawn protection.</param>
        public void Teleport(MapID MapID, ushort X, ushort Y, bool GiveProtection)
        {
            if (this.IsVendor)
                this.Vendor.StopVending();
            if (this.IsMining)
                this.Mine.Stop();

            DataMap newDMap = null;
            if (MapID.Id != Entity.MapID.Id)
            {
                Kernel.DMaps.TryGetValue(MapID, out newDMap);
                if (this.Pet != null)
                    this.Pet.Kill(null, TIME.Now);
            }
            else
            {
                newDMap = m_CurrentDMap;
            }
            if (newDMap != null)
            {
                if (newDMap.Invalid(X, Y))
                    return;
                m_CurrentDMap = newDMap;

                if (GiveProtection)
                    this.TimeStamps.SpawnProtection = TIME.Now.AddSeconds(5);

                DataPacket packet = DataPacket.Create();
                packet.UID = this.Entity.UID;


                bool SendRemove = true;
                if (MapID == this.Entity.MapID)
                    SendRemove = Kernel.GetDistance(X, Y, this.Entity.X, this.Entity.Y) >= 16;
                if (SendRemove)
                {
                    packet.ID = DataID.RemoveEntity;
                    SendRangePacket.Add(this.Entity, Kernel.ViewDistance, this.Entity.UID,
                        Kernel.ToBytes(&packet), ConquerCallbackKernel.CommonRemoveScreen);
                }

                this.LastMapID = (ushort)Entity.MapID.StanderdId;
                this.LastX = Entity.X;
                this.LastY = Entity.Y;

                this.Entity.MapID = MapID;
                this.Entity.X = X;
                this.Entity.Y = Y;

                packet.ID = DataID.Teleport;
                packet.dwParam1 = this.Entity.MapID;
                packet.wParam1 = this.Entity.X;
                packet.wParam2 = this.Entity.Y;
                this.Send(&packet);

                this.Respawn();
            }
        }
        /// <summary>
        /// Teleport this player to a specified destination with no spawn protection.
        /// </summary>
        /// <param name="MapID">The map id to teleport the player to.</param>
        /// <param name="X">The x-coordinate to teleport the player to.</param>
        /// <param name="Y">The y-coordinate to teleport the player to.</param>
        public void Teleport(MapID MapID, ushort X, ushort Y)
        {
            Teleport(MapID, X, Y, false);
        }
        /// <summary>
        /// Respawns this client to the surrounding player entities.
        /// </summary>
        public void Respawn()
        {
            fixed (SpawnEntityPacket* pSpawn = &this.Entity.Spawn)
                SendRangePacket.Add(this.Entity, Kernel.ViewDistance, this.Entity.UID, Kernel.ToBytes(pSpawn), null);
        }
        /// <summary>
        /// Teleports a client to it's current *server* x and y coordinates.
        /// This should be used if an invalid jump or walk is preformed for instance.
        /// </summary>
        public void Pullback()
        {
            Teleport(Entity.MapID, Entity.X, Entity.Y);
            if (this.Pet != null)
                this.Pet.Reattach();
        }
        /// <summary>
        /// Equips an item for the user. More Info:
        /// Removes from the inventory.
        /// Replaces an item, if an item is already in the slot.
        /// Adds the stats that belongs to the item.
        /// Updates hp and mana.
        /// This does not preform aditional checks, such as, whether it is valid to equip this item 
        /// to this slot, the user has sufficient stats, etc.
        /// </summary>
        /// <param name="Item">The item to be equipped.</param>
        /// <param name="Position">The position to equip it to.</param>
        /// <param name="Packet">The packet received when equipping the item, if null this field will be auto-filled.</param>
        /// <returns></returns>
        public bool Equip(Item Item, ItemPosition Position, ItemUsuagePacket* Packet)
        {
            Item existing_equipment = this.Equipment[Position];
            Item existing_arrow = null;
            bool removed_inventory = false;
            if (existing_equipment != null)
            {
                if (existing_equipment.IsItemType(ItemTypeConst.BowID) &&
                    existing_equipment.Position == ItemPosition.Right)
                {
                    if (this.Inventory.ItemCount > 39)
                    {
                        this.Send(MessageConst.INVENTORY_FULL);
                        return false;
                    }
                    existing_arrow = this.Equipment[ItemPosition.Left];
                    if (!Unequip(ItemPosition.Left, false)) // Remove Arrow
                        return false;
                }
                byte inventorySlot;
                this.Inventory.Search(Item.UID, out inventorySlot);
                if (this.Inventory.RemoveBySlot(inventorySlot, false) != InventoryErrNo.SUCCESS)
                    return false;
                else
                    removed_inventory = true;
                if (!Unequip(Position, false))
                    return false;
            }
#pragma warning disable
            Equipment.Equip(Item, Position);
#pragma warning restore

            if (Packet == null)
            {
                ItemUsuagePacket temp = ItemUsuagePacket.Create();
                Packet = &temp;
                Packet->UID = Item.UID;
                Packet->dwParam1 = (ushort)Position;
            }
            Packet->ID = ItemUsuageID.SetEquipPosition;
            Packet->dwParam2 = 0;
            this.Send(Packet);
            
            if (!removed_inventory)
            {
                if (this.Inventory.Remove(Item.UID) != InventoryErrNo.SUCCESS)
                    return false;
            }

            /* Restore the existing equipement to the inventory */
            if (existing_equipment != null)
                this.Inventory.Add(existing_equipment);
            if (existing_arrow != null)
                this.Inventory.Add(existing_arrow);

            this.CalculateBonus();
            this.CalculateAttack();

            BigUpdatePacket update = new BigUpdatePacket(2);
            update.HitpointsAndMana(this, 0);
            this.Send(update);

            //Show Item
            HeroItemsPacket HeroItems = new HeroItemsPacket().Create(this);
            this.Send(&HeroItems);

            this.Respawn();

            return true;
        }
        /// <summary>
        /// Unequips an item from the user. More Info: 
        /// Adds it the inventory (function fails if inventory is full).
        /// Removes it from the equipment window.
        /// Unloads the stats that belong to the item.
        /// Updates hp and mana.
        /// </summary>
        /// <param name="Position">The position to unequip from.</param>
        /// <param name="AddInventory">Whether to add the item being unequipped to the inventory</param>
        /// <returns></returns>
        public bool Unequip(ItemPosition Position, bool AddInventory)
        {
            if (Inventory.ItemCount < 40 || !AddInventory)
            {
#pragma warning disable
                Item item = Equipment.Unequip(Position);
#pragma warning restore
                if (item != null)
                {
                    bool Success = !AddInventory;
                    if (!Success)
                        Success = (this.Inventory.Add(item) == InventoryErrNo.SUCCESS);
                    if (Success)
                    {
                        ItemUsuagePacket packet = ItemUsuagePacket.Create();
                        packet.ID = ItemUsuageID.Unequip;
                        packet.UID = item.UID;
                        packet.dwParam1 = (uint)Position;
                        this.Send(&packet);

                        if (!AddInventory)
                        {
                            packet.ID = ItemUsuageID.RemoveInventory;
                            packet.dwParam1 = 0;
                            this.Send(&packet);
                        }

                        this.CalculateBonus();
                        this.CalculateAttack();

                        BigUpdatePacket update = new BigUpdatePacket(2);
                        update.HitpointsAndMana(this, 0);
                        this.Send(update);

                        //Show Item
                        HeroItemsPacket HeroItems = new HeroItemsPacket().Create(this);
                        this.Send(&HeroItems);

                        this.Respawn();
                        return true;
                    }
                }
            }
            else
            {
                this.Send(MessageConst.INVENTORY_FULL);
            }
            return false;
        }
        /// <summary>
        /// Drops equipment specified by the slot. If no equipment is found, nothing occurs.
        /// </summary>
        /// <param name="Position">The slot to drop the equipment from.</param>
        public void DropEquipment(ItemPosition Position)
        {
            DictionaryV2<uint, IDroppedItem> DroppedItems = this.CurrentDMap.DroppedItems;
            Item item = this.Equipment[Position];
            if (item != null)
            {
                this.Unequip(Position, false);
                ushort drop_x = this.Entity.X, drop_y = this.Entity.Y;
                if (CurrentDMap.FindValidDropLocation(ref drop_x, ref drop_y, 5))
                {
                    if (Kernel.GetDistance(this.Entity.X, this.Entity.Y, drop_x, drop_y) <= 5)
                    {
                        DroppedItemPacket DropItem = DroppedItemPacket.Create(item);
                        DropItem.MapID = this.Entity.MapID;
                        DropItem.X = drop_x;
                        DropItem.Y = drop_y;

                        CurrentDMap.SetItemOnTile(drop_x, drop_y, true);
                        DroppedItems.Override(DropItem.UID, DropItem);
                        SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&DropItem), null);
                    }
                }
            }
        }
        /// <summary>
        /// Drops an item from the inventory.
        /// </summary>
        /// <param name="Item">The item to be dropped</param>
        /// <param name="Slot">The slot in the inventory of where it is located.</param>
        public void DropInventoryItem(Item Item, byte Slot)
        {
            DictionaryV2<uint, IDroppedItem> DroppedItems = CurrentDMap.DroppedItems;
            ushort drop_x = this.Entity.X, drop_y = this.Entity.Y;
            if (CurrentDMap.FindValidDropLocation(ref drop_x, ref drop_y, 5))
            {
                if (Kernel.GetDistance(this.Entity.X, this.Entity.Y, drop_x, drop_y) <= 5)
                {
                    this.Inventory.Remove(Item.UID);
                    DroppedItemPacket DropItem = DroppedItemPacket.Create(Item);
                    DropItem.MapID = this.Entity.MapID;
                    DropItem.X = drop_x;
                    DropItem.Y = drop_y;

                    CurrentDMap.SetItemOnTile(drop_x, drop_y, true);
                    DroppedItems.Add(DropItem.UID, DropItem);
                    SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&DropItem), null);
                }
            }
        }
        /// <summary>
        /// Drops an item from the inventory.
        /// </summary>
        /// <param name="UID">The UID of the item to drop.</param>
        public void DropInventoryItem(uint UID)
        {
            byte Slot;
            Item Item = Inventory.Search(UID, out Slot);
            if (Item != null)
                DropInventoryItem(Item, Slot);
        }
        /// <summary>
        /// Drops a set amount of gold onto the floor where the user is standing.
        /// </summary>
        /// <param name="Amount">The amount to gold to be dropped.</param>
        public void DropGold(int Amount)
        {
            Amount = Math.Min(this.Money, Amount);

            ushort dropx, dropy;
            DictionaryV2<uint, IDroppedItem> droppedItems = CurrentDMap.DroppedItems;
            dropx = this.Entity.X;
            dropy = this.Entity.Y;
            if (CurrentDMap.FindValidDropLocation(ref dropx, ref dropy, 5))
            {
                this.Money -= Amount;
                UpdatePacket Update = UpdatePacket.Create();
                Update.ID = UpdateID.Money;
                Update.Value = (uint)this.Money;
                Update.UID = this.Entity.UID;
                this.Send(&Update);

                uint itemid = Kernel.MoneyToItemID(Amount);
                DroppedItemPacket gold = DroppedItemPacket.Create(itemid, Amount);
                gold.MapID = this.Entity.MapID;
                gold.X = dropx;
                gold.Y = dropy;
                IDroppedItem drop = gold;

                CurrentDMap.SetItemOnTile(dropx, dropy, true);
                droppedItems.Add(drop.UID, drop);
                SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&gold), null);
            }
        }
        /// <summary>
        /// Notifies the this instance that the entity has died.
        /// </summary>
        /// <param name="Delay">The time to wait before sending out changing the model to a ghost.</param>
        public void KillPlayer(TIME Delay)
        {
            if ((this.Entity.StatusFlag & StatusFlag.Dead) == StatusFlag.Dead)
                return;

            MapSettings map = new MapSettings(this.Entity.MapID);
            if (TournamentAI.Active)
            {
                if (Entity.MapID.Id == TournamentAI.MapID.Id)
                    Teleport(1002, 400, 400);
            }
            else
            {
                if (map.Status.CanGainPKPoints)
                {
                    sbyte DropItems = 0;
                    if ((Entity.StatusFlag & StatusFlag.RedName) == StatusFlag.RedName)
                        DropItems = 2;
                    else if ((Entity.StatusFlag & StatusFlag.BlackName) == StatusFlag.BlackName)
                        DropItems = 5;

                    if (DropItems > 0)
                    {
                        DropItems = (sbyte)Kernel.Random.Next(0, DropItems);
                        for (sbyte i = 0; i < DropItems; i++)
                        {
                            ItemPosition rand_slot = (ItemPosition)Kernel.Random.Next((int)Item.FirstSlot, (int)Item.LastSlot);
                            DropEquipment(rand_slot);
                        }
                    }

                    if ((this.Entity.StatusFlag & StatusFlag.BlackName) == StatusFlag.BlackName)
                    {
                        Teleport(6000, 29, 72);
                    }
                }
            }

            TIME Now = TIME.Now;
            this.TimeStamps.CanRevive = Now.AddSeconds(17);
            this.TimeStamps.GuardReviveTime = Now.AddSeconds(7);
            this.Entity.Spawn.StatusFlag &= ~StatusFlag.Fly;
            this.Entity.Spawn.StatusFlag |= StatusFlag.Dead;
            this.Entity.Spawn.StatusFlag |= StatusFlag.Ghost;
            this.Entity.Spawn.StatusFlag &= ~StatusFlag.XPSkills;
            this.Entity.Spawn.StatusFlag &= ~StatusFlag.Superman;
            this.Entity.Spawn.StatusFlag &= ~StatusFlag.Cyclone;
            this.Entity.Spawn.StatusFlag &= ~StatusFlag.PurpleShield;

            this.Entity.Dead = true;

            BigUpdatePacket big = new BigUpdatePacket(3);
            big.UID = this.Entity.UID;
            big.Append(0, UpdateID.Model, this.Entity.Model);
            big.Append(1, UpdateID.Hitpoints, this.Entity.Hitpoints);
            big.Append(2, UpdateID.RaiseFlag, this.Entity.StatusFlag);

            UpdatePacket update = UpdatePacket.Create();
            update.ID = UpdateID.RaiseFlag;
            update.BigValue = this.Entity.Spawn.StatusFlag;
            update.UID = this.Entity.UID;

            SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&update), null);
            SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, big, ConquerCallbackKernel.EnsureUserIsDead, Delay);
            
            if (this.Entity.MapID == MapID.GuildWar)
                this.TimeStamps.GuildWarTime = TIME.Now.AddMinutes(10);
        }
        /// <summary>
        /// Revives this player, only if they are dead.
        /// </summary>
        /// <param name="ReviveOnSpot">Whether to revive this player where they currently are.</param>
        public void RevivePlayer(bool ReviveOnSpot)
        {
            if (this.Entity.Dead)
            {
                TIME Now = TIME.Now;
                this.TimeStamps.SpawnProtection = Now.AddSeconds(5);
                this.Entity.Spawn.StatusFlag &= ~StatusFlag.Dead;
                this.Entity.Spawn.StatusFlag &= ~StatusFlag.Ghost;
                this.Entity.Spawn.StatusFlag &= ~StatusFlag.BlueName;
                this.Entity.Dead = false;
                if (this.Entity.Hitpoints == 0)
                    this.Entity.Hitpoints = 1;

                bool SendRespawn = false;
                uint res_mapid;
                ushort res_x, res_y;
                if (!ReviveOnSpot)
                {
                    uint[] res = new MapSettings(this.Entity.MapID).RevivePoint;
                    res_mapid = res[0];
                    res_x = (ushort)res[1];
                    res_y = (ushort)res[2];
                    SendRespawn = true;
                }
                else
                {
                    res_mapid = this.Entity.MapID.Id;
                    res_x = this.Entity.X;
                    res_y = this.Entity.Y;
                }

                bool SendRemove = true;
                if (res_mapid == this.Entity.MapID.Id)
                    SendRemove = (Kernel.GetDistance(res_x, res_y, this.Entity.X, this.Entity.Y) >= 8);
                if (SendRemove)
                {
                    DataPacket Data = DataPacket.Create();
                    Data.UID = this.Entity.UID;
                    Data.ID = DataID.RemoveEntity;
                    SendRangePacket.Add(this.Entity, Kernel.ViewDistance, this.Entity.UID, Kernel.ToBytes(&Data), ConquerCallbackKernel.CommonRemoveScreen);
                }

                if (SendRespawn)
                    Teleport(res_mapid, res_x, res_y);
                else
                {
                    this.Entity.X = res_x;
                    this.Entity.Y = res_y;
                    this.Entity.MapID = res_mapid;
                    this.Respawn();
                }

                BigUpdatePacket big = new BigUpdatePacket(3);
                big.UID = this.Entity.UID;
                big.Append(0, UpdateID.Hitpoints, this.Entity.Hitpoints);
                big.Append(1, UpdateID.Model, this.Entity.Model);
                big.Append(2, UpdateID.RaiseFlag, this.Entity.StatusFlag);
                SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, big, null);
            }
        }
        /// <summary>
        /// Awards experience to this player, the amount is multiplied by Kernel.ExperienceRate and truncated.
        /// </summary>
        /// <param name="Amount">The amount of experience to award.</param>
        /// <param name="RainbowMultiplier">Whether to include the rainbow-gem multipler in this equation.</param>
        public void AwardExperience(int Amount, bool RainbowMultiplier)
        {
            lock (this.Entity)
            {
                Amount = (int)(Amount * Kernel.ExperienceRate);
                if (this.Entity.Level < 130 && Amount > 0)
                {
                    bool LeveledUp = false;
                    Experience += (uint)Amount;
                    uint RequiredExp;
                    while (true)
                    {
                        RequiredExp = ServerDatabase.LevelExp.ReadUInt32("Level", this.Entity.Level.ToString(), 0);
                        if (RequiredExp > 0)
                        {
                            if (Experience >= RequiredExp)
                            {
                                Experience -= RequiredExp;
                                LeveledUp = true;
                                this.Entity.Spawn.Level++;
                                continue;
                            }
                        }
                        break;
                    }
                    if (LeveledUp)
                    {
                        if (this.Entity.Reborn == 0)
                        {
                            ServerDatabase.GetStats(this);
                        }
                        else
                        {
                            this.Stats.StatPoints += 3;
                        }

                        DataPacket levelup = DataPacket.Create();
                        levelup.UID = this.Entity.UID;
                        levelup.ID = DataID.LevelUp;
                        this.Send(&levelup);

                        BigUpdatePacket update = new BigUpdatePacket(6);
                        update.UID = this.Entity.UID;
                        update.Append(0, UpdateID.Level, this.Entity.Level);
                        update.AllStats(this, 1);
                        this.Send(update);
                    }
                    UpdatePacket exp = UpdatePacket.Create();
                    exp.UID = this.Entity.UID;
                    exp.ID = UpdateID.Experience;
                    exp.Value = this.Experience;
                    this.Send(&exp);
                }
            }
        }
        /// <summary>
        /// Awards proficiency experience to this player.
        /// </summary>
        /// <param name="damageAmount">The amount of experience to award.</param>
        public void AwardProfExperience(int damageAmount)
        {
            damageAmount = (int)(damageAmount * (1 + Gems[GemsConst.VioletGem]));
            Item left = Equipment[ItemPosition.Left];
            Item right = Equipment[ItemPosition.Right];
            if (right != null)
                AwardProfExperience(right.GetItemType(), damageAmount);
            if (left != null)
                AwardProfExperience(left.GetItemType(), damageAmount / 2);

        }
        private void AwardProfExperience(ushort wType, int Amount)
        {
            ISkill skill;
            if (Proficiencies.GetSkill(wType, out skill))
            {
                lock (skill)
                {
                    if (!skill.MaxLevel)
                    {
                        skill.Experience += Amount;
                        while (skill.Experience >= skill.NeededExperience)
                        {
                            skill.Experience -= skill.NeededExperience;
                            skill.Level++;
                            if (skill.MaxLevel)
                            {
                                skill.Experience = 0;
                                break;
                            }
                        }
                        skill.Send(this);
                    }
                }
            }
        }
        /// <summary>
        /// Awards spell experience to this player.
        /// </summary>
        /// <param name="damageAmount">The amount of experience to award.</param>
        public void AwardSpellExperience(ushort SpellID, int damageAmount)
        {
            float fMultiplier = 1.00f;
            if (InTransformation)
            {
                if (Transform.SpellID == 1360) // NightDevil
                    fMultiplier = 1 / 3;
                else if (Transform.SpellID == 1280) // Water Elf
                    fMultiplier = 1 / 2;
            }
            damageAmount = (int)(damageAmount * fMultiplier);
            ISkill skill;
            if (Spells.GetSkill(SpellID, out skill))
            {
                Spell spell = skill as Spell;
                if (!skill.MaxLevel && this.Entity.Level >= spell.NeededLevel)
                {
                    skill.Experience += damageAmount;
                    while (skill.Experience >= skill.NeededExperience)
                    {
                        skill.Experience -= skill.NeededExperience;
                        skill.Level++;
                        if (skill.MaxLevel)
                        {
                            skill.Experience = 0;
                            break;
                        }
                    }
                    skill.Send(this);
                }
            }
        }
        /// <summary>
        /// Receives team experience.
        /// </summary>
        /// <param name="TExp">The team experience data to receive from.</param>
        private void ReceiveTeamExperience(TeamExperience TExp) 
        {
            /* SPECIAL THANKS SPARKIE/UNKNOWNONE */
            int exp;
            int leveldiff = this.Entity.Level - TExp.MobLevel;
            if (leveldiff <= -20)
                exp = this.Entity.Level * 30;
            else
            {
                exp = TExp.MobMaxHP / 20;
                //bonus for being lower level
                if (leveldiff <= -10)
                    exp = (int)(exp * 1.3);
                else if (leveldiff <= -5)
                    exp = (int)(exp * 1.2);
                //bonus for having a noob in team
                if (TExp.NewbieInTeam)
                    exp *= 2;
                //bonus for spouse killing monster
                if (TExp.MobSlayerAccount == this.SpouseAccount)
                    exp *= 2;
            }
            //bonus for water wizards  
            if (this.Job >= 133 && this.Job <= 135)
                exp *= 2;
            AwardExperience(exp, false);
        }
        /// <summary>
        /// Distributes team experience if this player is in a team, and the team leader.
        /// </summary>
        /// <param name="Mob">The monster that has been killed.</param>
        public void DistributeTeamExperience(Monster Mob)
        {
            if (this.InTeam)
            {
                if (this.Team.Leader)
                {
                    TeamExperience exp = new TeamExperience(this, Mob);
                    for (int i = 0; i < this.Team.Teammates.Length; i++)
                    {
                        GameClient Teammate = this.Team.Teammates[i];
                        if (Teammate.Entity.MapID.Id == this.Entity.MapID.Id)
                        {
                            if (Kernel.GetDistance(this.Entity.X, this.Entity.Y, Teammate.Entity.X, Teammate.Entity.Y) <= Kernel.ViewDistance * 2)
                            {
                                Teammate.ReceiveTeamExperience(exp);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Makes this user flash blue name for 20 seconds.
        /// If they are already flashing their counter will be reset to end 20 seconds from now.
        /// </summary>
        public void FlashBlue()
        {
            this.TimeStamps.BlueName = TIME.Now.AddSeconds(35);
            this.Entity.Spawn.StatusFlag |= StatusFlag.BlueName;

            foreach (IMapObject obj in Screen.Objects)
            {
                if (obj.MapObjType == MapObjectType.Monster)
                {
                    Monster mob = obj.Owner as Monster;
                    if ((mob.Settings & MonsterSettings.Guard) == MonsterSettings.Guard)
                        mob.AssignPotentialTarget(this.Entity);
                }
            }

            UpdatePacket update = UpdatePacket.Create();
            update.ID = UpdateID.RaiseFlag;
            update.BigValue = this.Entity.StatusFlag;
            update.UID = this.Entity.UID;
            SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&update), null);
        }
        /// <summary>
        /// Unlearn a spell.
        /// </summary>
        /// <param name="ID">The spell ID</param>
        public void UnlearnSpell(ushort ID)
        {   
            ISkill skill;
            int index = Spells.GetSkillIdx(ID, out skill);
            if (index > -1)
            {
                Spells.Remove(index);
                DataPacket unlearn = DataPacket.Create();
                unlearn.ID = DataID.UnlearnSpell;
                unlearn.dwParam1 = skill.ID;
                unlearn.UID = this.Entity.UID;
                this.Send(&unlearn);
            }
        }
    }
}
