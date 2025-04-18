using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void ShowWarehouseItems(GameClient Client)
        {
            int Count;
            Warehouse warehouse = new Warehouse(Client.Account, Client.ActiveWarehouseID);
            BinaryFile bf = warehouse.ReadAllStart(&Count);
            SafePointer sf = WarehousePacket.Create(Count);

            WarehouseItem* start = (WarehouseItem*)&((WarehousePacket*)sf.Addr)->ItemStart;
            DatabaseWHItem* items = stackalloc DatabaseWHItem[Count];
            warehouse.ReadAllEnd(bf, Count, items);
            for (int i = 0; i < Count; i++)
            {
                start[i] = items[i].Item;
            }

            Client.Send(sf.Addr);
            sf.Free();
        }
    }
}