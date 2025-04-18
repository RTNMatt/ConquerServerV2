using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void CompleteTrade(GameClient Client, TradePacket* Packet)
        {
            if (Client.InTrade)
            {
                if (Client.Trade.Partner.InTrade)
                {
                    GameClient Partner = Client.Trade.Partner;
                    if (Partner.InTrade)
                    {
                        Client.Trade.Confirmed = true;
                        if (!Partner.Trade.Confirmed)
                        {
                            Partner.Send(Packet);
                        }
                        else
                        {
                            Packet->ID = TradeID.CloseTradeWindow;
                            Client.Send(Packet);
                            Partner.Send(Packet);

                            byte[] ClientItems;
                            byte[] PartnerItems;
                            if (Client.Trade.ValidateItems(out ClientItems, out PartnerItems))
                            {
                                if (Partner.NpcLink.InventorySpace >= ClientItems.Length &&
                                    Client.NpcLink.InventorySpace >= PartnerItems.Length)
                                {
                                    foreach (byte itemSlot in ClientItems)
                                    {
                                        Partner.Inventory.Add(Client.Inventory[itemSlot]);
                                        Client.Inventory.RemoveBySlot(itemSlot);
                                    }

                                    foreach (byte itemSlot in PartnerItems)
                                    {
                                        Client.Inventory.Add(Partner.Inventory[itemSlot]);
                                        Partner.Inventory.RemoveBySlot(itemSlot);
                                    }

                                    Client.Trade.ExchangeMoney();

                                    Partner.Trade = null;
                                    Client.Trade = null;
                                    return;
                                }
                            }
                            Client.Send(MessageConst.ERROR_IN_TRADE);
                        }
                    }
                }
            }
        }
    }
}