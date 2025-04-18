using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Client
{
    public unsafe interface IAssociate
    {
        uint UID { get; set; }
        bool Online { get; set; }
        AssociationID ID { set; }
        string Name { get; set; }
        string Account { get; set; }

        void CopyNameBuffer(sbyte* Dest);
        void CopyAccountBuffer(sbyte* Dest);
        void CopyToNameBuffer(sbyte* Src);
        void CopyToAccountBuffer(sbyte* Src);
        void Send(GameClient Client);
    }

    public static class AssociateExtentions
    {
        public static unsafe void ObtainDatabaseData(this FlexibleArray<IAssociate> collection, bool Friends, GameClient Output)
        {
            IniFile ini = new IniFile();
            for (int i = 0; i < collection.Length; i++)
            {
                IAssociate a = collection.Elements[i]; 
                sbyte* NameBuffer = stackalloc sbyte[16];
                ini.FileName = ServerDatabase.Path + "\\Accounts\\" + a.Account + ".ini";
                ini.ReadString("Character", "Name", null, NameBuffer, 16);
                a.CopyToNameBuffer(NameBuffer);
                a.ID = Friends ? AssociationID.AddFriend : AssociationID.AddEnemy;
                a.UID = ini.ReadUInt32("Character", "UID", 0);
                a.Online = (ini.ReadByte("Character", "Logged", 0) == 1);
                a.Send(Output);
            }
        }
        public static List<uint> GetOnlineUIDList(this FlexibleArray<IAssociate> Associates)
        {
            List<uint> uids = new List<uint>(Associates.Length);
            for (int i = 0; i < Associates.Length; i++)
                if (Associates.Elements[i].Online)
                    uids.Add(Associates.Elements[i].UID);
            return uids;
        }
        public static IAssociate Search(this FlexibleArray<IAssociate> Associates, uint UID)
        {
            for (int i = 0; i < Associates.Length; i++)
                if (Associates.Elements[i].UID == UID)
                    return Associates.Elements[i];
            return null;
        }
        public static IAssociate Search(this FlexibleArray<IAssociate> Associates, uint UID, out int Slot)
        {
            Slot = -1;
            for (int i = 0; i < Associates.Length; i++)
            {
                if (Associates.Elements[i].UID == UID)
                {
                    Slot = i;
                    return Associates.Elements[i];
                }
            }
            return null;
        }
    }
}
