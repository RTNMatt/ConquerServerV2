using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ConquerServer_v2.Core;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Client;

namespace ConquerServer_v2
{
    public unsafe class CreationThread
    {
        class GuildData
        {
            // if (Name==null) deleteguild
            public string Name;
            public GameClient Client;
        }
        class CharacterCreation
        {
            public GameClient Client;
            public CreateCharacterPacket Packet;
        }
        class CreationData
        {
            public const byte
                GuildDataType = 1,
                CharacterCreateType = 2;

            public byte Type;
            public object CreationObj;

            public CreationData(byte type, object obj)
            {
                Type = type;
                CreationObj = obj;
            }
        }

        private static int pendingThreads = 0;
        public static int PendingThreads
        {
            get { return pendingThreads; }
        }
        private static void _internalProcess(object obj)
        {
            pendingThreads++;
            CreationData data = obj as CreationData;
            if (data != null)
            {
                if (data.Type == CreationData.GuildDataType)
                {
                    #region GuildDataType
                    GuildData guild_data = data.CreationObj as GuildData;
                    if (guild_data != null)
                    {
                        if (guild_data.Client != null)
                        {
                            if (guild_data.Name == null && guild_data.Client.Guild.ID != 0)
                            {
                                if (guild_data.Client.Guild.Rank == GuildRank.Leader)
                                    guild_data.Client.Guild.Delete();
                            }
                            else if (guild_data.Client.Guild.ID == 0 && guild_data.Name != null)
                            {
                                if (!guild_data.Client.Guild.CreateNew(guild_data.Name))
                                    guild_data.Client.Send(MessageConst.FAILED_MAKE_GUILD);
                            }
                        }
                    }
                    #endregion
                }
                else if (data.Type == CreationData.CharacterCreateType)
                {
                    #region CharacterCreateType
                    CharacterCreation char_data = data.CreationObj as CharacterCreation;
                    if (char_data.Client != null)
                    {
                        fixed (CreateCharacterPacket* ptr = &char_data.Packet)
                            Packet_Processor.PacketProcessor.CreateCharacter(char_data.Client, ptr);
                    }
                    #endregion
                }
            }
            pendingThreads--;
        }
        private static ParameterizedThreadStart internalProcess = new ParameterizedThreadStart(_internalProcess);

        public static void CharacterCreate(GameClient Client, CreateCharacterPacket* Ptr)
        {
            CharacterCreation data = new CharacterCreation();
            data.Client = Client;
            data.Packet = *Ptr;
            new Thread(internalProcess).Start(new CreationData(CreationData.CharacterCreateType, data));
        }
        public static void Guild(GameClient Client, string GuildName)
        {
            GuildData data = new GuildData();
            data.Client = Client;
            data.Name = GuildName;
            new Thread(internalProcess).Start(new CreationData(CreationData.GuildDataType, data));
        }
    }
}   
