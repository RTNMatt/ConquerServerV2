using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;

namespace ConquerServer_v2.Packet_Structures
{
    public enum AssociationID : byte
    {
        AddFriend = 0x0F,
        RemoveFriend = 0x0E,
        SetOfflineFriend = 0x0D,
        SetOnlineFriend = 0x0C,
        NewFriend = 0x0B,
        RequestFriend = 0x0A,

        AddEnemy = 0x13,
        RemoveEnemy = 0x12,
        SetOfflineEnemy = 0x11,
        SetOnlineEnemy = 0x10
    }

    /// <summary>
    /// 0x3FB (Server->Client)
    /// </summary>
    public unsafe struct AssociatePacket : IAssociate
    {
        public ushort Size;
        public ushort Type;
        private uint m_UID;
        private AssociationID m_ID;
        private byte m_Online;
#pragma warning disable
        private fixed sbyte Junk[10];
#pragma warning restore
        private fixed sbyte szName[16];
        public fixed byte TQServer[8]; 
        private fixed sbyte szAccount[16];

        // IAssociate
        public uint UID { get { return m_UID; } set { m_UID = value; } }
        public AssociationID ID { get { return m_ID; } set { m_ID = value; } }
        public bool Online { get { return (m_Online == 1); } set { m_Online = (byte)(value ? 1 : 0); } }
        public string Name
        {
            get { fixed (sbyte* bp = szName) { return new string(bp); } }
            set { fixed (sbyte* bp = szName) { MSVCRT.memset(bp, 0, 16); value.CopyTo(bp); } }
        }
        public string Account
        {
            get { fixed (sbyte* bp = szAccount) { return new string(bp); } }
            set { fixed (sbyte* bp = szAccount) { MSVCRT.memset(bp, 0, 16); value.CopyTo(bp); } }
        }

        public void CopyNameBuffer(sbyte* Dest)
        {
            fixed (sbyte* bp = szName)
                MSVCRT.memcpy(Dest, bp, 16);
        }
        public void CopyAccountBuffer(sbyte* Dest)
        {
            fixed (sbyte* bp = szAccount)
                MSVCRT.memcpy(Dest, bp, 16);
        }
        public void CopyToNameBuffer(sbyte* Src)
        {
            fixed (sbyte* bp = szName)
                MSVCRT.memcpy(bp, Src, 16);
        }
        public void CopyToAccountBuffer(sbyte* Src)
        {
            fixed (sbyte* bp = szAccount)
                MSVCRT.memcpy(bp, Src, 16);
        }
        public void Send(GameClient Client)
        {
            fixed (AssociatePacket* pThis = &this)
                Client.Send(pThis);
        }

        public static AssociatePacket Create()
        {
            AssociatePacket retn = new AssociatePacket();
            retn.Size = 0x24;
            retn.Type = 0x3fb;
            PacketBuilder.AppendTQServer(retn.TQServer, 8);
            return retn;
        }
    }
}
