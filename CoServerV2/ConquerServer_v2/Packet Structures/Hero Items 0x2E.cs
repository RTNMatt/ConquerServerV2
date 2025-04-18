using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Structures
{
    /// <summary>
    /// 0x3F1(Server->Client)
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct HeroItemsPacket
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Type;
        [FieldOffset(12)]
        public ushort ID;


        [FieldOffset(32)]
        public uint HelmetID;
        [FieldOffset(36)]
        public uint NecklaceID;
        [FieldOffset(40)]
        public uint ArmorID;
        [FieldOffset(44)]
        public uint RightHandID;
        [FieldOffset(48)]
        public uint LeftHandID;
        [FieldOffset(52)]
        public uint RingID;
        [FieldOffset(56)]
        public uint TalismanID;
        [FieldOffset(60)]
        public uint BootsID;
        [FieldOffset(64)]
        public uint GarmentID;
        [FieldOffset(68)]
        public uint FanID;
        [FieldOffset(72)]
        public uint TowerID;

        [FieldOffset(76)]
        public fixed byte TQServer[8];

        public HeroItemsPacket Create(GameClient Client)
        {
            // Size and TQServer are appended when SetName() is called.
            HeroItemsPacket packet = new HeroItemsPacket();
            packet.Size = 76;
            packet.Type = 1009;
            packet.ID = 0x2E;

            if(Client.Equipment[ItemPosition.Head] != null)
                packet.HelmetID = Client.Equipment[ItemPosition.Head].UID;
            if (Client.Equipment[ItemPosition.Armor] != null)
                packet.ArmorID = Client.Equipment[ItemPosition.Armor].UID;
            if (Client.Equipment[ItemPosition.Left] != null)
                packet.LeftHandID = Client.Equipment[ItemPosition.Left].UID;
            if (Client.Equipment[ItemPosition.Right] != null)
                packet.RightHandID = Client.Equipment[ItemPosition.Right].UID;


            PacketBuilder.AppendTQServer(packet.TQServer, 8);
            return packet;
        }
    }
}
