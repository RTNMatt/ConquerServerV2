using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public unsafe struct DateTimePacket
    {
        public ushort Size;
        public ushort Type;
        public int Header;
        public int Year;
        public int Month;
        public int DayOfYear;
        public int DayOfMonth;
        public int Hour;
        public int Minute;
        public int Seconds;
        public fixed byte TQServer[8];

        private void Format()
        {
            DateTime dNow = DateTime.Now;
            Year = dNow.Year - 1900;
            Month = dNow.Month - 1;
            DayOfMonth = dNow.Day;
            DayOfYear = dNow.DayOfYear;
            Hour = dNow.Hour;
            Minute = dNow.Minute;
            Seconds = dNow.Second;
        }
        public static DateTimePacket Create()
        {
            DateTimePacket retn = new DateTimePacket();
            retn.Size = 0x24;
            retn.Type = 0x409;
            retn.Format();
            PacketBuilder.AppendTQServer(retn.TQServer, 8);
            return retn;
        }
    }
}
