using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Core
{
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
}
