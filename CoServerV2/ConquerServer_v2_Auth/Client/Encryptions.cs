using System;

namespace ConquerServer_v2.Client
{

    public sealed class RC5Exception : Exception
    {
        public RC5Exception(string message) : base(message) { }
    }

    public sealed class RC5
    {
        private readonly uint[] _bufKey = new uint[4];
        private readonly uint[] _bufSub = new uint[26];

        public RC5(byte[] data)
        {
            if (data.Length != 16) throw new RC5Exception("Invalid data length. Must be 16 bytes");
            const uint p32 = 0xB7E15163;
            const uint q32 = 0x61C88647;
            uint offsetA = 0, offsetB = 0, A = 0, B = 0;
            for (int i = 0; i < 4; i++)
                _bufKey[i] = (uint)(data[i * 4] + (data[i * 4 + 1] << 8) + (data[i * 4 + 2] << 16) + (data[i * 4 + 3] << 24));
            _bufSub[0] = p32;
            for (int i = 1; i < 26; i++)
            {
                _bufSub[i] = _bufSub[i - 1] - q32;
            }
            for (int s = 1; s <= 78; s++)
            {
                _bufSub[offsetA] = LeftRotate(_bufSub[offsetA] + A + B, 3);
                A = _bufSub[offsetA];
                offsetA = (offsetA + 1) % 0x1A;
                _bufKey[offsetB] = LeftRotate(_bufKey[offsetB] + A + B, (int)(A + B));
                B = _bufKey[offsetB];
                offsetB = (offsetB + 1) % 4;
            }
        }
        public byte[] Decrypt(byte[] data)
        {
            if (data.Length % 8 != 0) throw new RC5Exception("Invalid password length. Must be multiple of 8");
            int nLen = data.Length / 8 * 8;
            if (nLen <= 0) throw new RC5Exception("Invalid password length. Must be greater than 0 bytes.");
            uint[] bufData = new uint[data.Length / 4];
            for (int i = 0; i < data.Length / 4; i++)
                bufData[i] = (uint)(data[i * 4] + (data[i * 4 + 1] << 8) + (data[i * 4 + 2] << 16) + (data[i * 4 + 3] << 24));
            for (int i = 0; i < nLen / 8; i++)
            {
                uint ld = bufData[2 * i];
                uint rd = bufData[2 * i + 1];
                for (int j = 12; j >= 1; j--)
                {
                    rd = RightRotate((rd - _bufSub[2 * j + 1]), (int)ld) ^ ld;
                    ld = RightRotate((ld - _bufSub[2 * j]), (int)rd) ^ rd;
                }
                uint B = rd - _bufSub[1];
                uint A = ld - _bufSub[0];
                bufData[2 * i] = A;
                bufData[2 * i + 1] = B;
            }
            byte[] result = new byte[bufData.Length * 4];
            for (int i = 0; i < bufData.Length; i++)
            {
                result[i * 4] = (byte)bufData[i];
                result[i * 4 + 1] = (byte)(bufData[i] >> 8);
                result[i * 4 + 2] = (byte)(bufData[i] >> 16);
                result[i * 4 + 3] = (byte)(bufData[i] >> 24);
            }
            return result;
        }

        public byte[] Encrypt(byte[] data)
        {
            if (data.Length % 8 != 0) throw new RC5Exception("Invalid password length. Must be multiple of 8");
            int nLen = data.Length / 8 * 8;
            if (nLen <= 0) throw new RC5Exception("Invalid password length. Must be greater than 0 bytes.");
            uint[] bufData = new uint[data.Length / 4];
            for (int i = 0; i < data.Length / 4; i++)
                bufData[i] = (uint)(data[i * 4] + (data[i * 4 + 1] << 8) + (data[i * 4 + 2] << 16) + (data[i * 4 + 3] << 24));
            for (int i = 0; i < nLen / 8; i++)
            {
                uint A = bufData[i * 2];
                uint B = bufData[i * 2 + 1];
                uint le = A + _bufSub[0];
                uint re = B + _bufSub[1];
                for (int j = 1; j <= 12; j++)
                {
                    le = LeftRotate((le ^ re), (int)re) + _bufSub[j * 2];
                    re = LeftRotate((re ^ le), (int)le) + _bufSub[j * 2 + 1];
                }
                bufData[i * 2] = le;
                bufData[i * 2 + 1] = re;
            }
            byte[] result = new byte[bufData.Length * 4];
            for (int i = 0; i < bufData.Length; i++)
            {
                result[i * 4] = (byte)bufData[i];
                result[i * 4 + 1] = (byte)(bufData[i] >> 8);
                result[i * 4 + 2] = (byte)(bufData[i] >> 16);
                result[i * 4 + 3] = (byte)(bufData[i] >> 24);
            }
            return result;
        }

