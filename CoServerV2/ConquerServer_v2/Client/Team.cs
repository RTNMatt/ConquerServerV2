using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;
using ConquerServer_v2.Monster_AI;

namespace ConquerServer_v2.Client
{
    public struct TeamExperience
    {
        public string MobSlayerAccount;
        public ushort MobLevel;
        public ushort MobMaxHP;
        public bool NewbieInTeam;

        public TeamExperience(GameClient Slayer,  Monster mob)
        {
            MobSlayerAccount = Slayer.Account;
            NewbieInTeam = Slayer.Team.NewbieInTeam(mob.Family.Level);
            MobLevel = mob.Family.Level;
            MobMaxHP = (ushort)mob.Family.MaxHealth;
        }
    }

    public unsafe class Team
    {
        private GameClient Owner;
        private FlexibleArray<GameClient> m_Teammates;
        public TeammatePacket Info;
        public GameClient[] Teammates;
        public bool Leader { get { return (Owner.Entity.StatusFlag & StatusFlag.TeamLeader) == StatusFlag.TeamLeader; } }
        public bool Full { get { return (m_Teammates.Length >= 5); } }

        /// <summary>
        /// Synchoronizes the Teammate[] array with the m_Teammmates collection.
        /// </summary>
        private void Synchoronize()
        {
            Teammates = m_Teammates.ToTrimmedArray();
        }
        /// <summary>
        /// Removes a player from m_Teammates, this does not call Synchoronize()
        /// </summary>
        /// <param name="UID">The UID of the person to remove, this CANNOT be the owner of this instance's uid.</param>
        private void Remove(uint UID)
        {
            for (int i = 1; i < Teammates.Length; i++)
            {
                if (Teammates[i].Entity.UID == UID)
                {
                    m_Teammates.Remove(i);
                    break;
                }
            }
        }
            
        /// <summary>
        /// Creates a new team instance.
        /// </summary>
        /// <param name="owner">The owner of this new team class.</param>
        /// <param name="leader">Is the owner the leader of the team?</param>
        /// <param name="creationPacket">If they're the owner, this field should not be null.</param>
        public Team(GameClient owner, bool leader, TeamActionPacket* creationPacket)
        {
            m_Teammates = new FlexibleArray<GameClient>();
            m_Teammates.SetCapacity(5);
            m_Teammates.Add(owner);
            Synchoronize();

            Owner = owner;
            if (leader)
                Owner.Entity.Spawn.StatusFlag |= StatusFlag.TeamLeader;
            Info = TeammatePacket.Create();
            Info.UID = Owner.Entity.UID;
            Info.MaxHP = (ushort)Owner.Entity.MaxHitpoints;
            Info.HP = (ushort)Owner.Entity.Hitpoints;
            Info.Model = Owner.Entity.Model;
            fixed (sbyte* ptrName = Info.szName)
                Owner.Entity.Name.CopyTo(ptrName);

            if (creationPacket != null)
            {
                Owner.Send(creationPacket);
                UpdatePacket update = UpdatePacket.Create();
                update.UID = Owner.Entity.UID;
                update.ID = UpdateID.RaiseFlag;
                update.BigValue = Owner.Entity.StatusFlag;
                SendRangePacket.Add(Owner.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&update), null);
            }
        }

