using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;

namespace ConquerServer_v2.Core
{
    public delegate int ConquerCallback(IBaseEntity Sender, IBaseEntity Caller);
    public delegate int ConquerCallback<T>(IBaseEntity Sender, IBaseEntity Caller, T Param);

    public unsafe class ConquerCallbackKernel
    {
        public static ConquerCallback GetScreenReply = new ConquerCallback(_GetScreenReply);
        public static ConquerCallback CommonSendSpawn = new ConquerCallback(_CommonSendSpawn);
        public static ConquerCallback CommonRemoveScreen = new ConquerCallback(_CommonRemoveScreen);
        public static ConquerCallback<short> AngleCheck = new ConquerCallback<short>(_AngleCheck);
        public static ConquerCallback NotifyFriendsImOnline = new ConquerCallback(_NotifyFriendOnline);
        public static ConquerCallback NotifyFriendsImOffline = new ConquerCallback(_NotifyFriendOffline);
        public static ConquerCallback EnsureUserIsDead = new ConquerCallback(_EnsureUserIsDead);

        private static int _EnsureUserIsDead(IBaseEntity Sender, IBaseEntity Receiver)
        {
            if (Sender.Dead)
                return 0;
            return 1;
        }
        private static int _AngleCheck(IBaseEntity IAttacker, IBaseEntity IOpponent, short _Angle)
        {
            if (Math.Abs(_Angle - Kernel.GetAngle(IAttacker.X, IAttacker.Y, IOpponent.X, IOpponent.Y)) <= 120)
                return 1;
            return 0;
        }
        private static int _CommonSendSpawn(IBaseEntity Sender, IBaseEntity Receiver)
        {
            (Sender as CommonEntity).SendSpawn(Receiver.Owner as GameClient);
            return 0;
        }
        private static int _CommonRemoveScreen(IBaseEntity Sender, IBaseEntity Receiver)
        {
            (Receiver.Owner as GameClient).Screen.Remove(Sender.UID);
            return 0;
        }
        private static int _GetScreenReply(IBaseEntity Sender, IBaseEntity Receiver)
        {
            GameClient ReceiverClient = Receiver.Owner as GameClient;
            GameClient SenderClient = Sender.Owner as GameClient;
            SenderClient.Entity.SendSpawn(ReceiverClient);
            return 0;
        }
        private static int _NotifyFriendOnline(IBaseEntity Sender, IBaseEntity Caller)
        {
            GameClient friendClient = Caller.Owner as GameClient;
            IAssociate senderFriend = friendClient.Friends.Search(Sender.UID);
            if (senderFriend != null)
            {
                senderFriend.Online = true;
                senderFriend.ID = AssociationID.SetOnlineFriend;
                senderFriend.Send(friendClient);
            }
            return 0;
        }
        private static int _NotifyFriendOffline(IBaseEntity Sender, IBaseEntity Caller)
        {
            GameClient friendClient = Caller.Owner as GameClient;
            IAssociate senderFriend = friendClient.Friends.Search(Sender.UID);
            if (senderFriend != null)
            {
                senderFriend.Online = false;
                senderFriend.ID = AssociationID.SetOfflineFriend;
                senderFriend.Send(friendClient);
            }
            return 0;
        }
    }
}
