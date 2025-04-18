using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        private static int[] ComposeTable = { 20, 20, 80, 240, 720, 2160, 6480, 19440, 58320, 2700, 5500, 9000 };
        private static int[] ComposeTableAdd = { 0, 10, 40, 120, 360, 1080, 3240, 9720, 29160 };
        public static void ComposeItems(GameClient Client, ComposeItemPacket* Packet)
        {
            byte minorslot;
            Item main = Client.Inventory.Search(Packet->MainItem);
            Item minor = Client.Inventory.Search(Packet->MinorItem, out minorslot);
            if (main != null && minor != null)
            {
                if (main.Plus < Item.MaxPlus)
                {
                    int needed = ComposeTable[main.Plus];
                    int plus = minor.Plus;
                    main.ComposeProgress += ComposeTableAdd[plus];
                    while (main.ComposeProgress >= needed)
                    {
                        main.ComposeProgress -= needed;
                        main.Plus += 1;
                        if (main.Plus >= Item.MaxPlus)
                            break;
                        needed = ComposeTable[main.Plus];
                    }
                    main.SendInventoryUpdate(Client);
                    Client.Inventory.RemoveBySlot(minorslot);
                }
            }
        }
    }
}