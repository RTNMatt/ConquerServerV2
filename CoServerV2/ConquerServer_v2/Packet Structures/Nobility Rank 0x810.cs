using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Structures
{
    public enum NobilityRankType : uint
    {
        Icon = 0x03,
        Listings = 0x02,
        Donate = 0x01
    }

    public struct NobilityRank : IScorable
    {
        public string Name;
        public uint UID;
        public int Gold;
        public NobilityID Rank;
        public int Listing;

        public int Score { get { return Gold; } set { Gold = value; } }

        public string ListingString
        {
            get
            {
                return UID.ToString() + " 0 0 " + Name + " " + Gold.ToString() + " " + ((uint)Rank).ToString() + " " + Listing.ToString();
            }
        }
        public string LocalString
        {
            get
            {
                return UID.ToString() + " " + Gold.ToString() + " " + ((uint)Rank).ToString() + " " + Listing.ToString();
            }
        }    
        public NobilityRank(CommonEntity Entity, int Gold, int Listing)
        {
            Name = Entity.Name;
            UID = Entity.UID;
            Rank = Entity.Nobility;
            this.Gold = Gold;
            this.Listing = Listing;
        }
        public NobilityRank(uint UID, string Name, NobilityID Rank, int Gold, int Listing)
        {
            this.Name = Name;
            this.UID = UID;
            this.Rank = Rank;
            this.Gold = Gold;
            this.Listing = Listing;
        }
    }

    public unsafe class NobilityRankPacket
    {
        public uint Value;
        public NobilityRank[] Ranks;
        public NobilityRankType Type;

        public NobilityRank SingleRank
        {
            get { return Ranks[0]; }
            set
            {
                Ranks = new NobilityRank[1];
                Ranks[0] = value;
            }
        }
        public ushort TotalPages
        {
            get { fixed (void* ptr = &Value) { return *(((ushort*)ptr) + 1); } }
            set { fixed (void* ptr = &Value) { *(((ushort*)ptr) + 1) = value; } }
        }
        public ushort CurrentPage
        {
            get { fixed (void* ptr = &Value) { return *(((ushort*)ptr)); } }
            set { fixed (void* ptr = &Value) { *(((ushort*)ptr)) = value; } }
        }

        public static NobilityRankType ID(byte* Ptr)
        {
            return *((NobilityRankType*)(Ptr + 4));
        }
        public static ushort GetCurrentPage(byte* Ptr)
        {
            return *((ushort*)(Ptr + 8));
        }
        public static int GetSignedValue(byte* Ptr)
        {
            return *((int*)(Ptr + 8));
        }
        public static bool PaidInConquerPoints(byte* Ptr)
        {
            return *((bool*)(Ptr + 16));
        }

        private static void FormatHead(byte* pData, int Length, NobilityRankType type)
        {
            *((ushort*)pData) = (ushort)Length;
            *((ushort*)(pData + 2)) = 0x810;
            *((NobilityRankType*)(pData + 4)) = type;
        }
        private static void FormatStrings(byte* pData, ushort StartOffset, string[] Strings)
        {
            pData[StartOffset] = (byte)Strings.Length;
            StartOffset++;
            for (int i = 0; i < Strings.Length; i++)
            {
                pData[StartOffset] = (byte)Strings[i].Length;
                Strings[i].CopyTo(pData + StartOffset + 1);
                StartOffset += (ushort)(Strings[i].Length + 1);
            }
        }
        public static implicit operator byte[](NobilityRankPacket packet)
        {
            int strings_length = 0;
            string[] strings = new string[packet.Ranks.Length];
            for (int i = 0; i < packet.Ranks.Length; i++)
            {
                if (packet.Type == NobilityRankType.Icon)
                    strings[i] = packet.Ranks[i].LocalString;
                else if (packet.Type == NobilityRankType.Listings)
                    strings[i] = packet.Ranks[i].ListingString;
                strings_length += strings[i].Length;
            }
            byte[] data = new byte[33 + strings_length + 8];
            fixed (byte* pData = data)
            {
                FormatHead(pData, data.Length - 8, packet.Type);
                *((uint*)(pData + 8)) = packet.Value;
                FormatStrings(pData, 28, strings);
                PacketBuilder.AppendTQServer(pData, data.Length);
            }
            return data;
        }
    }
}
