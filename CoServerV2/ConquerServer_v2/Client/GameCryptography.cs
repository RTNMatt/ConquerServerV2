using System;
using System.Runtime.InteropServices;

namespace ConquerServer_v2.Client
{
    public class GameCryptography
    {
        Blowfish _blowfish;
        public GameCryptography(string key)
        {
            _blowfish = new Blowfish(BlowfishAlgorithm.CFB64);
            _blowfish.SetKey(System.Text.ASCIIEncoding.ASCII.GetBytes(key));
        }

        public void Decrypt(byte[] packet)
        {
            byte[] buffer = _blowfish.Decrypt(packet);
            System.Buffer.BlockCopy(buffer, 0, packet, 0, buffer.Length);
        }

        public void Encrypt(byte[] packet)
        {
            byte[] buffer = _blowfish.Encrypt(packet);
            System.Buffer.BlockCopy(buffer, 0, packet, 0, buffer.Length);
        }

        public Blowfish Blowfish
        {
            get { return _blowfish; }
        }
        public void SetKey(byte[] k)
        {
            _blowfish.SetKey(k);
        }
        public void SetIvs(byte[] i1, byte[] i2)
        {
            _blowfish.EncryptIV = i1;
            _blowfish.DecryptIV = i2;
        }
    }

    public enum BlowfishAlgorithm
    {
        ECB,
        CBC,
        CFB64,
        OFB64,
    };

    public class Blowfish : IDisposable
    {
        [DllImport("libeay32.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static void CAST_set_key(IntPtr _key, int len, byte[] data);

        [DllImport("libeay32.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static void CAST_cfb64_encrypt(byte[] in_, byte[] out_, int length, IntPtr schedule, byte[] ivec, ref int num, int enc);

        [StructLayout(LayoutKind.Sequential)]
        struct cast_key_st
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public UInt32[] data;
            [MarshalAs(UnmanagedType.I4)]
            public int short_key;
        }

        private BlowfishAlgorithm _algorithm;
        private IntPtr _key;
        private byte[] _encryptIv;
        private byte[] _decryptIv;
        private int _encryptNum;
        private int _decryptNum;

        public Blowfish(BlowfishAlgorithm algorithm)
        {
            _algorithm = algorithm;
            _encryptIv = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            _decryptIv = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            cast_key_st key = new cast_key_st();
            key.data = new UInt32[32];
            key.short_key = new int();
            _key = Marshal.AllocHGlobal(key.data.Length * sizeof(UInt32) + sizeof(int));
            Marshal.StructureToPtr(key, _key, false);
            _encryptNum = 0;
            _decryptNum = 0;
        }
        public void SetKey(byte[] data)
        {
            _encryptNum = 0;
            _decryptNum = 0;
            CAST_set_key(_key, data.Length, data);
        }
        public byte[] Encrypt(byte[] buffer)
        {
            byte[] ret = new byte[buffer.Length];
            switch (_algorithm)
            {
                case BlowfishAlgorithm.CFB64:
                    CAST_cfb64_encrypt(buffer, ret, buffer.Length, _key, _encryptIv, ref _encryptNum, 1);
                    break;
            }
            return ret;
        }
        public byte[] Decrypt(byte[] buffer)
        {
            byte[] ret = new byte[buffer.Length];
            switch (_algorithm)
            {
                case BlowfishAlgorithm.CFB64:
                    CAST_cfb64_encrypt(buffer, ret, buffer.Length, _key, _decryptIv, ref _decryptNum, 0);
                    break;
            }
            return ret;
        }
        public byte[] EncryptIV
        {
            get { return _encryptIv; }
            set { System.Buffer.BlockCopy(value, 0, _encryptIv, 0, 8); }
        }
        public byte[] DecryptIV
        {
            get { return _decryptIv; }
            set { System.Buffer.BlockCopy(value, 0, _decryptIv, 0, 8); }
        }
        public void Dispose()
        {
            Marshal.FreeHGlobal(_key);
        }
    }
}
