using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        private static bool FullSuper(GameClient Client)
        {
            for (ItemPosition pos = Item.FirstSlot; pos < Item.LastSlot; pos++)
            {
                if (pos == ItemPosition.Bottle || pos == ItemPosition.Garment || pos == ItemPosition.Left)
                    continue;
                Item gear = Client.Equipment[pos];
                if (gear == null)
                    return false;
                if (gear.ID % 10 != 9) // != super
                    return false;
            }
            return true;
        }
        private static void ChangeAction(GameClient Client, ConquerAction Action, DataPacket* dPtr)
        {
            if (Client.Entity.Action != Action)
            {
                Client.Entity.Action = Action;
                if (dPtr == null)
                {
                    DataPacket temp = DataPacket.Create();
                    dPtr = &temp;
                    dPtr->UID = Client.Entity.UID;
                    dPtr->ID = DataID.ChangeAction;
                    dPtr->dwParam1 = (uint)Action;
                }

                TIME Now = TIME.Now;
                if (Client.Entity.Action == ConquerAction.Cool && Client.TimeStamps.CanShowCool.Time <= Now.Time)
                {
                    Client.TimeStamps.CanShowCool = Now.AddSeconds(5);
                    StringPacket Effect = null;
                    if (!FullSuper(Client))
                    {
                        Item Armor = Client.Equipment[ItemPosition.Armor];
                        if (Armor != null)
                        {
                            if (Armor.ID % 10 == 9) // Super
                            {
                                Effect = new StringPacket();
                                Effect.Strings = new string[1];
                                /*if ((Client.Job >= 100 && Client.Job <= 101) || (Client.Job >= 132 && Client.Job <= 135) || (Client.Job >= 142 && Client.Job <= 145))
                                    Effect.Strings[0] = "taoist";
                                else if (Client.Job >= 10 && Client.Job <= 15)
                                    Effect.Strings[0] = "warrior";
                                else if (Client.Job >= 20 && Client.Job <= 25)
                                    Effect.Strings[0] = "fighter";
                                else if (Client.Job >= 40 && Client.Job <= 45)
                                    Effect.Strings[0] = "archer";*/
                            }
                        }
                    }
                    else
                    {
                        Effect = new StringPacket();
                        Effect.Strings = new string[1];
                        if ((Client.Job >= 100 && Client.Job <= 101) || (Client.Job >= 132 && Client.Job <= 135) || (Client.Job >= 142 && Client.Job <= 145))
                            Effect.Strings[0] = "taoist";
                        else if (Client.Job >= 10 && Client.Job <= 15)
                            Effect.Strings[0] = "warrior";
                        else if (Client.Job >= 20 && Client.Job <= 25)
                            Effect.Strings[0] = "fighter";
                        else if (Client.Job >= 40 && Client.Job <= 45)
                            Effect.Strings[0] = "archer";
                    }
                    if (Effect != null)
                    {
                        if (Effect.Strings[0] != null)
                        {
                            Effect.UID = Client.Entity.UID;
                            Effect.ID = StringID.Effect;
                            Effect.StringsLength = (byte)Effect.Strings[0].Length;
                            SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Effect, null);
                        }
                    }
                }
                SendRangePacket.Add(Client.Entity, Kernel.ViewDistance, 0, Kernel.ToBytes(dPtr), null);
            }
        }
        public static void ChangeAction(GameClient Client, DataPacket* lpPacket)
        {
            lpPacket->UID = Client.Entity.UID;
            ChangeAction(Client, (ConquerAction)lpPacket->dwParam1, lpPacket);
        }
    }
}