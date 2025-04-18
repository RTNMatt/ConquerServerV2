using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ConquerServer_v2.Database;
using ConquerServer_v2.Client;
using ConquerServer_v2.Monster_AI;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.GuildWar;

namespace ConquerServer_v2.Core
{
    public unsafe class Kernel
    {
        public const byte ViewDistance = 24;
        public const byte MinLevel = 1, MaxLevel = 130;
        public const ushort MeeleSpellID = 8036;
        public const double ExperienceRate = 2.5;

        private static object WriteSync;
        public static Dictionary<uint, DataMap> DMaps;
        public static DictionaryV2<uint, GameClient> ClientDictionary;

        public static RandomGenerator Random;
        public static GameClient[] Clients { get { return ClientDictionary.EnumerableValues; } }

        static Kernel()
        {
            Random = new RandomGenerator();
            WriteSync = new object();
            ClientDictionary = new DictionaryV2<uint, GameClient>();
        }

        public static void ShuffleArray<T>(T[] Array)
        {
            T[] source = Array;
            for (int inx = source.Length - 1; inx > 0; inx--)
            {
                int position = Kernel.Random.Next(inx + 1);
                T temp = source[inx];
                source[inx] = source[position];
                source[position] = temp;
            }
        }
        public static uint MoneyToItemID(int amount)
        {
            uint ItemID;
            if (amount >= 0 && amount <= 49)
                ItemID = 1090000; // Silver
            else if (amount >= 50 && amount <= 100)
                ItemID = 1090010; // Sycee
            else if (amount >= 100 && amount <= 499)
                ItemID = 1090020; // Gold
            else if (amount >= 500 && amount <= 999)
                ItemID = 1091000; // Gold Bullion
            else if (amount >= 1000 && amount <= 9999)
                ItemID = 1091010; // Gold Bar
            else
                ItemID = 1091020; // Gold Bars
            return ItemID;
        }
        public static void IncXY(ConquerAngle Facing, ref ushort x, ref ushort y)
        {
            sbyte xi, yi;
            xi = yi = 0;
            switch (Facing)
            {
                case ConquerAngle.North: xi = -1; yi = -1; break;
                case ConquerAngle.South: xi = 1; yi = 1; break;
                case ConquerAngle.East: xi = 1; yi = -1; break;
                case ConquerAngle.West: xi = -1; yi = 1; break;
                case ConquerAngle.NorthWest: xi = -1; break;
                case ConquerAngle.SouthWest: yi = 1; break;
                case ConquerAngle.NorthEast: yi = -1; break;
                case ConquerAngle.SouthEast: xi = 1; break;
            }
            x = (ushort)(x + xi);
            y = (ushort)(y + yi);
        }
        public static void NotifyDebugMsg(string Type, string Msg, bool Log)
        {
            lock (WriteSync)
            {
                Console.WriteLine(Type + "\r\n" + Msg);
                if (Log)
                {
                    DateTime now = DateTime.Now;
                    string file = now.Month.ToString() + "-" +
                        now.Day.ToString() + "-" +
                        now.Year.ToString();
                    File.AppendAllText(
                        ServerDatabase.Startup + "\\Debugging\\" + file + ".log",
                        Type + "\r\n" + Msg + "\r\n\r\n"
                    );
                }
            }
        }
        public static int GetDistance(ushort X, ushort Y, ushort X2, ushort Y2)
        {
            return Math.Max(Math.Abs(X - X2), Math.Abs(Y - Y2));
        }
        public static double GetE2DDistance(int X, int Y, int X2, int Y2)
        {
            int x = Math.Abs(X - X2);
            int y = Math.Abs(Y - Y2);
            return Math.Sqrt((x * x) + (y * y));
        }
        public static ConquerAngle GetFacing(short angle)
        {
            sbyte c_angle = (sbyte)((angle / 46) - 1);
            return (c_angle == -1) ? ConquerAngle.South : (ConquerAngle)c_angle;
        }
        public static short GetAngle(ushort X, ushort Y, ushort x2, ushort y2)
        {
            double r = Math.Atan2(y2 - Y, x2 - X);
            if (r < 0)
                r += Math.PI * 2;
            return (short)Math.Round(r * 180 / Math.PI);
        }
        public static byte[] ToBytes(void* PacketPtr)
        {
            ushort Size = *((ushort*)PacketPtr);
            byte[] Bytes = new byte[Size + 8];
            fixed (byte* dst = Bytes)
            {
                MSVCRT.memcpy(dst, PacketPtr, Bytes.Length);
            }
            return Bytes;
        }
        public static ushort GetAttributePoints(ushort Level)
        {
            const byte PrimeLevel = 120;

            ushort ret = 0;
            for (short i = (short)(Level - PrimeLevel); i > 0; i--)
                ret = (ushort)(ret + i);
            return ret;
        }

