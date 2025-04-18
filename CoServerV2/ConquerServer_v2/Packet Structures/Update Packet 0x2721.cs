using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using ConquerServer_v2.Client;

namespace ConquerServer_v2.Packet_Structures
{
    public enum UpdateID : uint
    {
        None = 0xFFFFFFFF,
        Hitpoints = 0,
        MaxHitpoints = 1,
        Mana = 2,
        MaxMana = 3,
        Money = 4,
        Experience = 5,
        PKPoints = 6,
        Job = 7,
        Stamina = 8,
        StatPoints = 10,
        Model = 11,
        Level = 12,
        Spirit = 13,
        Vitality = 14,
        Strength = 15,
        Agility = 16,
        HeavensBlessing = 17,
        DoubleExpTimer = 18,
        // ? ?
        CursedTimer = 20,
        RebornCount = 22,
        // ? ?
        RaiseFlag = 25,
        Hairstyle = 26,
        XPCircle = 27,
        LuckyTimeTimer = 28,
        ConquerPoints = 29,
        MentorBattlePower = 36,
        // ? ? ? ?
        QuizShowPoints = 40
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x24 + 8)]
    public unsafe struct UpdatePacket
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Type;
        [FieldOffset(4)]
        public uint UID;
        [FieldOffset(8)]
        public uint TotalUpdates;
        [FieldOffset(12)]
        public UpdateID ID;
        [FieldOffset(16)]
        public uint Value;
        [FieldOffset(20)]
        public uint Footer;
        [FieldOffset(16)]
        public ulong BigValue;
        [FieldOffset(24)]
        public fixed uint Pad[3];
        [FieldOffset(36)]
        private fixed byte TQServer[8];

        public static UpdatePacket Create()
        {
            UpdatePacket retn = new UpdatePacket();
            retn.Size = 0x24;
            retn.Type = 0x2721;
            retn.TotalUpdates = 1;
            PacketBuilder.AppendTQServer(retn.TQServer, 8);
            return retn;
        }
    }

    public unsafe class BigUpdatePacket
    {
        private byte[] Buffer;
        private struct internalUpdate
        {
            public ushort Size;
            public ushort Type;
            public uint UID;
            public uint UpdateCount;
        }

        const int SizeOf_Data = 20;
        [StructLayout(LayoutKind.Explicit, Size=SizeOf_Data)]
        public struct Data
        {
            [FieldOffset(0)]
            public UpdateID ID;
            [FieldOffset(4)]
            public uint Value;
            [FieldOffset(8)]
            public uint Footer;
            [FieldOffset(4)]
            public ulong qwValue;

            public static Data Create(UpdateID _ID, ulong _Value)
            {
                Data retn = new Data();
                retn.ID = _ID;
                retn.qwValue = _Value;
                return retn;
            }
            public static Data Create(UpdateID _ID, uint _Value)
            {
                Data retn = new Data();
                retn.ID = _ID;
                retn.Value = _Value;
                return retn;
            }
            public static Data Create(UpdateID _ID, int _Value)
            {
                Data retn = new Data();
                retn.ID = _ID;
                retn.Value = (uint)_Value;
                return retn;
            }
            public static Data Create(UpdateID _ID, ushort _Value)
            {
                Data retn = new Data();
                retn.ID = _ID;
                retn.Value = _Value;
                return retn;
            }
            public static Data Create(UpdateID _ID, byte _Value)
            {
                Data retn = new Data();
                retn.ID = _ID;
                retn.Value = _Value;
                return retn;
            }
        }

        public BigUpdatePacket(uint TotalUpdates)
        {
            Buffer = new byte[32 + (TotalUpdates * SizeOf_Data) + 8];
            fixed (byte* _iUpdate = Buffer)
            {
                internalUpdate* iUpdate = (internalUpdate*)_iUpdate;
                iUpdate->Size = (ushort)(Buffer.Length - 8);
                iUpdate->Type = 0x2721;
                iUpdate->UpdateCount = TotalUpdates;
                PacketBuilder.AppendTQServer((byte*)iUpdate, (ushort)Buffer.Length);
            }
        }
        public uint UID
        {
            get
            {
                fixed (byte* iUpdate = Buffer)
                    return ((internalUpdate*)iUpdate)->UID;
            }
            set
            {
                fixed (byte* iUpdate = Buffer)
                    ((internalUpdate*)iUpdate)->UID = value;
            }
        }
        public static implicit operator byte[](BigUpdatePacket big)
        {
            return big.Buffer;
        }

        public void Append(int UpdateNumber, UpdateID ID, ulong Value)
        {
            fixed (byte* iUpdate = Buffer)
            {
                *((BigUpdatePacket.Data*)(iUpdate + 12 + (UpdateNumber * SizeOf_Data))) =
                    BigUpdatePacket.Data.Create(ID, Value);
            }
        }
        public void Append(int UpdateNumber, UpdateID ID, uint Value)
        {
            fixed (byte* iUpdate = Buffer)
            {
                *((BigUpdatePacket.Data*)(iUpdate + 12 + (UpdateNumber * SizeOf_Data))) =
                    BigUpdatePacket.Data.Create(ID, Value);
            }
        }
        public void Append(int UpdateNumber, UpdateID ID, ushort Value)
        {
            fixed (byte* iUpdate = Buffer)
            {
                *((BigUpdatePacket.Data*)(iUpdate + 12 + (UpdateNumber * SizeOf_Data))) =
                    BigUpdatePacket.Data.Create(ID, Value);
            }
        }
        public void Append(int UpdateNumber, UpdateID ID, int Value)
        {
            fixed (byte* iUpdate = Buffer)
            {
                *((BigUpdatePacket.Data*)(iUpdate + 12 + (UpdateNumber * SizeOf_Data))) =
                    BigUpdatePacket.Data.Create(ID, Value);
            }
        }
        public void Append(int UpdateNumber, UpdateID ID, byte Value)
        {
            fixed (byte* iUpdate = Buffer)
            {
                *((BigUpdatePacket.Data*)(iUpdate + 12 + (UpdateNumber * SizeOf_Data))) =
                    BigUpdatePacket.Data.Create(ID, Value);
            }
        }
        public void HitpointsAndMana(GameClient Client, int StartIndex)
        {
            this.Append(StartIndex, UpdateID.Hitpoints, Client.Entity.Hitpoints);
            this.Append(StartIndex + 1, UpdateID.Mana, Client.Manapoints);
        }
        public void AllStats(GameClient Client, int StartIndex)
        {
            this.Append(StartIndex, UpdateID.Spirit, Client.Stats.Spirit);
            this.Append(StartIndex + 1, UpdateID.Vitality, Client.Stats.Vitality);
            this.Append(StartIndex + 2, UpdateID.Strength, Client.Stats.Strength);
            this.Append(StartIndex + 3, UpdateID.Agility, Client.Stats.Agility);
            this.Append(StartIndex + 4, UpdateID.StatPoints, Client.Stats.StatPoints);
        }
    }
}