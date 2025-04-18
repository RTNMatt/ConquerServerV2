using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Core;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Client
{
    public unsafe class ClientVendor
    {
        private GameClient Owner;
        private bool m_IsVending;
        private NpcEntity m_VendorNpc;
        private DictionaryV2<uint, VendingItem> VendingItems;

        public uint ShopID { get { return m_VendorNpc.UID; } }
        public bool IsVending { get { return m_IsVending; } }
        public VendingItem[] Items { get { return VendingItems.EnumerableValues; } }

        public ClientVendor(GameClient Client)
        {
            Owner = Client;
        }
        public bool StartVending()
        {
            if (m_IsVending)
                return false;
            if (Owner.CurrentDMap != null)
            {
                DictionaryV2<uint, NpcEntity> npcs = Owner.CurrentDMap.Npcs;
                ushort vendx = (ushort)(Owner.Entity.X - 2);
                ushort vendy = Owner.Entity.Y;
                foreach (NpcEntity npc in npcs.EnumerableValues)
                {
                    if (npc.X == vendx && npc.Y == vendy && !npc.IsVendor)
                    {
                        m_VendorNpc = npc;
                        m_VendorNpc.ConvertToVendor(Owner.Entity.Name);
                        fixed (SpawnNpcPacket* spawn = &m_VendorNpc.Spawn)
                            SendRangePacket.Add(Owner.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(spawn), null);
                        VendingItems = new DictionaryV2<uint, VendingItem>();
                        m_IsVending = true;
                        break;
                    }
                }
            }
            return m_IsVending;
        }
        public void StopVending()
        {
            if (m_IsVending)
            {
                m_VendorNpc.ConvertToStandard();
                fixed (SpawnNpcPacket* spawn = &m_VendorNpc.Spawn)
                    SendRangePacket.Add(Owner.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(spawn), null);
                m_VendorNpc = null;
                VendingItems = null;
                m_IsVending = false;
            }
        }

        public void AddItem(Item item, int Cost, bool PurchaseWithGold)
        {
            VendingItem vItem = new VendingItem();
            vItem.FromItem(item, PurchaseWithGold);
            vItem.Price = Cost;
            vItem.ShopID = ShopID;
            VendingItems.Add(vItem.UID, vItem);
        }
        public VendingItem SelectItem(uint UID)
        {
            VendingItem result;
            VendingItems.TryGetValue(UID, out result);
            return result;
        }
        public void RemoveItem(uint UID)
        {
            VendingItems.Remove(UID);
        }

        public static GameClient FindVendorClient(uint shopID)
        {
            foreach (GameClient iClient in Kernel.Clients)
            {
                if (iClient != null)
                {
                    if (iClient.IsVendor)
                    {
                        if (iClient.Vendor.ShopID == shopID)
                        {
                            return iClient;
                        }
                    }
                }
            }
            return null;
        }
    }
}
