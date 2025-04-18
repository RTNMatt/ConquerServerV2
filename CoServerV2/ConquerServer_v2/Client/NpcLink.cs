using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;
using ConquerServer_v2.Core;
using ConquerScriptLinker;

namespace ConquerServer_v2.Client
{
    public unsafe class ClientNpcLink : INpcPlayer
    {
        private GameClient Client;
        private Dictionary<string, object> Session;
        public ClientNpcLink(GameClient owner)
        {
            Client = owner;
            Session = new Dictionary<string, object>();
        }

        public void Send(void* Packet) { Client.Send(Packet); }
        public void Send(byte[] Packet) { Client.Send(Packet); }

        public GameClient Owner { get { return Client; } }

        public INpcItem GetEquipment(ushort Slot)
        {
            return Client.Equipment[(ItemPosition)Slot];
        }
        public void SetEquipment(ushort Slot, INpcItem Item)
        {
            Client.Equipment[(ItemPosition)Slot] = Item as Item;
        }
        public INpcSkill GetSpell(ushort ID)
        {
            ISkill find;
            Client.Spells.GetSkill(ID, out find);
            return find as INpcSkill;
        }
        public INpcSkill GetProficiency(ushort ID)
        {
            ISkill find;
            Client.Spells.GetSkill(ID, out find);
            return find as INpcSkill;
        }

        public string Account { get { return Client.Account; } }
        public string Name { get { return Client.Entity.Name; } }

        public uint UID { get { return Client.Entity.UID; } }
        public byte Job { get { return Client.Job; } set { Client.Job = value; SendUpdate(UpdateID.Job, value); } }
        public ushort Hairstyle { get { return Client.Entity.Hairstyle; } set { Client.Entity.Hairstyle = value; SendUpdate(UpdateID.Hairstyle, value); } }
        public ushort Reborn { get { return Client.Entity.Reborn; } }
        public int WarehousePassword
        {
            get
            {
                Warehouse wh = new Warehouse(Client.Account, Client.ActiveWarehouseID);
                return wh.ReadPassword();
            }
            set
            {
                Warehouse wh = new Warehouse(Client.Account, Client.ActiveWarehouseID);
                wh.UpdatePassword(value);
            }
        }
        public uint ServerFlags { get { return (uint)Client.ServerFlags; } set { Client.ServerFlags = (ServerFlags)value; } }
        public ushort Level { get { return Client.Entity.Level; } }

        public string SpouseAccount { get { return Client.SpouseAccount; } set { Client.SpouseAccount = value; } }
        public string Spouse { get { return Client.Spouse; } set { Client.Spouse = value; } }

        public uint GuildWarTime { get { return Client.TimeStamps.GuildWarTime.Time; } }
        public ushort GuildID { get { return Client.Guild.ID; } }
        public byte GuildRank { get { return (byte)Client.Guild.Rank; } }

        public int InventorySpace { get { return 40 - Client.Inventory.ItemCount; } }
        public int CountItems(uint ItemId) { return Client.Inventory.CountItem(ItemId); }
        public int RemoveItems(uint ItemId, byte Count)
        {
            int removed = 0;
            for (byte i = 0; i < Client.Inventory.MaxPossibleItems; i++)
            {
                if (Count == 0)
                    break;
                if (Client.Inventory[i] != null)
                {
                    if (Client.Inventory[i].ID == ItemId)
                    {
                        Client.Inventory.RemoveBySlot(i);
                        Count--;
                        removed++;
                    }
                }
            }
            return removed;
        }
        public void GiveItem(INpcItem Item)
        {
            Client.Inventory.Add(Item as Item);
        }

        public int HP
        {
            get { return Client.Entity.Hitpoints; }
            set { Client.Entity.Hitpoints = value; SendUpdate(UpdateID.Hitpoints, (uint)value); }
        }
        public int MP
        {
            get { return Client.Manapoints; }
            set { Client.Manapoints = value; SendUpdate(UpdateID.Mana, (uint)value); }
        }
        public int MaxHP { get { return Client.Entity.MaxHitpoints; } }
        public int MaxMP { get { return Client.MaxManapoints; } }

