using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Core
{
    public class RandomGenerator
    {
        const double REAL_UNIT_INT = 1.0 / ((double)int.MaxValue + 1.0);
        const double REAL_UNIT_UINT = 1.0 / ((double)uint.MaxValue + 1.0);
        const uint Y = 842502087, Z = 3579807591, W = 273326509;

        private uint x, y, z, w;

        public RandomGenerator()
        {
            Reinitialise((int)TIME.Now.Time);
        }
        public RandomGenerator(int seed)
        {
            Reinitialise(seed);
        }
        public void Reinitialise(int seed)
        {
            x = (uint)seed;
            y = Y;
            z = Z;
            w = W;
        }
        public int Next()
        {
            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;
            w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));

            uint rtn = w & 0x7FFFFFFF;
            if (rtn == 0x7FFFFFFF)
                return Next();
            return (int)rtn;
        }
        public int Next(int upperBound)
        {
            if (upperBound < 0)
                throw new ArgumentOutOfRangeException("upperBound", upperBound, "upperBound must be >=0");

            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;
            return (int)((REAL_UNIT_INT * (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))))) * upperBound);
        }
        public int Next(int lowerBound, int upperBound)
        {
            if (lowerBound > upperBound)
                throw new ArgumentOutOfRangeException(string.Format("upperBound: {0}, lowerBound: {1}", upperBound, lowerBound), (Exception)null);
            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;
            int range = upperBound - lowerBound;
            if (range < 0)
            {
                return lowerBound + (int)((REAL_UNIT_UINT * (double)(w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))) * (double)((long)upperBound - (long)lowerBound));
            }
            return lowerBound + (int)((REAL_UNIT_INT * (double)(int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))))) * (double)range);
        }
        public double NextDouble()
        {
            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;
            return (REAL_UNIT_INT * (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))));
        }
        public unsafe void NextBytes(byte[] buffer)
        {
            if (buffer.Length % 8 != 0)
                throw new ArgumentException("Buffer length must be divisible by 8", "buffer");

            uint x = this.x, y = this.y, z = this.z, w = this.w;

            fixed (byte* pByte0 = buffer)
            {
                uint* pDWord = (uint*)pByte0;
                for (int i = 0, len = buffer.Length >> 2; i < len; i += 2)
                {
                    uint t = (x ^ (x << 11));
                    x = y; y = z; z = w;
                    pDWord[i] = w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));

                    t = (x ^ (x << 11));
                    x = y; y = z; z = w;
                    pDWord[i + 1] = w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));
                }
            }

            this.x = x; this.y = y; this.z = z; this.w = w;
        }
        public uint NextUInt()
        {
            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;
            return (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)));
        }
        public int NextInt()
        {
            uint t = (x ^ (x << 11));
            x = y; y = z; z = w;
            return (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))));
        }
    }
}
