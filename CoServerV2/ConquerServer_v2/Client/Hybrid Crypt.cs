using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Client
{
    struct HybridKey_t
    {
        private uint x, y, z, w;
        public HybridKey_t(uint X, uint Y, uint Z, uint W)
        {
            x = y = z = w = 0;
            Reseed(X, Y, Z, W);
        }
        public void Reseed(uint X, uint Y, uint Z, uint W)
        {
            x = X;
            y = Y;
            z = Z;
            w = W;
        }
        public uint GenerateKey()
        {
            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;
            w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));
            return w;
        }
    }
    unsafe struct HybridBigKey_t
    {
        private const int key_capacity = sizeof(uint) * 4;
        private byte[] key_bytes;
        private HybridKey_t key;
        private int pos;
        public HybridBigKey_t(uint initial_key)
        {
            pos = 0;
            key_bytes = null;
            key = new HybridKey_t();
            BigReseed(initial_key);
        }
        public void BigReseed(uint initial_key)
        {
            key_bytes = new byte[key_capacity];
            key.Reseed(initial_key, initial_key ^ 0x12345, initial_key ^ 0xabcdef, initial_key & 0xFFFF);
            fixed (byte* ptr = key_bytes)
            {
                uint* dwkey = (uint*)ptr;
                dwkey[0] = key.GenerateKey();
                dwkey[1] = key.GenerateKey();
                dwkey[2] = key.GenerateKey();
                dwkey[3] = key.GenerateKey();
                pos = 0;
            }
        }
        public void BigEncrypt(byte* block, byte* out_block, int block_size)
        {
            uint block_checksum = 0;
            for (int i = 0; i < block_size; i++)
            {
                block_checksum = (block_checksum << 4) ^ block[i];
                out_block[i] = (byte)(block[i] ^ key_bytes[pos++]);
                if (pos == key_capacity)
                {
                    BigReseed(block_checksum);
                }
            }
        }
        public void BigDecrypt(byte* block, byte* out_block, int block_size)
        {
            uint block_checksum = 0;
            for (int i = 0; i < block_size; i++)
            {
                out_block[i] = (byte)(block[i] ^ key_bytes[pos++]);
                block_checksum = (block_checksum << 4) ^ block[i];
                if (pos == key_capacity)
                {
                    BigReseed(block_checksum);
                }
            }
        }
    }
    class SwordfishCrypto
    {
        private static uint[] InitializeKey = {
	        0x243f6a88, 0x85a308d3, 0x13198a2e, 0x03707344,
	        0xa4093822, 0x299f31d0, 0x082efa98, 0xec4e6c89,
	        0x452821e6, 0x38d01377, 0xbe5466cf, 0x34e90c6c,
	        0xc0ac29b7, 0xc97c50dd, 0x3f84d5b5, 0xb5470917,
	        0x9216d5d9, 0x8979fb1b
        };
        public static int RollLeft(uint Value, byte Roll, byte Size)
        {
            Roll = (byte)(Roll & 0x1F);
            return (int)((Value << Roll) | (Value >> (Size - Roll)));
        }
        public static int RollRight(uint Value, byte Roll, byte Size)
        {
            Roll = (byte)(Roll & 0x1F);
            return (int)((Value << (Size - Roll)) | (Value >> Roll));
        }

        private uint[] Key;
        private byte[] Dec_Ivec;
        private byte[] Enc_Ivec;
        private byte nDec;
        private byte nEnc;

        unsafe void SmallCrypt(ref uint I, byte* I2, uint key)
        {
            I = (I ^ key) ^ ((Key[I2[3] % Key.Length] << 8) | Key[I2[1] % Key.Length]);
        }
        unsafe void BigCrypt(ref uint l, ref uint r)
        {
            l = (l ^ Key[0]);
            fixed (void* Ptr_R = &r, Ptr_L = &l)
            {
                for (byte i = 0; i < 16; i++)
                {
                    if (i % 2 == 0)
                        SmallCrypt(ref r, (byte*)Ptr_L, Key[i]);
                    else
                        SmallCrypt(ref l, (byte*)Ptr_R, Key[i]);
                }
            }
            r = (r ^ Key[17]);
            uint swap = l;
            l = r;
            r = swap;
        }

        public SwordfishCrypto(byte[] userkey)
        {
            Key = new uint[InitializeKey.Length];
            InitializeKey.CopyTo(Key, 0);
            Dec_Ivec = new byte[8];
            Enc_Ivec = new byte[8];
            nDec = 0;
            nEnc = 0;

            for (byte i = 0; i < userkey.Length; i++)
            {
                if (i % 2 == 0)
                    Key[i] = (uint)RollLeft(Key[i], i, 32);
                else
                    Key[i] = (uint)RollRight(Key[i], i, 32);
                Key[i] ^= userkey[i];
            }
            uint l = 0, r = 0;
            for (int j = 0; j < Key.Length - 1; j++)
            {
                BigCrypt(ref l, ref r);
                Key[j] ^= l;
                Key[j + 1] ^= r;
            }
        }
        public unsafe void Encrypt(byte* Data, int DataLength)
        {
            for (int i = 0; i < DataLength; i++)
            {
                if (nEnc == 0)
                {
                    fixed (void* ptr = Enc_Ivec)
                    {
                        uint l = *((uint*)ptr);
                        uint r = *((uint*)ptr + 1);
                        BigCrypt(ref l, ref r);
                        *((uint*)ptr) = l;
                        *((uint*)ptr + 1) = r;
                    }
                }
                Data[i] ^= Enc_Ivec[nEnc];
                nEnc = (byte)((nEnc + 1) % Enc_Ivec.Length);
            }
        }
        public unsafe void Decrypt(byte* Data, int DataLength)
        {
            for (int i = 0; i < DataLength; i++)
            {
                if (nDec == 0)
                {
                    fixed (void* ptr = Dec_Ivec)
                    {
                        uint l = *((uint*)ptr);
                        uint r = *((uint*)ptr + 1);
                        BigCrypt(ref l, ref r);
                        *((uint*)ptr) = l;
                        *((uint*)ptr + 1) = r;
                    }
                }
                Data[i] ^= Dec_Ivec[nDec];
                nDec = (byte)((nDec + 1) % Dec_Ivec.Length);
            }
        }
    }

    /// <summary>
    /// Used for decrypting packets sent accross the auth server, and encrypting them.
    /// </summary>
    public unsafe class AuthCrypto
    {
        private HybridBigKey_t enc;
        private HybridBigKey_t dec;
        public AuthCrypto()
        {
            enc = new HybridBigKey_t(0x3ec271a);
            dec = new HybridBigKey_t(0x3ec271a);
        }
        public void Encrypt(byte* In, byte[] Out, int Size)
        {
            fixed (byte* pOut = Out)
                enc.BigEncrypt(In, pOut, Size);
        }
        public void Decrypt(byte[] In, byte[] Out, int Size)
        {
            fixed (byte* pIn = In, pOut = Out)
                dec.BigDecrypt(pIn, pOut, Size);
        }
    }
    /// <summary>
    /// Used for decrypting information in the 0x3FE packet for spells,
    /// Sent by the client.
    /// </summary>
    public class SpellCrypto
    {
        private SwordfishCrypto fisherman;
        private static byte[] sfkey = new byte[] { 0x92, 0x13, 0x37, 0xa7, 0xa0, 0xb0, 0x0b, 0x01 };

        public SpellCrypto()
        {
            fisherman = new SwordfishCrypto(sfkey);
        }
        public unsafe void Decrypt(ref uint UID, ref ushort SpellID, ref ushort X, ref ushort Y)
        {
            fixed (uint* pUID = &UID)
            {
                fixed (ushort* pSpellID = &SpellID, pX = &X, pY = &Y)
                {
                    fisherman.Decrypt((byte*)pUID, sizeof(uint));
                    fisherman.Decrypt((byte*)pX, sizeof(ushort));
                    fisherman.Decrypt((byte*)pY, sizeof(ushort));
                    fisherman.Decrypt((byte*)pSpellID, sizeof(ushort));
                }
            }
        }
    }
}