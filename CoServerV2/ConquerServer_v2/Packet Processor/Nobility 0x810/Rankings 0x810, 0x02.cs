using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void ShowNobilityRankings(GameClient Client, byte* Ptr)
        {
            const int max_count = 10;
            ushort page = Math.Max(NobilityRankPacket.GetCurrentPage(Ptr), (ushort)0);
            int position = Math.Max(page * max_count, 0);
            NobilityRank[] ranks = NobilityScoreBoard.QueryRanks();
            if (position < ranks.Length)
            {
                int count = Math.Min(max_count, (ranks.Length - position));
                NobilityRankPacket Packet = new NobilityRankPacket();
                Packet.Type = NobilityRankType.Listings;
                Packet.Ranks = new NobilityRank[count+1];
                Packet.CurrentPage = page;
                Packet.TotalPages = (ushort)(ranks.Length / max_count);
                if (ranks.Length % max_count != 0)
                    Packet.TotalPages++;
                Array.Copy(ranks, position, Packet.Ranks, 0, count);
                
                Client.Send(Packet);
            }
        }
    }
}