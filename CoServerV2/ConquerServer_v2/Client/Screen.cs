using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Core;
using ConquerServer_v2.Monster_AI;

namespace ConquerServer_v2.Client
{
    public class ClientScreen
    {
        private Dictionary<uint, IMapObject> ScreenDictionary;
        private IMapObject[] m_Screen;
        private GameClient Client;

        public ClientScreen(GameClient _Client)
        {
            Client = _Client;
            ScreenDictionary = new Dictionary<uint, IMapObject>(20);
            m_Screen = new IMapObject[0];
        }
        public IMapObject[] Objects
        {
            get
            {
                return m_Screen;
            }
        }
        public bool Add(IMapObject Base)
        {
            lock (ScreenDictionary)
            {
                if (!ScreenDictionary.ContainsKey(Base.UID))
                {
                    ScreenDictionary.Add(Base.UID, Base);
                    IMapObject[] tmp_Screen = new IMapObject[ScreenDictionary.Count];
                    ScreenDictionary.Values.CopyTo(tmp_Screen, 0);
                    m_Screen = tmp_Screen;
                    return true;
                }
            }
            return false;
        }
        public void Remove(uint ID)
        {
            lock (ScreenDictionary)
            {
                if (ScreenDictionary.Remove(ID))
                {
                    IMapObject[] tmp_Screen = new IMapObject[ScreenDictionary.Count];
                    ScreenDictionary.Values.CopyTo(tmp_Screen, 0);
                    m_Screen = tmp_Screen;
                }
            }
        }
        public void Cleanup()
        {
            bool remove;
            foreach (IMapObject Base in m_Screen)
            {
                remove = false;
                if (Base.MapObjType == MapObjectType.Player)
                {
                    if (remove = (Kernel.GetDistance(Client.Entity.X, Client.Entity.Y, Base.X, Base.Y) >= 16))
                    {
                        GameClient pPlayer = Base.Owner as GameClient;
                        lock (pPlayer.Screen.ScreenDictionary)
                        {
                            pPlayer.Screen.ScreenDictionary.Remove(Client.Entity.UID);
                        }
                    }
                }
                else
                {
                    if (Base.MapObjType == MapObjectType.Monster)
                    {
                        Monster monster = Base.Owner as Monster;
                        if (monster.Target != null)
                        {
                            if (monster.Target.UID == Client.Entity.UID)
                                monster.Target = null;
                        }
                    }
                    remove = (Kernel.GetDistance(Client.Entity.X, Client.Entity.Y, Base.X, Base.Y) >= 16);
                }
                
                if (remove)
                {
                    lock (ScreenDictionary)
                    {
                        ScreenDictionary.Remove(Base.UID);
                    }
                }
            }
        }
        public void FullWipe()
        {
            lock (ScreenDictionary)
            {
                ScreenDictionary.Clear();
                m_Screen = new IMapObject[0];
            }
        }
        public IMapObject FindObject(uint UID)
        {
            if (UID == 0)
                return null;
            else if (UID == Client.Entity.UID)
                return Client.Entity;
            IMapObject obj;
            ScreenDictionary.TryGetValue(UID, out obj);
            return obj;
        }
    }
}