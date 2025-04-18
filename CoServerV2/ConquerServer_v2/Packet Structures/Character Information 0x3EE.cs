using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;

namespace ConquerServer_v2.Packet_Structures
{
    /// <summary>
    /// 0x3EE (Server->Client)
    /// </summary>
    public unsafe class CharacterInfoPacket
    {
        public GameClient Client;
        public CharacterInfoPacket(GameClient _Client)
        {
            Client = _Client;
        }
        public static implicit operator byte[](CharacterInfoPacket info)
        {
            byte name_len = (byte)info.Client.Entity.Name.Length;
            byte[] Buffer = new byte[116 + 8 + name_len + info.Client.Spouse.Length];
            fixed (byte* Packet = Buffer)
            {
                *((ushort*)(Packet)) = (ushort)(Buffer.Length - 8);
                *((ushort*)(Packet + 2)) = 0x3EE;
                *((uint*)(Packet + 4)) = info.Client.Entity.UID;
                *((uint*)(Packet + 10)) = info.Client.Entity.Spawn.Model;
                *((ushort*)(Packet + 14)) = info.Client.Entity.Spawn.Hairstyle;
                *((int*)(Packet + 16)) = info.Client.Money;
                *((int*)(Packet + 20)) = info.Client.ConquerPoints;
                *((uint*)(Packet + 24)) = info.Client.Experience;

                *((StatData*)(Packet + 52)) = info.Client.Stats;
                *((ushort*)(Packet + 62)) = (ushort)info.Client.Entity.Hitpoints;
                *((ushort*)(Packet + 64)) = (ushort)info.Client.Manapoints;
                *((ushort*)(Packet + 66)) = info.Client.PKPoints;
                Packet[68] = (byte)info.Client.Entity.Spawn.Level;
                Packet[69] = info.Client.Job;
                Packet[71] = (byte)info.Client.Entity.Spawn.Reborn;
                // 71 = quiz show
                /* chunk of data added here for 5130 */

                Packet[110] = 0x01;
                Packet[111] = name_len;
                info.Client.Entity.Name.CopyTo(Packet + 112);


                //Packet[87] = 0x02;
                //Packet[88] = name_len;
                //Packet[89 + name_len] = (byte)info.Client.Spouse.Length;
                //info.Client.Entity.Name.CopyTo(Packet + 89);
                //info.Client.Spouse.CopyTo(Packet + 90 + name_len);
                PacketBuilder.AppendTQServer(Packet, Buffer.Length);
            }
            return Buffer;
        }
    }
}