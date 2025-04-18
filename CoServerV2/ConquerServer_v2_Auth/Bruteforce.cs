using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;

namespace ConquerServer_v2.BruteForce
{
    public class BruteForceEntry
    {
        public string IPAddress;
        public int WatchCheck;
        public TIME Unbantime;
        public TIME AddedTimeRemove;
    }

    public class BruteforceProtection
    {
        private static DictionaryV2<string, BruteForceEntry> collection = new DictionaryV2<string, BruteForceEntry>();
        private static int BanOnWatch;

        private static ThreadStart internalInit = new ThreadStart(_internalInit);
        private static void _internalInit()
        {
            TIME Now;
            bool Resync;
            while (true)
            {
                lock (collection)
                {
                    Now = WinMM.timeGetTime();
                    Resync = false;
                    foreach (BruteForceEntry bfe in collection.EnumerableValues)
                    {
                        if (bfe.AddedTimeRemove.Time <= Now.Time)
                        {
                            collection.Remove(bfe.IPAddress, false);
                        }
                        else if (bfe.Unbantime.Time != 0)
                        {
                            if (bfe.Unbantime.Time <= Now.Time)
                            {
                                collection.Remove(bfe.IPAddress, false);
                            }
                        }
                    }
                    if (Resync)
                        collection.SynchoronizeValues();
                }

                Thread.Sleep(2000);
            }
        }

        public static void Init(int WatchBeforeBan)
        {
            BanOnWatch = WatchBeforeBan;
            new Thread(internalInit).Start();
        }

        public static void AddWatch(string IPAddress)
        {
            lock (collection)
            {
                BruteForceEntry bfe;
                if (!collection.TryGetValue(IPAddress, out bfe))
                {
                    bfe = new BruteForceEntry();
                    bfe.IPAddress = IPAddress;
                    bfe.WatchCheck = 1;
                    bfe.AddedTimeRemove = WinMM.timeGetTime().AddMinutes(5);
                    bfe.Unbantime = new TIME(0);
                    collection.Add(IPAddress, bfe);
                }
                else
                {
                    bfe.WatchCheck++;
                    if (bfe.WatchCheck >= BanOnWatch)
                    {
                        bfe.Unbantime = WinMM.timeGetTime().AddMinutes(15);
                    }
                }
            }
        }
        
        public static bool IsBanned(string IPAddress)
        {
            bool check = false;
            BruteForceEntry bfe;
            if (collection.TryGetValue(IPAddress, out bfe))
            {
                check = (bfe.Unbantime.Time != 0);
            }
            return check;
        }
    }
}
