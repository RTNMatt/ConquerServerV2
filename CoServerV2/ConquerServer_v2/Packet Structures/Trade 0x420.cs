using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public enum TradeID : uint
    {
        RequestNewTrade = 0x01,
        RequestCloseTrade = 0x02,
        RequestAddItemToTrade = 0x06,
        RequestAddMoneyToTrade = 0x07,
        RequestAddConquerPointsToTrade = 0x0D,
        RequestCompleteTrade = 0x0A,
        ShowTradeWindow = 0x03,
        CloseTradeWindow = 0x05,
        DisplayMoney = 0x08,
        DisplayConquerPoints = 0x0C
    }

    public unsafe struct TradePacket
    {
        public ushort Size;
        public ushort Type;
        public uint dwParam;
        public TradeID ID;
        public fixed sbyte TQServer[8];

        public static TradePacket Create()
        {
            TradePacket retn = new TradePacket();
            retn.Size = 0x0C;
            retn.Type = 0x420;
            PacketBuilder.AppendTQServer((byte*)retn.TQServer, 8);
            return retn;
        }
    }
}