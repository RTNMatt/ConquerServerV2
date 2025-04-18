using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Database
{
    public unsafe struct DatabaseAssociate
    {
        public uint UID;
        public fixed sbyte Account[16];

        public void FromAssociate(IAssociate Associate)
        {
            this.UID = Associate.UID;
            fixed (sbyte* szAccount = Account)
                Associate.CopyAccountBuffer(szAccount);
        }
        public IAssociate GetAssociate()
        {
            AssociatePacket associate = AssociatePacket.Create();
            associate.UID = UID;
            fixed (sbyte* szAccount = Account)
                associate.CopyToAccountBuffer(szAccount);
            return associate;
        }
    }
}
