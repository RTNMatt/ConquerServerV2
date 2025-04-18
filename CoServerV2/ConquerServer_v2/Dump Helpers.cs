using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ConquerServer_v2
{
    public partial class Program
    {
        static string StrHexToAnsi(string StrHex)
        {
            string[] Data = StrHex.Split(' ');
            string Ansi = "";
            foreach (string tmpHex in Data)
            {
                if (tmpHex != "")
                {
                    byte ByteData = byte.Parse(tmpHex, NumberStyles.HexNumber);
                    Ansi += (ByteData >= 32 && ByteData <= 126) ? ((char)ByteData).ToString() : ".";
                }
            }
            return Ansi;
        }
        public static byte[] GetBytes(string SpacingHex)
        {
            string[] Delimeter = SpacingHex.Split(' ');
            byte[] bytes = new byte[Delimeter.Length];
            for (int i = 0; i < Delimeter.Length; i++)
                bytes[i] = byte.Parse(Delimeter[i], NumberStyles.HexNumber);
            return bytes;
        }
        public static string Dump(byte[] Bytes)
        {
            string Hex = "";
            foreach (byte b in Bytes)
                Hex += b.ToString("X2") + " ";
            string Out = "";
            while (Hex.Length != 0)
            {
                int Remove;
                string SubString = Hex.Substring(0, (Hex.Length >= 48) ? 48 : Hex.Length);
                Remove = SubString.Length;
                SubString = SubString.PadRight(60, ' ') + StrHexToAnsi(SubString);
                Hex = Hex.Remove(0, Remove);
                Out += SubString + "\r\n";
            }
            return Out;
        }
    }
}
