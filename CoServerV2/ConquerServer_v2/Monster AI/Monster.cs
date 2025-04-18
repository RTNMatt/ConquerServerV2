using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConquerServer_v2.Core;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;
using ConquerServer_v2.Client;
using ConquerServer_v2.Attack_Processor;

namespace ConquerServer_v2.Monster_AI
{
    public unsafe class Monster
    {
        // Used for Messengers & Pets
        public ushort OverrideSpell;
        public ushort OverrideSpellLevel;

        public CommonEntity Entity;
        public MonsterSettings Settings;
        public IBaseEntity Target;
        public MonsterStatus Status;
        public MonsterSpawn Spawn;
        public MonsterFamily Family;
        public ushort SpawnX;
        public ushort SpawnY;
        protected TIME CanAttack;
        protected TIME CanRevive;
        public TIME RemoveTime;
        public Monster(MonsterSpawn spawn, MonsterFamily family, MobCollection colletion, MonsterSettings settings)
        {
            Initialize(spawn, family, colletion, settings);
        }
        protected virtual void Initialize(MonsterSpawn spawn, MonsterFamily family, MobCollection colletion, MonsterSettings settings)
        {
            Entity = new CommonEntity(this, EntityFlag.Monster);
            Settings = settings;
            Family = family;
            Spawn = spawn;
        }

        /// <summary>
        /// Used to assert a potential target for this target, this function will do nothing,
        /// unless this monster is of EntityFlag.Monster. If this monster already had a target,
        /// it will not change.
        /// </summary>
        /// <param name="entity"></param>
        public void AssignPotentialTarget(IBaseEntity entity)
        {
            lock (Family)
            {
                if (Target == null)
                {
                    if (Entity.EntityFlag == EntityFlag.Monster)
                    {
                        if ((Settings & MonsterSettings.Guard) == MonsterSettings.Guard)
                        {
                            if ((entity.StatusFlag & StatusFlag.BlueName) == StatusFlag.BlueName)
                                Target = entity;
                        }
                        else if ((Settings & MonsterSettings.Reviver) == MonsterSettings.Reviver)
                        {
                            if (entity.Dead && entity.EntityFlag == EntityFlag.Player)
                            {
                                if ((entity.Owner as GameClient).TimeStamps.GuardReviveTime.Time <= TIME.Now.Time)
                                {
                                    Target = entity;
                                }
                            }
                        }
                        else
                        {
                            Target = entity;
                        }
                    }
                }
            }
        }

