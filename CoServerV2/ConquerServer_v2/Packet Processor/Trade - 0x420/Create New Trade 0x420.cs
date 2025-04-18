using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void NewTrade(GameClient Client, TradePacket* Packet)
        {
            if (Client.Trade == null)
                Client.Trade = new ClientTradeSession(Client);
            GameClient Partner = Client.Trade.Partner;
            if (Partner == null)
            {
                Partner = Kernel.FindClientByUID(Packet->dwParam);
                if (Partner != null)
                {
                    if (!Partner.InTrade)
                    {
                        Partner.Trade = new ClientTradeSession(Partner);
                        Packet->dwParam = Client.Entity.UID;
                        Partner.Trade.Partner = Client;
                        Partner.Send(Packet);
                    }
                    else
                    {
                        Client.Send(MessageConst.PLAYER_IN_TRADE);
                    }
                }
            }
            else
            {
                if (Partner.Entity.UID == Packet->dwParam && Partner.Trade != null)
                {
                    Partner.Trade.Partner = Client;
                    Partner.Trade.WindowOpen = true;
                    Client.Trade.WindowOpen = true;
                    Partner.Trade.Confirmed = false;
                    Client.Trade.Confirmed = false;

                    // --- Bypass Trading Partner Protection ---
                    /*TradePartnerPacket partner = TradePartnerPacket.Create();
                    partner.ID = TradePartnerID.AddTradePartner;
                    partner.Serialize(Partner);
                    Client.Send(&partner, partner.Size);
                    partner.Serialize(Client);
                    Partner.Send(&partner, partner.Size);*/
                    // ------

                    Packet->ID = TradeID.ShowTradeWindow;
                    Client.Send(Packet);
                    Packet->dwParam = Client.Entity.UID;
                    Partner.Send(Packet);
                }
                else
                {
                    Client.Send(MessageConst.PLAYER_IN_TRADE);
                }
            }
        }
    }
}