        public int ConquerPoints
        {
            get { return Client.ConquerPoints; }
            set { Client.ConquerPoints = value; SendUpdate(UpdateID.ConquerPoints, (uint)value); }
        }
        public int Money
        {
            get { return Client.Money; }
            set { Client.Money = value; SendUpdate(UpdateID.Money, (uint)value); }
        }

        public ushort LastMapID { get { return Client.LastMapID; } }
        public ushort LastX { get { return Client.LastX; } }
        public ushort LastY { get { return Client.LastY; } }

        public ushort StatPoints
        {
            get { return Client.Stats.StatPoints; }
            set { Client.Stats.StatPoints = value; SendUpdate(UpdateID.StatPoints, value); }
        }
        public ushort Strength
        {
            get { return Client.Stats.Strength; }
            set { Client.Stats.Strength = value; SendUpdate(UpdateID.Strength, value); }
        }
        public ushort Agility
        {
            get { return Client.Stats.Agility; }
            set { Client.Stats.Agility = value; SendUpdate(UpdateID.Agility, value); }
        }
        public ushort Spirit
        {
            get { return Client.Stats.Spirit; }
            set { Client.Stats.Spirit = value; SendUpdate(UpdateID.Spirit, value); }
        }
        public ushort Vitality
        {
            get { return Client.Stats.StatPoints; }
            set { Client.Stats.StatPoints = value; SendUpdate(UpdateID.StatPoints, value); }
        }

        public void AddSession(string Key, object Value)
        {
            if (Session.ContainsKey(Key))
            {
                Session[Key] = Value;
            }
            else
            {
                Session.Add(Key, Value);
            }
        }
        public object PopSession(string Key)
        {
            object Value;
            if (Session.TryGetValue(Key, out Value))
                Session.Remove(Key);
            return Value;
        }
        public bool VarExists(string Key)
        {
            return Session.ContainsKey(Key);
        }
        public void ClearSession()
        {
            Session.Clear();
        }

        public void SendUpdate(uint id, uint value)
        {
            SendUpdate((UpdateID)id, value);
        }
        public void SendUpdate(UpdateID id, uint value)
        {
            UpdatePacket update = UpdatePacket.Create();
            update.UID = Client.Entity.UID;
            update.ID = id;
            update.Value = value;
            Client.Send(&update);
        }
        public void SendData(ushort DataId, uint dwValue, ushort wValue1, ushort wValue2)
        {
            DataPacket data = DataPacket.Create();
            data.ID = (DataID)DataId;
            data.UID = Client.Entity.UID;
            data.dwParam1 = dwValue;
            data.wParam1 = wValue1;
            data.wParam2 = wValue2;
            Client.Send(&data);
        }
        public void SendString(uint id, params string[] args)
        {
            StringPacket packet = new StringPacket();
            packet.ID = (StringID)id;
            packet.UID = Client.Entity.UID;
            packet.Strings = args;
            packet.StringsLength = 0;
            foreach (string s in args)
                packet.StringsLength += (byte)s.Length;
            Client.Send(packet);
        }
        public void Respawn()
        {
            Client.Respawn();
        }
        public void RecalculateStats()
        {
            Client.CalculateBonus();
            Client.CalculateAttack();
        }

        public void WriteDatabase(string Key, string Value)
        {
            IniFile ini = new IniFile(ServerDatabase.Path + @"\Accounts\" + Client.Account + ".ini");
            ini.WriteString("Character", Key, Value);
        }
        public string ReadDatabase(string Key, string Default, int MaxTextLength)
        {
            IniFile ini = new IniFile(ServerDatabase.Path + @"\Accounts\" + Client.Account + ".ini");
            return ini.ReadString("Character", Key, Default, MaxTextLength);
        }
    }
}
