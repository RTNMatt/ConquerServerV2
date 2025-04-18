using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Client
{
    public unsafe class AuthClient
    {
        public NetworkClient NetworkSocket;
        public AuthCryptographer/*AuthCryptographer*/ Crypto;
        public uint AuthID;
        public string Account;
        public string Password;

        public AuthClient(NetworkClient Client)
        {
            NetworkSocket = Client;
            Crypto = new AuthCryptographer(true);//*/ new AuthCryptographer(true);
        }
        public void Send(void* Ptr)
        {
            ushort Size = *((ushort*)Ptr);
            byte[] Chunk = new byte[Size];
            Crypto.Encrypt((byte*)Ptr, Chunk, Size);
            NetworkSocket.Send(Chunk);
        }
    }
}