        internal static uint LeftRotate(uint dwVar, int dwOffset)
        {
            return (dwVar << (dwOffset & 0x1F) | dwVar >> 0x20 - (dwOffset & 0x1F));
        }

        internal static uint RightRotate(uint dwVar, int dwOffset)
        {
            return (dwVar >> (dwOffset & 0x1F) | dwVar << 0x20 - (dwOffset & 0x1F));
        }
    }


    public class Assembler
    {
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
    }
    public class ConquerKeys
    {
        #region public static byte[] Key1 = new byte[256];
        public static byte[] Key1 = 
            {
			    0x9D, 0x90, 0x83, 0x8A, 0xD1, 0x8C, 0xE7, 0xF6, 0x25, 0x28, 0xEB, 0x82, 0x99, 0x64, 0x8F, 0x2E,
			    0x2D, 0x40, 0xD3, 0xFA, 0xE1, 0xBC, 0xB7, 0xE6, 0xB5, 0xD8, 0x3B, 0xF2, 0xA9, 0x94, 0x5F, 0x1E, 
		    	0xBD, 0xF0, 0x23, 0x6A, 0xF1, 0xEC, 0x87, 0xD6, 0x45, 0x88, 0x8B, 0x62, 0xB9, 0xC4, 0x2F, 0x0E, 
		        0x4D, 0xA0, 0x73, 0xDA, 0x01, 0x1C, 0x57, 0xC6, 0xD5, 0x38, 0xDB, 0xD2, 0xC9, 0xF4, 0xFF, 0xFE, 
		    	0xDD, 0x50, 0xC3, 0x4A, 0x11, 0x4C, 0x27, 0xB6, 0x65, 0xE8, 0x2B, 0x42, 0xD9, 0x24, 0xCF, 0xEE, 
		    	0x6D, 0x00, 0x13, 0xBA, 0x21, 0x7C, 0xF7, 0xA6, 0xF5, 0x98, 0x7B, 0xB2, 0xE9, 0x54, 0x9F, 0xDE, 
		    	0xFD, 0xB0, 0x63, 0x2A, 0x31, 0xAC, 0xC7, 0x96, 0x85, 0x48, 0xCB, 0x22, 0xF9, 0x84, 0x6F, 0xCE, 
		        0x8D, 0x60, 0xB3, 0x9A, 0x41, 0xDC, 0x97, 0x86, 0x15, 0xF8, 0x1B, 0x92, 0x09, 0xB4, 0x3F, 0xBE, 
		        0x1D, 0x10, 0x03, 0x0A, 0x51, 0x0C, 0x67, 0x76, 0xA5, 0xA8, 0x6B, 0x02, 0x19, 0xE4, 0x0F, 0xAE, 
		    	0xAD, 0xC0, 0x53, 0x7A, 0x61, 0x3C, 0x37, 0x66, 0x35, 0x58, 0xBB, 0x72, 0x29, 0x14, 0xDF, 0x9E, 
			    0x3D, 0x70, 0xA3, 0xEA, 0x71, 0x6C, 0x07, 0x56, 0xC5, 0x08, 0x0B, 0xE2, 0x39, 0x44, 0xAF, 0x8E, 
			    0xCD, 0x20, 0xF3, 0x5A, 0x81, 0x9C, 0xD7, 0x46, 0x55, 0xB8, 0x5B, 0x52, 0x49, 0x74, 0x7F, 0x7E, 
		        0x5D, 0xD0, 0x43, 0xCA, 0x91, 0xCC, 0xA7, 0x36, 0xE5, 0x68, 0xAB, 0xC2, 0x59, 0xA4, 0x4F, 0x6E, 
			    0xED, 0x80, 0x93, 0x3A, 0xA1, 0xFC, 0x77, 0x26, 0x75, 0x18, 0xFB, 0x32, 0x69, 0xD4, 0x1F, 0x5E, 
			    0x7D, 0x30, 0xE3, 0xAA, 0xB1, 0x2C, 0x47, 0x16, 0x05, 0xC8, 0x4B, 0xA2, 0x79, 0x04, 0xEF, 0x4E, 
			    0x0D, 0xE0, 0x33, 0x1A, 0xC1, 0x5C, 0x17, 0x06, 0x95, 0x78, 0x9B, 0x12, 0x89, 0x34, 0xBF, 0x3E
            };
        #endregion
        #region public static byte[] Key2 = new byte[256];
        public static byte[] Key2 =
            {
				0x62, 0x4F, 0xE8, 0x15, 0xDE, 0xEB, 0x04, 0x91, 0x1A, 0xC7, 0xE0, 0x4D, 0x16, 0xE3, 0x7C, 0x49,
				0xD2, 0x3F, 0xD8, 0x85, 0x4E, 0xDB, 0xF4, 0x01, 0x8A, 0xB7, 0xD0, 0xBD, 0x86, 0xD3, 0x6C, 0xB9,
				0x42, 0x2F, 0xC8, 0xF5, 0xBE, 0xCB, 0xE4, 0x71, 0xFA, 0xA7, 0xC0, 0x2D, 0xF6, 0xC3, 0x5C, 0x29,
				0xB2, 0x1F, 0xB8, 0x65, 0x2E, 0xBB, 0xD4, 0xE1, 0x6A, 0x97, 0xB0, 0x9D, 0x66, 0xB3, 0x4C, 0x99,
				0x22, 0x0F, 0xA8, 0xD5, 0x9E, 0xAB, 0xC4, 0x51, 0xDA, 0x87, 0xA0, 0x0D, 0xD6, 0xA3, 0x3C, 0x09,
				0x92, 0xFF, 0x98, 0x45, 0x0E, 0x9B, 0xB4, 0xC1, 0x4A, 0x77, 0x90, 0x7D, 0x46, 0x93, 0x2C, 0x79,
				0x02, 0xEF, 0x88, 0xB5, 0x7E, 0x8B, 0xA4, 0x31, 0xBA, 0x67, 0x80, 0xED, 0xB6, 0x83, 0x1C, 0xE9,
				0x72, 0xDF, 0x78, 0x25, 0xEE, 0x7B, 0x94, 0xA1, 0x2A, 0x57, 0x70, 0x5D, 0x26, 0x73, 0x0C, 0x59,
				0xE2, 0xCF, 0x68, 0x95, 0x5E, 0x6B, 0x84, 0x11, 0x9A, 0x47, 0x60, 0xCD, 0x96, 0x63, 0xFC, 0xC9,
				0x52, 0xBF, 0x58, 0x05, 0xCE, 0x5B, 0x74, 0x81, 0x0A, 0x37, 0x50, 0x3D, 0x06, 0x53, 0xEC, 0x39,
				0xC2, 0xAF, 0x48, 0x75, 0x3E, 0x4B, 0x64, 0xF1, 0x7A, 0x27, 0x40, 0xAD, 0x76, 0x43, 0xDC, 0xA9,
				0x32, 0x9F, 0x38, 0xE5, 0xAE, 0x3B, 0x54, 0x61, 0xEA, 0x17, 0x30, 0x1D, 0xE6, 0x33, 0xCC, 0x19,
				0xA2, 0x8F, 0x28, 0x55, 0x1E, 0x2B, 0x44, 0xD1, 0x5A, 0x07, 0x20, 0x8D, 0x56, 0x23, 0xBC, 0x89,
				0x12, 0x7F, 0x18, 0xC5, 0x8E, 0x1B, 0x34, 0x41, 0xCA, 0xF7, 0x10, 0xFD, 0xC6, 0x13, 0xAC, 0xF9,
				0x82, 0x6F, 0x08, 0x35, 0xFE, 0x0B, 0x24, 0xB1, 0x3A, 0xE7, 0x00, 0x6D, 0x36, 0x03, 0x9C, 0x69,
				0xF2, 0x5F, 0xF8, 0xA5, 0x6E, 0xFB, 0x14, 0x21, 0xAA, 0xD7, 0xF0, 0xDD, 0xA6, 0xF3, 0x8C, 0xD9
            };
        #endregion
        #region public static uint[] PasswordKey = new uint[30];
        public static uint[] PasswordKey = new uint[] 
           {
                0xEBE854BC, 0xB04998F7, 0xFFFAA88C, 
                0x96E854BB, 0xA9915556, 0x48E44110, 
                0x9F32308F, 0x27F41D3E, 0xCF4F3523, 
                0xEAC3C6B4, 0xE9EA5E03, 0xE5974BBA, 
                0x334D7692, 0x2C6BCF2E, 0xDC53B74,  
                0x995C92A6, 0x7E4F6D77, 0x1EB2B79F, 
                0x1D348D89, 0xED641354, 0x15E04A9D,
                0x488DA159, 0x647817D3, 0x8CA0BC20, 
				0x9264F7FE, 0x91E78C6C, 0x5C9A07FB, 
                0xABD4DCCE, 0x6416F98D, 0x6642AB5B 
           };
        #endregion
    }
    public class PasswordCrypter
    {
        public unsafe static sbyte* Decrypt(uint* Password)
        {
            uint temp1, temp2;
            for (sbyte i = 1; i >= 0; i--)
            {
                temp1 = *((uint*)(Password + (i * 2 + 1)));
                temp2 = *((uint*)(Password + (i * 2)));
                for (sbyte i2 = 11; i2 >= 0; i2--)
                {
                    temp1 = (uint)Assembler.RollRight(temp1 - ConquerKeys.PasswordKey[i2 * 2 + 7], (byte)temp2, 32) ^ temp2;
                    temp2 = (uint)Assembler.RollRight(temp2 - ConquerKeys.PasswordKey[i2 * 2 + 6], (byte)temp1, 32) ^ temp1;
                }
                *((uint*)Password + (i * 2 + 1)) = temp1 - ConquerKeys.PasswordKey[5];
                *((uint*)Password + (i * 2)) = temp2 - ConquerKeys.PasswordKey[4];
            }
            return (sbyte*)Password;
        }
        public unsafe static string Decrypt(string HexEncodedPassword)
        {
            byte* Password = stackalloc byte[16];
            int i = 0;
            while (HexEncodedPassword != "")
            {
                Password[i] = byte.Parse(HexEncodedPassword.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                HexEncodedPassword = HexEncodedPassword.Remove(0, 2);
                i++;
            }
            return new string(Decrypt((uint*)Password));
        }
    }
    public class AuthCryptographer
    {
        private ushort InCounter;
        private ushort OutCounter;
        private bool Server;

