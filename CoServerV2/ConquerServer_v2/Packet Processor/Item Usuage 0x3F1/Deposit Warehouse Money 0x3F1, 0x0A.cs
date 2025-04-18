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
            // Check if the player has enough money to deposit
            if (Client.Money >= lpPacket->dwParam1)
            {
                // Create a Warehouse object for the player's active warehouse, read current money
                Warehouse wh = new Warehouse(Client.Account, Client.ActiveWarehouseID);
                long Money = wh.ReadGold();

                // Prevent integer overflow, ie. room in warehouse to store the gold
                if (Money + lpPacket->dwParam1 > int.MaxValue)
                    return;

                // Add the deposited amount to the warehouse, save new wh value, update player client inventory
                Money += lpPacket->dwParam1;
                wh.UpdateGold((int)Money);
                Client.Money -= (int)lpPacket->dwParam1;

                // Update the warehouse money in the packet
                lpPacket->ID = ItemUsuageID.ShowWarehouseMoney;
                lpPacket->dwParam1 = (uint)Money;
                Client.Send(lpPacket);

                // Send an update packet to show the player their new money total
                UpdatePacket Update = UpdatePacket.Create();
                Update.UID = Client.Entity.UID;
                Update.ID = UpdateID.Money;
                Update.Value = (uint)Client.Money;
                Client.Send(&Update);
            }
            else
            {
                // Come on? You don't have enough money to deposit? We'll blame a typo, all those k's look confusing
                Client.Send(MessageConst.WAREHOUSE_MONEY_FULL);
            }
        }
    }
}