using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public enum StringID : byte
    {
        GuildName = 0x03,
        Spouse = 0x06,
        Effect = 0x0A,
        GuildMemberList = 0x0B,
        ViewEquipment = 0x10,
        Sound = 0x14,
        AllyGuild = 0x15,
        EnemyGuild = 0x16
    }

    public unsafe class StringPacket
    {
        public ushort Size, Type;
        public string[] Strings;
        public uint UID;
        public byte StringsLength;
        public StringID ID;
        public static implicit operator byte[](StringPacket Packet)
        {
            byte[] Buffer = new byte[20 + Packet.Strings.Length + Packet.StringsLength];
            fixed (byte* Pointer = Buffer)
            {
                Packet.Size = *((ushort*)(Pointer + 0)) = (ushort)(Buffer.Length - 8);
                Packet.Type = *((ushort*)(Pointer + 2)) = 0x3F7;
                *((uint*)(Pointer + 4)) = Packet.UID;
                *((StringID*)(Pointer + 8)) = Packet.ID;
                *((byte*)(Pointer + 9)) = (byte)Packet.Strings.Length;
                if (Packet.Strings != null)
                {
                    ushort i = 10;
                    for (byte i2 = 0; i2 < Packet.Strings.Length; i2++)
                    {
                        Buffer[i] = (byte)Packet.Strings[i2].Length;
                        Packet.Strings[i2].CopyTo(i + 1 + Pointer);
                        i += (ushort)(1 + Buffer[i]);
                    }
                }
                PacketBuilder.AppendTQServer(Pointer, Buffer.Length);
            }
            return Buffer;
        }
        public static implicit operator StringPacket(byte* Packet)
        {
            // No strings are ever passed in the byte[]
            StringPacket retn = new StringPacket();
            //fixed (byte* Packet = Bytes)
            //{
                retn.Size = *((ushort*)(Packet + 0));
                retn.Type = *((ushort*)(Packet + 2));
                retn.UID = *((uint*)(Packet + 4));
                retn.ID = *((StringID*)(Packet + 8));
            //}
            return retn;
        }
    }
}