        /// <summary>
        /// Finds a client by uid (uint), returns null if no client with 
        /// the specified UID is found.
        /// </summary>
        /// <param name="ClientUID">The UID of the client to search for.</param>
        public static GameClient FindClientByUID(uint ClientUID)
        {
            foreach (GameClient Client in Clients)
            {
                if (Client.Entity.UID == ClientUID)
                    return Client;
            }
            return null;
        }
        /// <summary>
        /// Finds a entity by uid (uint), returns null if no entity with 
        /// the specified UID is found.
        /// </summary>
        /// <param name="EntityUID">The UID of the entity to search for.</param>
        /// <param name="MapID">The MapID that the entity is on.</param>
        /// <returns></returns>
        public static IBaseEntity FindEntity(uint EntityUID, DataMap Map)
        {
            if (EntityUID >= 1000000)
            {
                GameClient Player = FindClientByUID(EntityUID);
                if (Player != null)
                    return Player.Entity;
            }
            else if (EntityUID >= 700000 && EntityUID <= 800000)
            {
                GameClient Player = FindClientByUID((uint)(1000000 + (EntityUID - 700000)));
                if (Player != null)
                    if (Player.Pet != null)
                        return Player.Pet.Entity;
            }
            else
            {
                if (Map.HasMobs)
                {
                    Monster monster = Map.Mobs.Search(EntityUID);
                    if (monster != null)
                        return monster.Entity;
                }
            }
            return null;
        }
        /// <summary>
        /// Finds a client by name (string), returns null if no client with 
        /// the specified name is found.
        /// </summary>
        /// <param name="ClientUID">The Name of the client to search for.</param>
        public static GameClient FindClientByName(string Name)
        {
            foreach (GameClient Client in Clients)
            {
                if (Client.Entity.Name == Name)
                    return Client;
            }
            return null;
        }
        /// <summary>
        /// Reloads a players screen
        /// </summary>
        /// <param name="Client">The player's client</param>
        /// <param name="Callback">A callback (only called on other players)</param>
        public static void GetScreen(GameClient Client, ConquerCallback Callback)
        {
            Client.Screen.Cleanup();
            int Distance;
            const int ScreenView = 16;
            TIME Now = TIME.Now;
            MapID MapID = Client.Entity.MapID;

            #region Clients
            foreach (GameClient iClient in Kernel.Clients)
            {
                if (iClient != null)
                {
                    if (iClient.Entity.MapID.Id == Client.Entity.MapID.Id &&
                        iClient.Entity.UID != Client.Entity.UID)
                    {
                        Distance = GetDistance(Client.Entity.X, Client.Entity.Y, iClient.Entity.X, iClient.Entity.Y);
                        if (Distance <= ScreenView)
                        {
                            iClient.Entity.SendSpawn(Client);
                            if (iClient.Pet != null)
                            {
                                if (iClient.Pet.Entity.Dead)
                                    iClient.Pet.Entity.SendSpawn(Client);
                            }
                            if (Callback != null)
                            {
                                Callback(Client.Entity, iClient.Entity);
                            }
                        }
                    }
                }
            }
            #endregion
            if (Client.CurrentDMap != null)
            {
                #region Npcs
                foreach (NpcEntity npc in Client.CurrentDMap.Npcs.EnumerableValues)
                {
                    if (npc.MapID.Id == MapID.Id)
                    {
                        Distance = GetDistance(Client.Entity.X, Client.Entity.Y, npc.X, npc.Y);
                        if (Distance <= ScreenView)
                        {
                            npc.SendSpawn(Client);
                        }
                    }
                }
                #endregion
                #region Monsters
                if (Client.CurrentDMap.HasMobs)
                {
                    if (Client.CurrentDMap.Mobs.MapID.Id == MapID.Id)
                    {
                        foreach (MonsterSpawn Spawn in Client.CurrentDMap.Mobs.Spawns)
                        {
                            if ((Client.Entity.X >= Spawn.SpawnX && Client.Entity.X <= Spawn.MaxSpawnX &&
                                Client.Entity.Y >= Spawn.SpawnY && Client.Entity.Y <= Spawn.MaxSpawnY) ||
                                Kernel.GetDistance((ushort)Spawn.SpawnX, (ushort)Spawn.SpawnY, Client.Entity.X, Client.Entity.Y) <= ScreenView)
                            {
                                foreach (Monster monster in Spawn.Monsters)
                                {
                                    if (!monster.Entity.Dead)
                                    {
                                        if (Kernel.GetDistance(Client.Entity.X, Client.Entity.Y, monster.Entity.X, monster.Entity.Y) <= ScreenView)
                                        {
                                            monster.Entity.SendSpawn(Client);
                                            monster.AssignPotentialTarget(Client.Entity);
                                            monster.Spawn.RunAI();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
                #region Dropped Items
                DictionaryV2<uint, IDroppedItem> droppedItems = Client.CurrentDMap.DroppedItems;
                bool resync = false;
                foreach (IDroppedItem item in droppedItems.EnumerableValues)
                {
                    if (item.MapID.Id == MapID.Id)
                    {
                        if (item.RemoveTime.Time > Now.Time)
                        {
                            if (Kernel.GetDistance(item.X, item.Y, Client.Entity.X, Client.Entity.Y) <= ScreenView)
                            {
                                item.SendSpawn(Client);
                            }
                        }
                        else
                        {
                            DroppedItemPacket dItem = (DroppedItemPacket)item;
                            dItem.DropType = DropID.Remove;
                            SendRangePacket.Add(item.MapID, item.X, item.Y,
                                    Kernel.ViewDistance, 0, Kernel.ToBytes(&dItem), null);
                            Client.CurrentDMap.SetItemOnTile(item.X, item.Y, false);
                            droppedItems.Remove(item.UID, false);
                            resync = true;
                        }
                    }
                }
                if (resync)
                    droppedItems.SynchoronizeValues();
                #endregion
            }
            #region GuildWar Monsters
            if (Client.Entity.MapID == MapID.GuildWar)
            {
                foreach (SOBMonster monster in GuildWarKernel.Monsters)
                {
                    if (Kernel.GetDistance(monster.X, monster.Y, Client.Entity.X, Client.Entity.Y) <= ScreenView)
                        monster.SendSpawn(Client);
                }
            }
            #endregion
        }
    }
}

