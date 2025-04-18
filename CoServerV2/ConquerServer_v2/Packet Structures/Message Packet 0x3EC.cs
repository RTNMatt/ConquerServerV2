using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public class MessageConst
    {
        private static byte[] _ANSWER_OK;
        
        public static byte[] WAREHOUSE_MONEY_FULL = new MessagePacket("Your warehouse money cannot exceed " + int.MaxValue.ToString(), 0x00FF0000, ChatID.TopLeft);
        public static byte[] SUCCESS_REMOVE_GUILD = new MessagePacket("The guild has been removed from ally/enemy but you will not see this change unless you restart your client.", 0x0000FF00, ChatID.Center);
        public static byte[] PK_FORBIDDEN = new MessagePacket("You are forbidden to PK on this map.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] FIGHT = new MessagePacket("FIGHT", 0x00FFFFFF, ChatID.Center);
        public static byte[] TOURNAMENT_START1 = new MessagePacket("A tournament has started, visit `OldQuarrier` at `@mm 1020 563 524` to join!", 0x00FFFFFF, ChatID.Center);
        public static byte[] TOURNAMENT_START2 = new MessagePacket("A tournament has started, visit `OldQuarrier` at `@mm 1020 563 524` to join!", 0x00FFFFFF, ChatID.Broadcast);
        public static byte[] FRIEND_LIST_FULL = new MessagePacket("Either you, or your friends' friend list is full.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] NO = new MessagePacket("No.", 0x00FF0000, ChatID.Center);
        public static byte[] WALK_ONLY = new MessagePacket("Please, only walk into this area.", 0x00FF0000, ChatID.Center);
        public static byte[] CLEAR_TOP_RIGHT = new MessagePacket("", 0xCCCC00, ChatID.ClearTopRight);
        public static byte[] CANNOT_UPGRADE_LEVEL = new MessagePacket("You cannot upgrade this items level.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] CANNOT_UPGRADE_QUALITY = new MessagePacket("You cannot upgrade this items quality.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] PLAYER_OFFLINE = new MessagePacket("Player offline.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] FAILED_MAKE_GUILD = new MessagePacket("Your guild could not be created due to the name being taken, or too long.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] ERROR_IN_TRADE = new MessagePacket("There was an error with the trade", 0x00FF0000, ChatID.TopLeft);
        public static byte[] ALREADY_IN_TRADE = new MessagePacket("You're already in a trade.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] PLAYER_IN_TRADE = new MessagePacket("Player already in a trade.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] NOT_ENOUGH_ROOM_INVENTORY = new MessagePacket("There is not enough room in your inventory", 0x00FF0000, ChatID.TopLeft);
        public static byte[] CANNOT_AFFORD = new MessagePacket("You cannot afford this.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] ALREADY_IN_TEAM2 = new MessagePacket("They're already in a team.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] TEAM_FULL = new MessagePacket("Sorry, the team is full.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] ALREADY_IN_TEAM = new MessagePacket("You're already in a team, please leave before preforming this action.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] SPEED_HACK = new MessagePacket("You've been suspected of speed hacking.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] WAIT_MESSAGE = new MessagePacket("Please wait before sending any more messages.", 0x00FF0000, ChatID.TopLeft);
        public static byte[] FAILED_LOAD_CHARACTER = new MessagePacket("The server failed to load your character. If this problem persists contact an admin.", 0x00FFFFFF, ChatID.Dialog);
        public static byte[] NEW_ROLE = new MessagePacket("NEW_ROLE", "ALLUSERS", 0x00FFFFFF, ChatID.Dialog);
        //public static byte[] ANSWER_OK = new MessagePacket("ANSWER_OK", "", 0x00FFFFFF, ChatID.Dialog);
        //public static byte[] ANSWER_OK = new MessagePacket("ANSWER_OK", "ALLUSERS", "SYSTEM", 0x00FFFFFF, ChatID.Dialog);
        
        //Fixes Loggin Disconnections
        public static byte[] ANSWER_OK
        {
            get
            {
                if (_ANSWER_OK == null)
                {
                    _ANSWER_OK = new MessagePacket("ANSWER_OK", "ALLUSERS", "SYSTEM", 0x00FFFFFF, ChatID.Dialog);
                    return _ANSWER_OK;
                }
                else
                {
                    _ANSWER_OK = new MessagePacket("ANSWER_OK", "ALLUSERS", "SYSTEM", 0x00FFFFFF, ChatID.Dialog);
                    return _ANSWER_OK;
                }
            }
        }

        public static byte[] ANSWER_NO = new MessagePacket("No, fuck off.", "ALLUSERS", 0x00FFFFFF, ChatID.Dialog);
        public static byte[] INVENTORY_FULL = new MessagePacket("Your inventory is full.", 0x00FF0000, ChatID.TopLeft);
    }

    public enum ChatID : ushort
    {
        Talk = 0x7D0,
        Team = 0x7D3,
        Guild = 0x7D4,
        Whisper = 0x7D1,
        Service = 0x7DE,
        TopLeft = 0x7D5,
        Friends = 0x7D9,
        Ghost = 0x7DD,
        CharacterCreation = 0x834,
        Center = 0x7DB,
        WorldChat = 0x7E5,
        Broadcast = 0x9C4,
        Dialog = 0x835,
        TopRight = 0x83D,
        ClearTopRight = 0x83C,
        Board = 0x899,
        Bulletin = 0x83F
    }

    public unsafe struct MessagePacket
    {
        public string Message, From, To;
        public uint dwParam;
        public uint Color;
        public ChatID ChatType;
        public uint SenderModel;

        public MessagePacket(string _Message, uint _Color, ChatID _ChatType)
        {
            Message = _Message;
            To = "ALL";
            From = "SYSTEM";
            Color = _Color;
            ChatType = _ChatType;
            dwParam = SenderModel = 0;
        }
        public MessagePacket(string _Message, string _To, uint _Color, ChatID _ChatType)
        {
            Message = _Message;
            To = _To;
            From = "SYSTEM";
            Color = _Color;
            ChatType = _ChatType;
            dwParam = SenderModel = 0;
        }
        public MessagePacket(string _Message, string _To, string _From, uint _Color, ChatID _ChatType)
        {
            Message = _Message;
            To = _To;
            From = _From;
            Color = _Color;
            ChatType = _ChatType;
            dwParam = SenderModel = 0;
        }
        public static implicit operator byte[](MessagePacket Msg)
        {
            byte[] Buffer = new byte[32 + 8 + Msg.Message.Length + Msg.From.Length + Msg.To.Length];
            fixed (byte* Packet = Buffer)
            {
                *((ushort*)(Packet)) = (ushort)(Buffer.Length - 8);
                *((ushort*)(Packet + 2)) = 0x3EC;
                *((uint*)(Packet + 4)) = Msg.Color;
                *((ChatID*)(Packet + 8)) = Msg.ChatType;
                *((uint*)(Packet + 12)) = Msg.dwParam;
                *((uint*)(Packet + 20)) = Msg.SenderModel;
                Packet[24] = 0x04;

                Packet[25] = (byte)Msg.From.Length;
                Packet[26 + Msg.From.Length] = (byte)Msg.To.Length;
                Packet[28 + Msg.From.Length + Msg.To.Length] = (byte)Msg.Message.Length;

                Msg.From.CopyTo(Packet + 26);
                Msg.To.CopyTo(Packet + 27 + Msg.From.Length);
                Msg.Message.CopyTo(Packet + 29 + Msg.From.Length + Msg.To.Length);
                PacketBuilder.AppendTQServer(Packet, (ushort)Buffer.Length);
            }
            return Buffer;
        }
        public static implicit operator MessagePacket(byte* Pointer)
        {
            MessagePacket Msg = new MessagePacket();
            Msg.Color = *((uint*)(Pointer + 4));
            Msg.ChatType = *((ChatID*)(Pointer + 8));
            Msg.dwParam = *((uint*)(Pointer + 12));
            Msg.SenderModel = *((uint*)(Pointer + 20));
            Msg.From = new string((sbyte*)(Pointer + 26), 0, *(Pointer + 25));
            Msg.To = new string((sbyte*)(Pointer + 27 + Msg.From.Length), 0, *(Pointer + 26 + Msg.From.Length));
            Msg.Message = new string((sbyte*)(Pointer + 29 + Msg.From.Length + Msg.To.Length), 0, *(Pointer + 28 + Msg.From.Length + Msg.To.Length));
            return Msg;
        }
    }
}