        protected virtual void Roam()
        {
            IBaseEntity localTarget;
            if ((localTarget = Target) != null)
            {
                if ((Settings & MonsterSettings.Aggressive) == MonsterSettings.Aggressive)
                {
                    int distance = Kernel.GetDistance(this.Entity.X, this.Entity.Y, localTarget.X, localTarget.Y);
                    if (distance <= 16)
                    {
                        if (Family.AttackRange >= distance)
                        {
                            Status = MonsterStatus.Attacking;
                        }
                        else if (Family.ViewRange >= distance)
                        {
                            Status = MonsterStatus.Targetting;
                        }
                        else
                        {
                            if ((Settings & MonsterSettings.Moves) == MonsterSettings.Moves)
                            {
                                if (Kernel.Random.Next(1000) % 100 < 10)
                                {
                                    ConquerAngle walk = (ConquerAngle)(Kernel.Random.Next(1000) % 8);
                                    ushort x = this.Entity.X;
                                    ushort y = this.Entity.Y;
                                    Kernel.IncXY(walk, ref x, ref y);
                                    bool Continue = false;
                                    DataMap DMap = Spawn.CollectionOwner.DMap;
                                    if (DMap != null)
                                    {
                                        if (!DMap.Invalid(x, y))
                                        {
                                            if (!DMap.MonsterOnTile(x, y))
                                            {
                                                DMap.SetMonsterOnTile(this.Entity.X, this.Entity.Y, false);
                                                DMap.SetMonsterOnTile(x, y, true);
                                                Continue = true;
                                            }
                                        }
                                    }
                                    if (Continue)
                                    {
                                        this.Entity.X = x;
                                        this.Entity.Y = y;
                                        this.Entity.Facing = walk;

                                        MovementPacket Packet = MovementPacket.Create();
                                        Packet.Running = 0;
                                        Packet.Direction = (int)this.Entity.Facing;
                                        Packet.UID = this.Entity.UID;
                                        SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&Packet), null);
                                    }
                                }
                            }
                        }
                        return;
                    }
                }
                Target = null;
            }
            if ((Settings & MonsterSettings.Reviver) == MonsterSettings.Reviver)
            {
                Status = MonsterStatus.Attacking;
            }
        }
        protected int ReviveSurroundings(object arg)
        {
            IBaseEntity localTarget;
            if ((localTarget = Target) != null)
            {
                if (localTarget.Dead)
                {
                    GameClient Client = localTarget.Owner as GameClient;
                    SpellAnimationPacket animate = new SpellAnimationPacket();
                    animate.SpellID = 1100;
                    animate.Level = 0;
                    animate.X = this.Entity.X;
                    animate.Y = this.Entity.Y;
                    animate.AttackerUID = this.Entity.UID;
                    animate.Targets = new Dictionary<IBaseEntity, Damage>();
                    animate.Targets.Add(Client.Entity, new Damage(0, 0));
                    Client.RevivePlayer(true);
                    SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, animate, null);
                }
                Target = null;
            }
            return 0;
        }
        protected virtual int AttackTarget(object arg)
        {
            IBaseEntity localTarget;
            if ((localTarget = Target) != null)
            {
                if ((Settings & MonsterSettings.Guard) == MonsterSettings.Guard)
                {
                    if ((localTarget.StatusFlag & StatusFlag.BlueName) != StatusFlag.BlueName)
                    {
                        Target = null;
                        Status = MonsterStatus.Roam;
                        return -1;
                    }
                }

                TIME Now = TIME.Now;
                if (CanAttack.Time <= Now.Time)
                {
                    if (!AttackProcessor.ProcessMeele(this.Entity, localTarget, AttackID.Physical))
                    {
                        if (Kernel.GetDistance(localTarget.X, localTarget.Y, this.Entity.X, this.Entity.Y) <= 16)
                        {
                            Status = MonsterStatus.Targetting;
                        }
                        else
                        {
                            Status = MonsterStatus.Roam;
                            Target = null;
                        }
                    }
                    CanAttack = Now.AddSeconds(1);
                }
            }
            else
            {
                Status = MonsterStatus.Roam;
            }
            return 0;
        }
        protected virtual void Attacking()
        {
            if ((Settings & MonsterSettings.Aggressive) == MonsterSettings.Aggressive)
            {
                if ((Settings & MonsterSettings.HasPlayerOwner) != MonsterSettings.HasPlayerOwner &&
                    Target != null)
                {
                    AttackTarget(null);
                }
            }
            else if ((Settings & MonsterSettings.RevivesSurroundings) == MonsterSettings.RevivesSurroundings)
            {
                ReviveSurroundings(null);
            }
        }
        protected virtual void SeekAndDestroy()
        {
            IBaseEntity localTarget;
            if ((localTarget = Target) != null)
            {
                int distance = Kernel.GetDistance(localTarget.X, localTarget.Y, this.Entity.X, this.Entity.Y);
                if (Kernel.GetDistance(localTarget.X, localTarget.Y, this.Entity.X, this.Entity.Y) <= 16)
                {
                    if (localTarget.Dead)
                    {
                        Target = null;
                    }
                    else
                    {
                        if (Family.AttackRange >= distance)
                        {
                            Status = MonsterStatus.Attacking;
                            return;
                        }
                        else if (Family.ViewRange >= distance && ((Settings & MonsterSettings.Moves) == MonsterSettings.Moves))
                        {
                            ushort x = this.Entity.X;
                            ushort y = this.Entity.Y;

                            ConquerAngle walk = Kernel.GetFacing(Kernel.GetAngle(this.Entity.X, this.Entity.Y, localTarget.X, localTarget.Y));
                            Kernel.IncXY(walk, ref x, ref y);
                            bool Continue = false;
                            DataMap DMap = Spawn.CollectionOwner.DMap;
                            if (DMap != null)
                            {
                                if (!DMap.Invalid(x, y))
                                {
                                    if (!DMap.MonsterOnTile(x, y))
                                    {
                                        DMap.SetMonsterOnTile(this.Entity.X, this.Entity.Y, false);
                                        DMap.SetMonsterOnTile(x, y, true);
                                        Continue = true;
                                    }
                                }
                            }
                            if (Continue)
                            {
                                this.Entity.X = x;
                                this.Entity.Y = y;
                                this.Entity.Facing = walk;

                                MovementPacket Packet = MovementPacket.Create();
                                Packet.Running = 1;
                                Packet.Direction = (int)this.Entity.Facing;
                                Packet.UID = this.Entity.UID;
                                SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&Packet), null);
                            }
                            return;
                        }
                    }
                }
            }
            Status = MonsterStatus.Roam;
        }
        public virtual void TryRespawn(bool ignoreTime)
        {
            if (CanRevive.Time <= TIME.Now.Time || ignoreTime)
            {
                this.Spawn.MembersDead--;
                this.Entity.Dead = false;
                this.Entity.Spawn.StatusFlag &= ~StatusFlag.Ghost;
                this.Entity.Spawn.StatusFlag &= ~StatusFlag.BlackName; 

                DataPacket respawn = DataPacket.Create();
                respawn.ID = DataID.SpawnEffect;
                respawn.UID = this.Entity.UID;
                Spawn.TryObtainSpawnXY(out this.Entity.Spawn.X, out this.Entity.Spawn.Y);
                SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&respawn), ConquerCallbackKernel.CommonSendSpawn);

                Status = MonsterStatus.Roam;
            }
        }

        /// <summary>
        /// Notifies that this instance of a monster has died.
        /// </summary>
        /// <param name="Killer">The entity that killed this monster.</param>
        /// <param name="Delay">The delay time before sending the remove entity packet.</param>
        public virtual void Kill(IBaseEntity Killer, TIME Delay)
        {
            Entity.Dead = true;
            CanRevive = TIME.Now.AddMinutes(1);
            RemoveTime = Delay;
            Status = MonsterStatus.Respawning;
            if (Spawn.Monsters.Length > 1)
            {   
                Spawn.MembersDead++;
                if (Spawn.MembersDead == Spawn.Monsters.Length)
                {
                    Spawn.ReviveGeneration();
                }
            }

            this.Entity.Spawn.StatusFlag |= StatusFlag.Ghost;
            this.Entity.Spawn.StatusFlag |= StatusFlag.BlackName;
            UpdatePacket update = UpdatePacket.Create(); 
            update.ID = UpdateID.RaiseFlag;
            update.BigValue = this.Entity.StatusFlag;
            update.UID = this.Entity.UID;
            SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&update), null);

            DataPacket remove = DataPacket.Create();
            remove.UID = this.Entity.UID;
            remove.ID = DataID.RemoveEntity;
            SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&remove), ConquerCallbackKernel.CommonRemoveScreen, Delay);

            if ((Settings & MonsterSettings.DropItemsOnDeath) == MonsterSettings.DropItemsOnDeath)
            {
                if (Killer.EntityFlag == EntityFlag.Player)
                    DropRewards(Killer.UID);
                else
                    DropRewards(0);
            }
            Spawn.CollectionOwner.DMap.SetMonsterOnTile(this.Entity.X, this.Entity.Y, false);
        }

        public void DropRewards(uint KillerUID)
        {
            if ((Settings & MonsterSettings.DropItemsOnDeath) == MonsterSettings.DropItemsOnDeath)
            {
                ushort dropx, dropy;
                TIME protection = TIME.Now.AddSeconds(10);
                DataMap DMap = this.Spawn.CollectionOwner.DMap;
                DictionaryV2<uint, IDroppedItem> droppedItems = DMap.DroppedItems;
                int num = Kernel.Random.Next(0, 1000);
                if (num <= 500)
                {
                    dropx = this.Entity.X;
                    dropy = this.Entity.Y;
                    if (DMap.FindValidDropLocation(ref dropx, ref dropy, 5))
                    {
                        DMap.SetItemOnTile(dropx, dropy, true);

                        uint itemid;
                        int amount = Family.ItemGenerator.GenerateGold(out itemid);
                        DroppedItemPacket gold = DroppedItemPacket.Create(itemid, amount, KillerUID, protection);
                        gold.MapID = this.Entity.MapID;
                        gold.X = dropx;
                        gold.Y = dropy;
                        IDroppedItem drop = gold;
                        droppedItems.Add(drop.UID, drop);
                        SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&gold),
                            delegate(IBaseEntity sender, IBaseEntity caller)
                            {
                                drop.SendSpawn(caller.Owner as GameClient);
                                return 0;
                            }
                        );
                    }
                }
                else
                {
                    bool lucky;
                    byte sockets;
                    num = num % 6;
                    for (int i = 0; i < num; i++)
                    {
                        dropx = this.Entity.X;
                        dropy = this.Entity.Y;
                        if (!DMap.FindValidDropLocation(ref dropx, ref dropy, 5))
                            break;
                        byte ID_Quality;
                        bool ID_Special;
                        uint ID = Family.ItemGenerator.GenerateItemId(out ID_Quality, out ID_Special);
                        if (ID != 0)
                        {
                            DMap.SetItemOnTile(dropx, dropy, true);

                            StanderdItemStats itemstats = new StanderdItemStats(ID);
                            Item item = new Item();
                            item.ID = ID;
                            item.MaxDurability = itemstats.Durability;
                            item.Durability = (short)Math.Min(item.MaxDurability, Kernel.Random.Next(1, 3) * 100);

                            // <item_attributes>
                            if (!ID_Special)
                            {
                                lucky = (ID_Quality > 7); // q>unique
                                if (!lucky)
                                    lucky = (item.Plus = Family.ItemGenerator.GeneratePurity()) != 0;
                                if (!lucky)
                                    lucky = (item.Bless = Family.ItemGenerator.GenerateBless()) != 0;
                                if (!lucky)
                                {
                                    sockets = Family.ItemGenerator.GenerateSocketCount(ID);
                                    if (sockets >= 1)
                                        item.SocketOne = GemsConst.OpenSocket;
                                    else if (sockets == 2)
                                        item.SocketTwo = GemsConst.OpenSocket;
                                }
                            }
                            // </item_attributes>

                            DroppedItemPacket packet = DroppedItemPacket.Create(item, KillerUID, protection);
                            packet.MapID = this.Entity.MapID;
                            packet.X = dropx;
                            packet.Y = dropy;
                            IDroppedItem drop = packet;
                            droppedItems.Add(drop.UID, drop);
                            SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&packet),
                                delegate(IBaseEntity sender, IBaseEntity caller)
                                {
                                    drop.SendSpawn(caller.Owner as GameClient);
                                    return 0;
                                }
                            );
                        }
                        if (ID_Special)
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Queries this monster to preform it's next action. This should only be called by the Monster AI Kernel.
        /// </summary>
        public void QueryAction()
        {
            IBaseEntity localTarget;
            if ((localTarget = Target) != null)
            {
                if (((localTarget.ServerFlags & ServerFlags.LoggedOut) == ServerFlags.LoggedOut) || localTarget.MapID.Id != this.Entity.MapID.Id)
                {
                    if (this.Entity.EntityFlag == EntityFlag.Monster)
                    {
                        Target = null;
                    }
                }
            }
            switch (Status)
            {
                case MonsterStatus.Roam: Roam(); break;
                case MonsterStatus.Targetting: SeekAndDestroy(); break;
                case MonsterStatus.Attacking: Attacking(); break;
                case MonsterStatus.Respawning: TryRespawn(false); break;
            }
        }
    }
}