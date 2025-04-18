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
    public unsafe class MonsterPet : Monster
    {
        private static GeneralQueue Actions;
        static MonsterPet()
        {
            Actions = new GeneralQueue();
            Actions.Start(ThreadPriority.Lowest);
        }

        public AssignPetPacket Assignment;
        public GameClient Owner;

        public static void GetAssignmentData(GameClient Client, string Name, AssignPetPacket* pData)
        {
            pData->Model = ServerDatabase.Pets.ReadUInt16(Name, "Mesh", 0);
            if (pData->Model != 0)
            {
                pData->X = Client.Entity.X;
                pData->Y = Client.Entity.Y;
                pData->Unknown1 = 1;
                pData->UID = 700000 + (Client.Entity.UID - 1000000);
                Name.CopyTo(pData->Name);
            }
            else
            {
                throw new ArgumentException("Bad pet name");
            }
        }

        public MonsterPet(GameClient Client, string Name, AssignPetPacket Assign)
            : base(null, null, null, MonsterSettings.None)
        {
            if (ServerDatabase.Pets.SectionExists(Name))
            {
                Assignment = Assign;
                Owner = Client;
                Owner.Pet = this;

                Family = new MonsterFamily();
                Family.Level = ServerDatabase.Pets.ReadUInt16(Name, "Level", 0);
                Family.MaxAttack = ServerDatabase.Pets.ReadInt32(Name, "Attack", 0);
                Family.MinAttack = Family.MaxAttack;
                Family.Mesh = ServerDatabase.Pets.ReadUInt16(Name, "Mesh", 0);
                Family.MaxHealth  = ServerDatabase.Pets.ReadInt32(Name, "Hitpoints", 0);
                Family.Defense = ServerDatabase.Pets.ReadUInt16(Name, "Defence", 0);
                Family.AttackRange = ServerDatabase.Pets.ReadSByte(Name, "AttackRange", 0);
                
                Entity.Name = ServerDatabase.Pets.ReadString(Name, "Name", "ERROR");
                Entity.MapID = Client.Entity.MapID;
                Entity.UID = 700000 + (Client.Entity.UID - 1000000);
                OverrideSpell = ServerDatabase.Pets.ReadUInt16(Name, "SpellID", 0);
                OverrideSpellLevel = ServerDatabase.Pets.ReadUInt16(Name, "SpellLvl", 0);

                Entity.Level = Family.Level;
                Entity.MaxAttack = Family.MaxAttack;
                Entity.MinAttack = Family.MinAttack;
                Entity.Mesh = Family.Mesh;
                Entity.MaxHitpoints = Family.MaxHealth;
                Entity.Hitpoints = Family.MaxHealth;
                Entity.Defence = Family.Defense;
                Entity.X = Owner.Entity.X;
                Entity.Y = Owner.Entity.Y;
            }
            else
            {
                throw new ArgumentException("Name");
            }
        }

        protected override void Initialize(MonsterSpawn spawn, MonsterFamily family, MobCollection colletion, MonsterSettings settings)
        {
            Entity = new CommonEntity(this, EntityFlag.Pet);
        }


        protected override int AttackTarget(object arg)
        {
            IBaseEntity localTarget;
            if ((localTarget = Target) != null)
            {
                TIME Now = TIME.Now;
                if (CanAttack.Time <= Now.Time)
                {
                    if (!AttackProcessor.ProcessMeele(this.Entity, localTarget, AttackID.Physical))
                        Target = null;
                    CanAttack = Now.AddSeconds(1);
                }
            }
            return 0;
        }

        public void Attack()
        {
            Actions.Add(AttackTarget, null);
        }

        public void Attach()
        {
            DataPacket spawn = DataPacket.Create();
            spawn.ID = DataID.SpawnEffect;
            spawn.UID = Assignment.UID;
            SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&spawn), ConquerCallbackKernel.CommonSendSpawn);
            fixed (SpawnEntityPacket* pSpawn = &Entity.Spawn)
                Owner.Send(pSpawn);
        }

        public void Reattach()
        {
            fixed (AssignPetPacket* pAssign = &Assignment)
                Owner.Send(pAssign);
            Attach();
        }

        public override void Kill(IBaseEntity Killer, TIME Delay)
        {
            Entity.Dead = true;
            Owner.Pet = null;

            DataPacket remove = DataPacket.Create();
            remove.UID = this.Entity.UID;
            remove.ID = DataID.RemoveEntity;
            SendRangePacket.Add(this.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&remove), ConquerCallbackKernel.CommonRemoveScreen, Delay);
        }
    }
}
