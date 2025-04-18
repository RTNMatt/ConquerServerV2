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
        x=y=z=w=0;
		Reseed(X, Y, Z, W);
	}
	public void Reseed (uint X, uint Y, uint Z, uint W)
	{
		x = X;
		y = Y;
		z = Z;
		w = W;
	}
	public uint GenerateKey ()
	{
		uint t = (x ^ (x << 11));
        x = y; y = z; z = w;
        w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));
		return w;
	}
}
//
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
	public void BigEncrypt (byte* block, byte* out_block, int block_size)
	{
		uint block_checksum = 0;
		for (int i = 0; i < block_size; i++)
		{
			block_checksum = (block_checksum << 4) ^ block[i];
            out_block[i] = (byte)(block[i] ^ key_bytes[pos++]);
			if (pos == key_capacity)
			{
				BigReseed (block_checksum);
			}
		}
	}
	public void BigDecrypt (byte* block, byte* out_block, int block_size)
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
    /// <summary>
    /// Used for decrypting packets sent accross the auth server, and encrypting them.
    /// </summary>
    public unsafe class AuthCrypto
    {
        private HybridBigKey_t enc;
        private HybridBigKey_t dec;
        public AuthCrypto()
        {
            enc = new HybridBigKey_t(0x69a7a17);
            dec = new HybridBigKey_t(0x69a7a17);
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
        private HybridBigKey_t key_EncryptSpellID;
        private HybridBigKey_t key_EncryptSpellUID;
        private HybridBigKey_t key_EncryptCoordinate;

        public SpellCrypto()
        {
            key_EncryptSpellID = new HybridBigKey_t(0x3f2975);
            key_EncryptSpellUID = new HybridBigKey_t(0x912873);
            key_EncryptCoordinate = new HybridBigKey_t(0xfeca7a);
        }
        public unsafe void Decrypt(ref uint UID, ref ushort SpellID, ref ushort X, ref ushort Y)
        {
            fixed (uint* pUID = &UID)
            {
                fixed (ushort* pSpellID = &SpellID, pX = &X, pY = &Y)
                {
                    key_EncryptSpellUID.BigDecrypt((byte*)pUID, (byte*)pUID, sizeof(uint));
                    key_EncryptSpellID.BigDecrypt((byte*)pSpellID, (byte*)pSpellID, sizeof(ushort));
                    key_EncryptCoordinate.BigDecrypt((byte*)pX, (byte*)pX, sizeof(ushort));
                    key_EncryptCoordinate.BigDecrypt((byte*)pY, (byte*)pY, sizeof(ushort));
                }
            }
        }
    }
}
