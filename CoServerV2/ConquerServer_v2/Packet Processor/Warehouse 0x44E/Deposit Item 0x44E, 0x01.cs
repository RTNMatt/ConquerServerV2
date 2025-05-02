using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        // Handles depositing an item from the player's inventory into their warehouse
        public static void DepositWarehouseItem(GameClient Client, WarehousePacket* Packet)
        {
            Console.WriteLine($"[DepositWarehouseItem] Trying to deposit item UID: {Packet->ItemUID}");

            // Try to find the item in the player's inventory
            byte itemslot;
            Item item = Client.Inventory.Search(Packet->ItemUID, out itemslot);
            if (item != null)
            {
                Console.WriteLine($"[DepositWarehouseItem] Found item in inventory: {item.UID}, slot: {itemslot}");

                int Count;
                // Initialize warehouse instance for this client
                Warehouse warehouse = new Warehouse(Client.Account, Client.ActiveWarehouseID);



                // Begin reading the existing warehouse items
                BinaryFile bf = warehouse.ReadAllStart(&Count);
                // Allocate memory for the existing items + 1 (the new item to deposit)
                DatabaseWHItem* items = stackalloc DatabaseWHItem[Count + 1];
                // Finish reading warehouse items into memory
                warehouse.ReadAllEnd(bf, Count, items);
                // Add the new item at the end of the items list
                items[Count] = new DatabaseWHItem(item);
                // Remove the item from the player's inventory
                Client.Inventory.RemoveBySlot(itemslot);

                // Increase the item count by 1
                Count++;
                // Save the updated items list back to the warehouse file
                warehouse.UpdateItems(items, Count);
                // Create a warehouse packet to send the updated item list back to the client
                SafePointer ptr = WarehousePacket.Create(Count);
                Packet = (WarehousePacket*)ptr.Addr;
                Packet->Action = WarehouseActionID.Show;

                // Copy all warehouse items into the packet
                WarehouseItem* start = (WarehouseItem*)&Packet->ItemStart;
                for (int i = 0; i < Count; i++)
                {
                    start[i] = items[i].Item;
                }

                Console.WriteLine($"[DepositWarehouseItem] Sending updated warehouse with {Count} items.");

                // Send the updated warehouse packet to the client
                Client.Send(ptr.Addr);

                // Free the allocated memory
                ptr.Free();
            }
            else // If item was not found in inventory
            {
                Console.WriteLine($"[DepositWarehouseItem] Item not found in inventory: {Packet->ItemUID}");
            }
        }
    }
}