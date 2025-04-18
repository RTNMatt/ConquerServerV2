using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;

namespace ConquerServer_v2.Packet_Structures
{
    public unsafe partial class PacketBuilder
    {
        public static SafePointer TQServer;
        static PacketBuilder()
        {
            TQServer = new SafePointer(8);
            for (byte i = 0; i < 8; i++)
                TQServer.Addr[i] = (byte)"TQServer"[i];
        }

        public static bool IsUnsafePacket(byte[] Data)
        {
            fixed (byte* lpData = Data)
            {
                ushort Size = *((ushort*)(lpData + 0));
                if (Data.Length == Size + 8)
                {
                    return IsUnsafePacket(lpData, Size);
                }
            }
            return false;
        }
        public static bool IsUnsafePacket(byte* Ptr, ushort Size)
        {
            if (*((ushort*)Ptr) != Size)
                return true;
            return (MSVCRT.memcmp(Ptr + Size, PacketBuilder.TQServer.Addr, 8) != 0);
        }
        public static bool GetPubKeyFromReply(byte[] Data, out byte[] retn)
        {
            fixed (byte* lpData = Data)
            {
                ushort Offset = (ushort)(*((uint*)(lpData + 11)) + 4 + 11);
                if (Offset > Data.Length)
                {
                    retn = null;
                    return false;
                }
                int retnSize = *((int*)(lpData + Offset));
                if (retnSize > (Data.Length - Offset))
                {
                    retn = null;
                    return false;
                }
                retn = new byte[retnSize];
                fixed (byte* lpRetn = retn)
                    MSVCRT.memcpy(lpRetn, lpData + Offset + 4, retn.Length);
            }
            return true;
        }
        public static void AppendTQServer(byte* Buffer, int BufferSize)
        {
            MSVCRT.memcpy((sbyte*)Buffer + (BufferSize - 8), TQServer.Addr, 8);
        }

        public static uint UInt32FromString(string PacketString)
        {
            string[] data = PacketString.Split(' ');
            byte* block = stackalloc byte[4];
            for (int i = 0; i < 4; i++)
                block[i] = byte.Parse(data[i], NumberStyles.HexNumber);
            return *((uint*)block);
        }
    }
}