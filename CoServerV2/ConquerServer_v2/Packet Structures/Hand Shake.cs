using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Structures
{
    public unsafe partial class PacketBuilder
    {
        public static byte[] HandShakePacket(byte[] g, byte[] p, byte[] pub_key)
        {
            int Size = 47 + p.Length + g.Length + pub_key.Length + 12 + 8 + 8;
            byte[] buffer = new byte[Size];
            fixed (byte* lpBuffer = buffer)
            {
                int rand = Kernel.Random.Next() % ushort.MaxValue;
                *((int*)(lpBuffer + 0)) = rand;
                *((int*)(lpBuffer + 4)) = rand;
                *((byte*)(lpBuffer + 8)) = 1;
                *((ushort*)(lpBuffer + 9)) = 4940;
                *((uint*)(lpBuffer + 11)) = (uint)(Size - 11);
                *((uint*)(lpBuffer + 15)) = (uint)12;
                *((int*)(lpBuffer + 19)) = rand;
                *((int*)(lpBuffer + 23)) = rand;
                *((int*)(lpBuffer + 27)) = rand;
                *((int*)(lpBuffer + 31)) = 8; // client_bf_data.length
                *((uint*)(lpBuffer + 43)) = 8; // server_bf_data.length
                
                *((int*)(lpBuffer + 55)) = p.Length;
                ushort Offset = 59;
                fixed (byte* lpP = p)
                    MSVCRT.memcpy(lpBuffer + Offset, lpP, p.Length);
                Offset += (ushort)p.Length;
                *((int*)(lpBuffer + Offset)) = g.Length;
                fixed (byte* lpG = g)
                    MSVCRT.memcpy(lpBuffer + Offset + 4, lpG, g.Length);
                Offset += (ushort)(g.Length + 4);
                *((int*)(lpBuffer + Offset)) = pub_key.Length;
                fixed (byte* lpKey = pub_key)
                    MSVCRT.memcpy(lpBuffer + Offset + 4, lpKey, pub_key.Length);
                Offset += (ushort)(pub_key.Length + 4);
                AppendTQServer(lpBuffer, (ushort)buffer.Length);
            }
            return buffer;
        }
    }
}