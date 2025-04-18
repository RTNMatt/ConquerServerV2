using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerScriptLinker
{
    public unsafe interface INpcPlayer
    {
        string Account { get; }
        string Name { get; }
        ushort Hairstyle { get; set; }
        uint UID { get; }
        byte Job { get; set; }
        ushort Reborn { get; }
        int WarehousePassword { get; set; }
        uint ServerFlags { get; set; }
        ushort Level { get; }

        string Spouse { get; set; }
        string SpouseAccount { get; set; }

        uint GuildWarTime { get; }
        ushort GuildID { get; }
        byte GuildRank { get; }

        INpcSkill GetSpell(ushort SpellID);
        INpcSkill GetProficiency(ushort ProfID);
        INpcItem GetEquipment(ushort Slot);
        void SetEquipment(ushort Slot, INpcItem Item);

        void GiveItem(INpcItem Item);
        int RemoveItems(uint ItemId, byte Count);
        int CountItems(uint ItemId);
        int InventorySpace { get; }

        int HP { get; set; }
        int MaxHP { get; }
        int MP { get; set;  }
        int MaxMP { get; }

        int Money { get; set; }
        int ConquerPoints { get; set; }

        ushort Strength { get; set; }
        ushort Vitality { get; set; }
        ushort Agility { get; set; }
        ushort Spirit { get; set; }
        ushort StatPoints { get; set; }

        ushort LastMapID { get; }
        ushort LastX { get; }
        ushort LastY { get; }

        void AddSession(string Key, object Value);
        object PopSession(string Key);
        bool VarExists(string Key);
        void ClearSession();

        void Send(byte[] Packet);
        void Send(void* Packet);
        void SendUpdate(uint UpdateId, uint Value);
        void SendData(ushort DataId, uint dwValue, ushort wValue1, ushort wValue2);
        void SendString(uint id, params string[] args);
        void Respawn();

        void WriteDatabase(string Key, string Value);
        string ReadDatabase(string Key, string Default, int MaxTextLength);
    }
}
