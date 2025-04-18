using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerScriptLinker;

namespace ConquerServer_v2.Database
{
    public struct LotteryItem
    {
        public byte Chance;
        public string PrizeName;
        public uint PrizeID;
        public byte BoxColor;
        public byte SocketCount;
        public byte Plus;

        public INpcItem ToItem()
        {
            StanderdItemStats std = new StanderdItemStats(PrizeID);
            Item item = new Item();
            item.ID = PrizeID;
            item.Plus = Plus;
            if (SocketCount >= 1)
                item.SocketOne = GemsConst.OpenSocket;
            if (SocketCount == 2)
                item.SocketOne = GemsConst.OpenSocket;
            item.Durability = std.Durability;
            item.MaxDurability = std.Durability;
            return item;
        }
        public override string ToString()
        {
            return PrizeName + " -> " + Chance;
        }
    }

    public struct LotteryBox
    {
        public LotteryItem[] Items;
        private List<LotteryItem> Buffer;
        private int Counter;
        public void OpenBox()
        {
            Items = null;
            Buffer = new List<LotteryItem>();
        }
        public void AddItem(LotteryItem item)
        {
            Buffer.Add(item);
        }
        public void SealBox()
        {
            Items = Buffer.ToArray();
            Buffer = null;
            Counter = 0;
            int rand = Kernel.Random.Next(10);
            for (int i = 0; i < rand; i++)
            {
                MixItems();
            }
        }
        public INpcItem SelectItem  (int rate)
        {
            for (Counter = Counter + 1; Counter < Items.Length; Counter++)
            {
                if (Items[Counter].Chance >= rate)
                    return Items[Counter].ToItem();
            }
            for (Counter = 0; Counter < Items.Length; Counter++)
            {
                if (Items[Counter].Chance >= rate)
                    return Items[Counter].ToItem();
            }
            return null;
        }
        public void MixItems()
        {
            Kernel.ShuffleArray<LotteryItem>(Items);
        }
    }

    public class Lottery
    {
        private static LotteryBox[] Boxes;
        private static DateTime LastUpdated;
        private static string LotteryPath;

        private static int ReloadLotteryItems()
        {
            int result = 0;
            DirectoryInfo dInfo = new DirectoryInfo(LotteryPath);
            if (LastUpdated != dInfo.LastWriteTime)
            {
                LastUpdated = dInfo.LastWriteTime;
                IniFile ini = new IniFile();
                string[] files = Directory.GetFiles(LotteryPath);
                LotteryBox[] temp_Boxes = new LotteryBox[5];
                for (int i = 0; i < temp_Boxes.Length; i++)
                    temp_Boxes[i].OpenBox();
                for (int i = 0; i < files.Length; i++)
                {
                    ini.FileName = files[i];
                    LotteryItem item = new LotteryItem();
                    item.BoxColor = (byte)(ini.ReadByte("cq_lottery", "color", 0) - 1);
                    item.Chance = ini.ReadByte("cq_lottery", "chance", 0);
                    item.PrizeName = ini.ReadString("cq_lottery", "prize_name", "", 32);
                    item.PrizeID = ini.ReadUInt32("cq_lottery", "prize_item", 0);
                    item.SocketCount = ini.ReadByte("cq_lottery", "hole_num", 0);
                    item.Plus = ini.ReadByte("cq_lottery", "addition_lev", 0);
                    temp_Boxes[item.BoxColor].AddItem(item);

                    StanderdItemStats std = new StanderdItemStats(item.PrizeID);
                    if (!item.PrizeName.Contains(std.Name))
                    {
                        Console.WriteLine("Corrupted Lottery File: \r\n" + ini.FileName);
                    }
                }
                for (int i = 0; i < temp_Boxes.Length; i++)
                {
                    temp_Boxes[i].SealBox();
                    result += temp_Boxes[i].Items.Length;
                }
                Boxes = temp_Boxes;
            }
            return result;
        }
        private static void MixBoxes()
        {
            Kernel.ShuffleArray<LotteryBox>(Boxes);
        }
        public static INpcItem SelectItem(int BoxID, int Rate)
        {
            ReloadLotteryItems();
            MixBoxes();
            return Boxes[BoxID].SelectItem(Rate);
        }
        public static int Init()
        {
            LotteryPath = ServerDatabase.Path + "\\Lottery\\";
            return ReloadLotteryItems();
        }
    }
}
