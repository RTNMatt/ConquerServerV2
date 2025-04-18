using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void DepositWarehouseMoney(GameClient Client, ItemUsuagePacket* lpPacket)
        {
            if (Client.Money >= lpPacket->dwParam1)
            {
                Warehouse wh = new Warehouse(Client.Account, Client.ActiveWarehouseID);
                long Money = wh.ReadGold();
                if (Money + lpPacket->dwParam1 > int.MaxValue)
                    return;
                Money += lpPacket->dwParam1;
                wh.UpdateGold((int)Money);
                Client.Money -= (int)lpPacket->dwParam1;

                lpPacket->ID = ItemUsuageID.ShowWarehouseMoney;
                lpPacket->dwParam1 = (uint)Money;
                Client.Send(lpPacket);

                UpdatePacket Update = UpdatePacket.Create();
                Update.UID = Client.Entity.UID;
                Update.ID = UpdateID.Money;
                Update.Value = (uint)Client.Money;
                Client.Send(&Update);
            }
            else
            {
                Client.Send(MessageConst.WAREHOUSE_MONEY_FULL);
            }
        }
    }
}