using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void DonateNobility(GameClient Client, byte* Ptr)
        {
#if !TOURNAMENT_NOBILITY
            bool OK = false;
            UpdatePacket Update = UpdatePacket.Create();
            Update.UID = Client.Entity.UID;
            int Amount = NobilityRankPacket.GetSignedValue(Ptr);

            if (NobilityRankPacket.PaidInConquerPoints(Ptr))
            {
                int Amount2 = NobilityScoreBoard.ToConquerPoints(Amount);
                if (OK = (Client.ConquerPoints >= Amount2))
                {
                    Client.ConquerPoints -= Amount2;
                    Update.ID = UpdateID.ConquerPoints;
                    Update.Value = (uint)Client.ConquerPoints;
                }
            }
            else
            {
                if (OK = (Client.Money >= Amount))
                {
                    Client.Money -= Amount;
                    Update.ID = UpdateID.Money;
                    Update.Value = (uint)Client.Money;
                }
            }

            if (OK)
            {
                Client.Send(&Update);
                NobilityScoreBoard.Donate(Client, Amount);
            }
#endif
        }
    }
}