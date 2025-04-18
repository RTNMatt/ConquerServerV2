using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public enum BadPacketError
    {
        InvalidSize,
        NoTQClient
    }

    public class BadPacketException : Exception
    {
        private byte[] m_Packet;
        private ushort m_Type;
        private int m_SubID;

        public int SubID { get { return m_SubID; } }
        public ushort Type { get { return m_Type; } }
        public byte[] Packet { get { return m_Packet; } }
        
        private static string GenerateMsg(byte[] packet, ushort type, int subid, BadPacketError error)
        {
            return string.Format("A bad packet (base due to {0}) has been received with the type of {1}, and sub-id of {2}", error, type, subid);
        }

        public BadPacketException(byte[] Packet, ushort Type, int SubID, BadPacketError Error) :
            base(GenerateMsg(Packet, Type, SubID, Error))
        {
            this.m_Packet = Packet;
            this.m_Type = Type;
            this.m_SubID = SubID;
        }
    }
}
