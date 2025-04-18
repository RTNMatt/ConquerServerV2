using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Core;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;
using ConquerServer_v2.Monster_AI;

namespace ConquerServer_v2.Client
{
    public class PlayerMiner
    {
        public GameClient Client;
        private MineField Field;
        private TIME timeCanMine;
        private int mineCount;

        public bool CanMine
        {
            get { return TIME.Now.Time >= timeCanMine.Time; }
        }
        public void Stop()
        {
            Client.Mine = null;
            Client.ServerFlags &= ~ServerFlags.Mining;
        }
        public bool Start(GameClient Owner)
        {
            mineCount = 0;
            Client = Owner;
            Field = new MineField(Owner.Entity.MapID);
            Client.ServerFlags |= ServerFlags.Mining;
            return Field.ValidField;
        }
        public unsafe void SwingPickaxe()
        {
            Item right = Client.Equipment[ItemPosition.Right];
            if (right == null)
            {
                Stop();
                return;
            }
            if (!right.IsItemType(ItemTypeConst.PickaxeID))
            {
                Stop();
                return;
            }

            int nRand = Kernel.Random.Next(10);
            foreach (MineField.Ore Ore in Field.Ores)
            {
                if (Ore.ID == 0)
                    continue;
                uint idOreType = Ore.GetRandom();
                int nChance = 10 - (int)((Ore.ID % 100) / 10);
                if (nRand < nChance)
                {
                    Item award = new Item();
                    award.ID = idOreType;
                    if (Client.Inventory.Add(award) != InventoryErrNo.SUCCESS)
                    {
                        // some tq bullshit about dropping the item?
                    }
                    break;
                }
            }

            foreach (uint nGemID in Field.FieldGems)
            {
                if (nGemID == 0)
                    continue;

                int Slot = (int)((nGemID - 700000) / 10);
                for (byte j = 0; j < MineField.Gems[Slot].Length; j++)
                {
                    if (MineField.Gems[Slot][j].Rate)
                    {
                        Item award = new Item();
                        award.ID = MineField.Gems[Slot][j].ID;
                        if (Client.Inventory.Add(award) != InventoryErrNo.SUCCESS)
                        {
                            //Client.Send(new MessagePacket("You just got fucked, you could've mined a gem, but your inventory was full.", 0x00FF0000, ChatID.TopLeft));
                        }
                        break;
                    }
                }
            }

            mineCount++;
            timeCanMine = TIME.Now.AddSeconds(3);

            DataPacket showmine = DataPacket.Create();
            showmine.ID = DataID.Mining;
            showmine.UID = Client.Entity.UID;
            SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(&showmine), null);
        }
    }
}