        public AuthCryptographer(bool IsServer)
        {
            Server = IsServer;
            InCounter = OutCounter = 0;
        }
        public void Encrypt(byte[] In, byte[] Out, int Size)
        {
            lock (this)
            {
                for (int i = 0; i < Size; i++)
                {
                    if (Server)
                    {
                        Out[i] = (byte)(In[i] ^ 0xAB);
                        Out[i] = (byte)((Out[i] << 4) | (Out[i] >> 4));
                        Out[i] = (byte)(ConquerKeys.Key2[OutCounter >> 8] ^ Out[i]);
                        Out[i] = (byte)(ConquerKeys.Key1[OutCounter & 0xFF] ^ Out[i]);
                    }
                    else
                    {
                        Out[i] = (byte)(ConquerKeys.Key1[OutCounter & 0xFF] ^ In[i]);
                        Out[i] = (byte)(ConquerKeys.Key2[OutCounter >> 8] ^ Out[i]);
                        Out[i] = (byte)((Out[i] << 4) | (Out[i] >> 4));
                        Out[i] = (byte)(Out[i] ^ 0xAB);
                    }
                    OutCounter = (ushort)(OutCounter + 1);
                }
            }
        }
        public unsafe void Encrypt(byte* In, byte[] Out, int Size)
        {
            lock (this)
            {
                for (int i = 0; i < Size; i++)
                {
                    if (Server)
                    {
                        Out[i] = (byte)(In[i] ^ 0xAB);
                        Out[i] = (byte)((Out[i] << 4) | (Out[i] >> 4));
                        Out[i] = (byte)(ConquerKeys.Key2[OutCounter >> 8] ^ Out[i]);
                        Out[i] = (byte)(ConquerKeys.Key1[OutCounter & 0xFF] ^ Out[i]);
                    }
                    else
                    {
                        Out[i] = (byte)(ConquerKeys.Key1[OutCounter & 0xFF] ^ In[i]);
                        Out[i] = (byte)(ConquerKeys.Key2[OutCounter >> 8] ^ Out[i]);
                        Out[i] = (byte)((Out[i] << 4) | (Out[i] >> 4));
                        Out[i] = (byte)(Out[i] ^ 0xAB);
                    }
                    OutCounter = (ushort)(OutCounter + 1);
                }
            }
        }
        public void Decrypt(byte[] In, byte[] Out, int Size)
        {
            lock (this)
            {
                for (ushort i = 0; i < Size; i++)
                {
                    if (Server)
                    {
                        Out[i] = (byte)(In[i] ^ 0xAB);
                        Out[i] = (byte)((Out[i] << 4) | (Out[i] >> 4));
                        Out[i] = (byte)(ConquerKeys.Key2[InCounter >> 8] ^ Out[i]);
                        Out[i] = (byte)(ConquerKeys.Key1[InCounter & 0xFF] ^ Out[i]);
                    }
                    else
                    {
                        Out[i] = (byte)(ConquerKeys.Key1[InCounter & 0xFF] ^ In[i]);
                        Out[i] = (byte)(ConquerKeys.Key2[InCounter >> 8] ^ Out[i]);
                        Out[i] = (byte)((Out[i] << 4) | (Out[i] >> 4));
                        Out[i] = (byte)(Out[i] ^ 0xAB);
                    }
                    InCounter = (ushort)(InCounter + 1);
                }
            }
        }
    }
}