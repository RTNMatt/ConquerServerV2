using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Database
{
    //Represents an item stored in warehouse (checks durability/stacking)
    public struct DatabaseWHItem
    {
        public WarehouseItem Item;
        public short Amount;
        public short MaxAmount;

        //Constructor for creating a new item
        public DatabaseWHItem(Item item)
        {
            Item = WarehouseItem.Create(item);
            Amount = item.Durability;
            MaxAmount = item.MaxDurability;
        }

        //Reaconstruct item from warehouse to an item
        public Item ToItem()
        {
            Item item = Item.ToItem();
            item.Durability = Amount;
            item.MaxDurability = MaxAmount;
            return item;
        }
    }

    //Manages reading and writing of warehouse data
    public unsafe class Warehouse
    {
        private static string WarehousePath; //path for all warehouses
        //initialize the static warehouse path
        public static void Init()
        {
            WarehousePath = ServerDatabase.Path + @"\Warehouse\"; 
        }
        private string File; //path to this players warhouse item file
        private string MainFile; //path to this players warehouse password/gold file
        private uint WarehouseID;

        //Constructor: builds file paths from account and warehouse ID
        public Warehouse(string Acccount, uint warehouseID)
        {
            File = WarehousePath + warehouseID.ToString() + "\\" + Acccount + ".bin";
            MainFile = WarehousePath + Acccount + ".bin";
            WarehouseID = warehouseID;
        }
        // Read the password (stored 4 bytes in)
        public int ReadPassword()
        {
            int pass = 0;
            BinaryFile data = new BinaryFile(MainFile, System.IO.FileMode.Open);
            if (data.Success)
            {
                data.Position = 4; // Skip first 4 bytes
                data.Read(&pass, sizeof(int));
                data.Close();
            }
            return pass;
        }
        // Update the password (stored 4 bytes in)
        public void UpdatePassword(int Value)
        {
            BinaryFile data = new BinaryFile(MainFile, System.IO.FileMode.Create);
            if (data.Success)
            {
                data.Position = 4;
                data.Read(&Value, sizeof(int));
                data.Close();
            }
        }
        // Read the stored gold amount (stored at beginning)
        public int ReadGold()
        {
            int amount = 0;
            BinaryFile data = new BinaryFile(MainFile, System.IO.FileMode.Open);
            if (data.Success)
            {
                data.Read(&amount, sizeof(int));
                data.Close();
            }
            return amount;
        }
        // Update the stored gold amount
        public void UpdateGold(int Amount)
        {
            BinaryFile data = new BinaryFile(MainFile, System.IO.FileMode.Create);
            if (data.Success)
            {
                data.Write(&Amount, sizeof(int));
                data.Close();
            }
        }
        // Begin reading warehouse items — returns BinaryFile instance and item count
        public BinaryFile ReadAllStart(int* ItemCount)
        {
            BinaryFile data = new BinaryFile(File, System.IO.FileMode.Open);
            if (data.Success)
            {
                data.Read(ItemCount, sizeof(int)); // Read the number of items
            }
            else
            {
                *ItemCount = 0; // File doesn't exist or failed
            }
            return data;
        }
        // Continue reading items into array
        public void ReadAllEnd(BinaryFile data, int ItemCount, DatabaseWHItem* Items)
        {
            if (data.Success)
            {
                for (int i = 0; i < ItemCount; i++)
                {
                    data.Read(Items, ItemCount, sizeof(DatabaseWHItem));
                }
                data.Close();
            }
        }
        // Save all items back to the warehouse file
        public void UpdateItems(DatabaseWHItem* Items, int ItemCount)
        {
            Console.WriteLine($"[Warehouse] Saving {ItemCount} items to {File}");
            BinaryFile data = new BinaryFile(File, System.IO.FileMode.Create);
            if (data.Success)
            {
                data.Write(&ItemCount, sizeof(int)); // Write item count
                data.Write(Items, ItemCount, sizeof(DatabaseWHItem)); // Write array of items
                data.Close();
            }
            else
            {
                Console.WriteLine($"[Warehouse] Failed to create warehouse file: {File}");
            }
        }
    }
}