        /// <summary>
        /// Resynchoronizes this players maxhp, hp, model and name with the rest of the team.
        /// This will not send any packets if not required.
        /// </summary>
        public void Resynchoronize()
        {
            UpdatePacket update = UpdatePacket.Create();
            update.UID = Owner.Entity.UID;
            if (Owner.Entity.MaxHitpoints != Info.MaxHP)
            {
                Info.MaxHP = (ushort)Owner.Entity.MaxHitpoints;
                update.ID = UpdateID.MaxHitpoints;
                update.Value = Info.MaxHP;
                SendTeamPacket(&update, false);
            }
            if (Owner.Entity.Hitpoints != Info.HP)
            {
                Info.HP = (ushort)Owner.Entity.Hitpoints;
                update.ID = UpdateID.Hitpoints;
                update.Value = Info.HP;
                SendTeamPacket(&update, false);
            }
            if (Owner.Entity.Model != Info.Model)
            {
                Info.Model = Owner.Entity.Model;
                update.ID = UpdateID.Model;
                update.Value = Info.Model;
                SendTeamPacket(&update, false);
            }
        }
        /// <summary>
        /// Searchs for a teammate by the uid, returns null if they're not found.
        /// </summary>
        /// <param name="UID">The uid of the person to search for.</param>
        public GameClient Search(uint UID)
        {
            foreach (GameClient temmate in Teammates)
                if (temmate.Entity.UID == UID)
                    return temmate;
            return null;
        }
        /// <summary>
        /// Sends a packet to the entire team.
        /// </summary>
        /// <param name="self">Should the packet be send to the owner of this class?</param>
        public void SendTeamPacket(byte[] packet, bool self)
        {
            foreach (GameClient client in Teammates)
            {
                if (client.Entity.UID != Owner.Entity.UID || self)
                    client.Send(packet);
            }
        }
        /// <summary>
        /// Sends a packet to the entire team.
        /// </summary>
        /// <param name="self">Should the packet be send to the owner of this class?</param>
        public void SendTeamPacket(void* packet, bool self)
        {
            foreach (GameClient client in Teammates)
            {
                if (client.Entity.UID != Owner.Entity.UID || self)
                {
                    client.Send(packet);
                }
            }
        }
        /// <summary>
        /// This will join the owner of this class, to an existing team.
        /// This should be used if someone has accepted an invitation to a team,
        /// or the leader has allowed someone to join the team.
        /// </summary>
        /// <param name="leadersTeam">This should be the instance to the leader's team class.</param>
        public void JoinTeam(Team leadersTeam)
        {
            // Start the loop at the 
            for (int i = 1; i < leadersTeam.Teammates.Length; i++)
            {
                GameClient teammate = leadersTeam.Teammates[i];
                m_Teammates.Add(teammate);
                teammate.Team.m_Teammates.Add(Owner);
                teammate.Team.Synchoronize();
                fixed (TeammatePacket* aboutteammate = &teammate.Team.Info)
                    Owner.Send(aboutteammate);
            }
            leadersTeam.m_Teammates.Add(Owner);
            leadersTeam.Synchoronize();
            m_Teammates.Add(leadersTeam.Owner);
            fixed (TeammatePacket* aboutleader = &leadersTeam.Info)
                Owner.Send(aboutleader);
            Synchoronize();

            fixed (TeammatePacket* aboutme = &Info)
                SendTeamPacket(aboutme, false);
        }
        /// <summary>
        /// The owner of the class will leave the current team.
        /// </summary>
        /// <param name="leavePacket">A pointer to the leaving packet, if null, this function will fill out the information for you.</param>
        public void LeaveTeam(TeamActionPacket* leavePacket)
        {
            if (leavePacket == null)
            {
                TeamActionPacket temp = TeamActionPacket.Create();
                temp.UID = Owner.Entity.UID;
                temp.ID = TeamActionID.LeaveTeam;
                leavePacket = &temp;
            }
            for (int i = 1; i < Teammates.Length; i++)
            {
                GameClient teammate = Teammates[i];
                teammate.Team.Remove(Owner.Entity.UID);
                teammate.Team.Synchoronize();
                teammate.Send(leavePacket);
            }
            Owner.Send(leavePacket);
            Owner.Team = null;
        }
        /// <summary>
        /// Dismisses this team, warning: this does not check is the owner is the leader of the team.
        /// </summary>
        /// <param name="dismissPacket">A pointer to the dismiss packet, if null, this function will fill out the information for you.</param>
        public void DismissTeam(TeamActionPacket* dismissPacket)
        {
            if (dismissPacket == null)
            {
                TeamActionPacket temp = TeamActionPacket.Create();
                temp.UID = Owner.Entity.UID;
                temp.ID = TeamActionID.Dismiss;
                dismissPacket = &temp;
            }
            for (int i = 1; i < Teammates.Length; i++)
            {
                GameClient teammate = Teammates[i];
                teammate.Team = null;
                teammate.Send(dismissPacket);
            }
            Owner.Team = null;
            Owner.Send(dismissPacket);
            Owner.Entity.Spawn.StatusFlag &= ~StatusFlag.TeamLeader;

            UpdatePacket update = UpdatePacket.Create();
            update.UID = Owner.Entity.UID;
            update.ID = UpdateID.RaiseFlag;
            update.BigValue = Owner.Entity.StatusFlag;
            SendRangePacket.Add(Owner.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&update), null);
        }
        /// <summary>
        /// Determines whether there is a newbie in the team if a teammates level is lower than
        /// 20 of the monsters.
        /// </summary>
        /// <param name="MonsterLevel">The monster's level</param>
        public bool NewbieInTeam(ushort MonsterLevel)
        {
            for (int i = 1; i < Teammates.Length; i++)
            {
                if (Teammates[i].Entity.Level - MonsterLevel < 20)
                    return true;
            }
            return false;
        }
    }
}
