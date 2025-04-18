using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void WithdrawWarehouseMoney(GameClient Client, ItemUsuagePacket* lpPacket)
        {
            Warehouse wh = new Warehouse(Client.Account, Client.ActiveWarehouseID);
            int storedMoney = (int)wh.ReadGold();

            if (lpPacket->dwParam1 <= storedMoney)
            {
                storedMoney -= (int)lpPacket->dwParam1;
                wh.UpdateGold(storedMoney);
                Client.Money += (int)lpPacket->dwParam1;

                lpPacket->ID = ItemUsuageID.ShowWarehouseMoney;
                lpPacket->dwParam1 = (uint)storedMoney;
                Client.Send(lpPacket);

                UpdatePacket update = UpdatePacket.Create();
                update.UID = Client.Entity.UID;
                update.ID = UpdateID.Money;
                update.Value = (uint)Client.Money;
                Client.Send(&update);
            }
            else
            {
                Client.Send(MessageConst.NOT_ENOUGH_WAREHOUSE_MONEY);
            }
        }
    }
}
