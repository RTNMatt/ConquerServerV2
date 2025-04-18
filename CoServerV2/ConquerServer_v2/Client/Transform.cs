using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Client
{
    /// <summary>
    /// Encalpsulates the old stats before the user transformed to beable to restore them.
    /// Make sure all operators that modify stats modify these, and not the actual
    /// stat, if a user is transformed.
    /// </summary>
    public class Transform
    {
        public int Defence;
        public int Dodge;
        public int MDefence;
        public ushort ID;
        public ushort SpellID;
        private GameClient Client;

        public Transform(GameClient owner)
        {
            Client = owner;
        }
        /// <summary>
        /// Starts a new transformation, preserves existing stats.
        /// </summary>
        /// <param name="TransformID">The ID (mesh) your transforming into.</param>
        public void Start(ushort TransformID, ushort SpellID)
        {
            Defence = Client.Entity.Defence;
            MDefence = Client.Entity.MDefence;
            Dodge = Client.Entity.Dodge;
            ID = TransformID;
            this.SpellID = SpellID;
        }
        /// <summary>
        /// Stops the transformation, and reinstates the old stats.
        /// </summary>
        public void Stop()
        {
            double HPModifier = (double)Client.Entity.Hitpoints / Client.Entity.MaxHitpoints;
            Client.Entity.Defence = Defence;
            Client.Entity.MDefence = MDefence;
            Client.Entity.Dodge = Dodge;
            Client.CalculateAttack(); // Restore Attack
            Client.CalculateBonus();  // Restore HP
            Client.Entity.Hitpoints = Math.Max(1, (int)(Client.Entity.MaxHitpoints * HPModifier));
            Client.Entity.OverlappingMesh = 0;
            ID = 0;
        }
        /// <summary>
        /// Send all the updates nessecary to transforming to everyone around you.
        /// </summary>
        public void SendUpdates()
        {
            BigUpdatePacket big = new BigUpdatePacket(3);
            big.UID = Client.Entity.UID;
            big.Append(0, UpdateID.Model, Client.Entity.Model);
            big.Append(1, UpdateID.MaxHitpoints, Client.Entity.MaxHitpoints);
            big.Append(2, UpdateID.Hitpoints, Client.Entity.Hitpoints);
            SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, big, null);
        }
    }
}
