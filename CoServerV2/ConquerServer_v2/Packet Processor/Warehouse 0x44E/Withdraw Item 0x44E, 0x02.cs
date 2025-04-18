using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void WithdrawWarehouseItem(GameClient Client, WarehousePacket* Packet)
        {
            int Count;
            Warehouse warehouse = new Warehouse(Client.Account, Client.ActiveWarehouseID);
            BinaryFile bf = warehouse.ReadAllStart(&Count);
            DatabaseWHItem* items = stackalloc DatabaseWHItem[Count];
            warehouse.ReadAllEnd(bf, Count, items);

            if (Client.Inventory.ItemCount < 40)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (items[i].Item.UID == Packet->ItemUID)
                    {
                        Client.Inventory.Add(items[i].ToItem());
                        MSVCRT.memcpy(items + i, items + i + 1, (Count - i) * sizeof(WarehouseItem));
                        Count--;
                        warehouse.UpdateItems(items, Count);
                        
                        SafePointer ptr = WarehousePacket.Create(Count);
                        WarehouseItem* start = (WarehouseItem*)&((WarehousePacket*)ptr.Addr)->ItemStart;
                        for (int i2 = 0; i2 < Count; i2++)
                        {
                            start[i2] = items[i2].Item;
                        }
                        Client.Send(ptr.Addr);
                        ptr.Free();
                    }
                }
            }
        }
    }
}