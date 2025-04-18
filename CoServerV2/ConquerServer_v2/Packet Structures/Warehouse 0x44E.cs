using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Packet_Structures
{
    // Enum representing the types of warehouse actions a client can request
    public enum WarehouseActionID : uint
    {
        Show = 0xA00,           // Request to view warehouse contents
        DepositItem = 0xA01,    // Request to deposit an item into the warehouse
        WithdrawItem = 0xA02    // Request to withdraw an item from the warehouse
    }

    // Represents the structure of the Warehouse Packet (0x44E) sent from the server to the client
    [StructLayout(LayoutKind.Sequential)]
    /// <summary>
    /// 0x44E (Server->Client)
    /// </summary>
    public unsafe struct WarehousePacket
    {
        public ushort Size;                 // Total packet size (excluding protocol header)
        public ushort Type;                 // Packet type (should be 0x44E for warehouse)
        public uint NpcID;                  // ID of the NPC being interacted with (e.g., warehouse NPC)
        public WarehouseActionID Action;    // Action being requested (view, deposit, withdraw)
        public int Count;                   // Item count or used as ItemUID based on context
        // Helper property to alias Count as ItemUID for certain operations
        public uint ItemUID { get { return (uint)Count; } set { Count = (int)value; } }
        public byte ItemStart;  // Marks the start of item data in the packet

        public static SafePointer Create(int Count)
        {
            // Calculate packet size: header (16 bytes) + item data + extra 8 bytes (likely for alignment)
            int Size = 16 + (sizeof(WarehouseItem) * Count) + 8;
            SafePointer SafePtr = new SafePointer(Size);
            // Initialize the packet structure
            WarehousePacket* ptr = (WarehousePacket*)SafePtr.Addr;
            ptr->Size = (ushort)(Size - 8); // Size without the protocol header
            ptr->Type = 0x44E;
            ptr->Count = Count;
            // Append server-specific TQ header
            PacketBuilder.AppendTQServer(SafePtr.Addr, Size);
            return SafePtr;
        }
    }
    // Represents a single item in the warehouse
    [StructLayout(LayoutKind.Explicit, Size=24)]
    /// <summary>
    /// An internal structure of the 0x44E packet (Server->Client)
    /// </summary>
    public unsafe struct WarehouseItem
    {
        [FieldOffset(0)]
        public uint UID;            // Unique identifier of the item
        [FieldOffset(4)]
        public uint ID;             // Item type ID
        [FieldOffset(9)]
        public byte SocketOne;      // First socket type
        [FieldOffset(10)]
        public byte SocketTwo;      // Second socket type
        [FieldOffset(11)]
        public ushort wUnknown;     // Unknown field (used for padding or unused data)
        [FieldOffset(13)]
        public byte Plus;           // +Level of the item
        [FieldOffset(14)]
        public byte Bless;          // Blessing level
        [FieldOffset(15)]
        public bool Free;           // Whether the item is bound/free
        [FieldOffset(16)]
        public byte Enchant;        // Enchantment level
        [FieldOffset(17)]           
        public fixed byte bUnknowns[5]; // Unknown/padding bytes
        [FieldOffset(23)]
        public byte Color;          // Item color (e.g., for quality)

        // Converts the warehouse item to a general Item structure.
        //An Item instance populated with warehouse item data.
        public Item ToItem()
        {
            Item retn = new Item();
            retn.ID = ID;
            retn.SocketOne = SocketOne;
            retn.SocketTwo = SocketTwo;
            retn.Plus = Plus;
            retn.Bless = Bless;
            retn.Enchant = Enchant;
            retn.Color = (byte)Color;
            retn.Free = Free;
            return retn;
        }
        // Creates a WarehouseItem structure based on an existing Item instance.
        //A WarehouseItem populated with data from the base Item.
        public static WarehouseItem Create(Item Base)
        {
            WarehouseItem retn = new WarehouseItem();
            retn.UID = Base.UID;
            retn.ID = Base.ID;
            retn.SocketOne = Base.SocketOne;
            retn.SocketTwo = Base.SocketTwo;
            retn.Plus = Base.Plus;
            retn.Bless = Base.Bless;
            retn.Enchant = Base.Enchant;
            retn.Color = Base.Color;
            retn.Free = Base.Free;
            return retn;
        }
    }
}
