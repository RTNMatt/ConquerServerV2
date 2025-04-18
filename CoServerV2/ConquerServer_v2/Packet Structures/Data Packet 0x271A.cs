using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public enum DataID : ushort
    {
        NinjaStep = 0x9C,
        EndFly = 0x78,
        GUIDialog = 0x7E,
        SetLocation = 0x4A,
        SetMapColor = 0x68,
        Jump = 0x89,
        UnlearnSpell = 0x6D,
        UnlearnProficiency = 0x6E,
        GuardJump = 0x82,
        LevelUp = 0x5C,
        FriendInfo = 0x8C,
        Teleport = 0x56,
        GetSurroundings = 0x72,
        RemoveEntity = 0x87,
        RequestTeamPosition = 0x6A,
        ChangePkMode = 0x60,
        Revive = 0x5E,
        RequestEntity = 0x66,
        ChangeAction = 0x51,
        ChangeDirection = 0x4F,
        Hotkeys = 0x4B,
        ConfirmAssociates = 0x4C,
        ConfirmProficiencies = 0x4D,
        ConfirmSpells = 0x4E,
        ConfirmGuild = 0x61,
        Login = 251,
        ChangeAvatar = 0x97,
        EnterPortal = 0x55,
        DeleteCharacter = 0x5F,
        Switch = 0x74,
        RequestFriendInfo = 0x94,
        EndTransform = 0x76,
        Mining = 0x63,
        StartVend = 0x6F,
        SpawnEffect = 0x86,
        Shop = 0xA0,
        None = 0x00
    }

    public class DataSwitchArg
    {
        public const uint
            MarriageMouse = 1067,
            EnchantWindow = 1091;
    }

    public class DataGUIDialog
    {
        public const uint
            Warehouse = 4,
            Composition = 1;
    }

    /// <summary>
    /// 0x271A (Server->Client, Client->Server)
    /// </summary>
    public unsafe struct DataPacket
    {
        public ushort Size;
        public ushort Type;
        public uint UID; //ok
        public uint dwParam1; //32 Packet ID
        public ushort dwParam_Lo 
        { 
            get { return (ushort)dwParam1; }
            set { dwParam1 = (uint)((dwParam_Hi << 16) | value); }
        }
        public ushort dwParam_Hi 
        { 
            get { return (ushort)(dwParam1 >> 16); }
            set { dwParam1 = (uint)((value << 16) | dwParam_Lo); }
        }
        private fixed byte Junk1[4];
        public uint TimeStamp; //SHOP ID 
        public DataID ID;
        private fixed byte Junk2[2];
        public ushort wParam1; //NPC X
        public ushort wParam2; // NPC Y
        private fixed byte Junk3[9];
        private fixed byte TQServer[8];

        public static DataPacket Create()
        {
            DataPacket packet = new DataPacket();
            packet.Size = 0x25;
            packet.Type = 0x271A;
            packet.TimeStamp = Convert.ToUInt32(TIME.Now.ToString());
            PacketBuilder.AppendTQServer(packet.TQServer, 8);
            return packet;
        }

        public static DataPacket Create(uint SetNewTimeStamp)
        {
            DataPacket packet = new DataPacket();
            packet.Size = 0x25;
            packet.Type = 0x271A;
            packet.TimeStamp = SetNewTimeStamp;
            PacketBuilder.AppendTQServer(packet.TQServer, 8);
            return packet;
        }
    }
}
