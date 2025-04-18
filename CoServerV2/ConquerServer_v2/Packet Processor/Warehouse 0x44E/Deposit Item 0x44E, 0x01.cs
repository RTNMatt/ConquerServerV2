using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void DepositWarehouseItem(GameClient Client, WarehousePacket* Packet)
        {
            byte itemslot;
            Item item = Client.Inventory.Search(Packet->ItemUID, out itemslot);
            if (item != null)
            {
                int Count;
                Warehouse warehouse = new Warehouse(Client.Account, Client.ActiveWarehouseID);
                BinaryFile bf = warehouse.ReadAllStart(&Count);
                DatabaseWHItem* items = stackalloc DatabaseWHItem[Count + 1];
                warehouse.ReadAllEnd(bf, Count, items);
                items[Count] = new DatabaseWHItem(item);
                Client.Inventory.RemoveBySlot(itemslot);

                Count++;
                warehouse.UpdateItems(items, Count);
                SafePointer ptr = WarehousePacket.Create(Count);
                Packet = (WarehousePacket*)ptr.Addr;
                Packet->Action = WarehouseActionID.Show;

                WarehouseItem* start = (WarehouseItem*)&Packet->ItemStart;
                for (int i = 0; i < Count; i++)
                {
                    start[i] = items[i].Item;
                }
                
                Client.Send(ptr.Addr);
                ptr.Free();
            }
        }
    }
}