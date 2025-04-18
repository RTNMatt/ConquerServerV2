using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ConquerServer_v2.Packet_Structures
{
    public enum NpcClickID : byte
    {
        None = 0x00,
        Dialogue = 0x01,
        Option = 0x02,
        Input = 0x03,
        Avatar = 0x04,
        Finish = 0x64,
        DeleteGuildMember = 0x66
    }

    [StructLayout(LayoutKind.Explicit)]
    /// <summary>
    /// 0x7EF (Client->Server),
    /// 0x7F0 (Client->Server)
    /// 
    /// </summary>
    public unsafe struct NpcClickPacket
    {
        [FieldOffset(0)]
        public ushort Size;
        [FieldOffset(2)]
        public ushort Type;
        [FieldOffset(4)]
        public uint NpcID;
        [FieldOffset(8)]
        public ushort Avatar;
        [FieldOffset(8)]
        public ushort MaxInputLength;
        [FieldOffset(10)]
        public byte OptionID;
        [FieldOffset(11)]
        public NpcClickID ResponseID;
        [FieldOffset(12)]
        public bool DontDisplay;
        [FieldOffset(13)]
        public byte InputLength;
        [FieldOffset(14)]
        private sbyte szInput;
        public string Input
        {
            get { fixed (sbyte* ptr = &szInput) { return new string(ptr, 0, InputLength); } }
            set { fixed (sbyte* ptr = &szInput) { value.CopyTo(ptr); } }
        }
        /// <summary>
        /// Initializes a memory-block to be a NpcClickPacket, of type 0x7F0
        /// </summary>
        /// <param name="Ptr">The memory-block (make sure this has sufficient size for your string).</param>
        /// <param name="TextLength">The length of your input string.</param>
        /// <returns></returns>
        public static NpcClickPacket* Create(void* Ptr, int TextLength)
        {
            NpcClickPacket* packet = (NpcClickPacket*)Ptr;
            packet->Size = (ushort)(0x11 + TextLength);
            packet->Type = 0x7F0;
            packet->OptionID = 0xFF;
            packet->DontDisplay = true;
            packet->InputLength = (byte)TextLength;
            PacketBuilder.AppendTQServer((byte*)Ptr, packet->Size + 8);
            return packet;
        }
    }
}
