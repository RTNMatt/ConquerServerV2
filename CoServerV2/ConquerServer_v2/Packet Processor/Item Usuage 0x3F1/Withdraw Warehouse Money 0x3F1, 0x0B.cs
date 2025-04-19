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
            // Create a Warehouse object for the player's active warehouse, read current money
            Warehouse wh = new Warehouse(Client.Account, Client.ActiveWarehouseID);
            long storedMoney = wh.ReadGold();
            //Check if the player has enough money in the warehouse to withdraw
            if (lpPacket->dwParam1 <= storedMoney)
            {
                // Create a Warehouse object for the player's active warehouse, read current money
                

                // Prevent over withdraw, this is a warehouse not a bank
                if (storedMoney - lpPacket->dwParam1 < 0)
                    return;
                // Subtract the withdrawn amount from the warehouse, save new wh value, update player client inventory
                storedMoney -= lpPacket->dwParam1;
                wh.UpdateGold((int)storedMoney);
                Client.Money += (int)lpPacket->dwParam1;

                // Update the warehouse money in the packet
                lpPacket->ID = ItemUsuageID.ShowWarehouseMoney;
                lpPacket->dwParam1 = (uint)storedMoney;
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
                
            }
        }
    }
}
