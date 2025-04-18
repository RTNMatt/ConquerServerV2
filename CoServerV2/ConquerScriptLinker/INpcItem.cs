using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerScriptLinker
{
    public unsafe interface INpcItem
    {
        uint UID { get; set; }
        uint ID { get; set; }
        byte Bless { get; set; }
        byte Enchant { get; set; }
        byte Plus { get; set; }
        byte SocketOne { get; set; }
        byte SocketTwo { get; set; }
        short RebornEffects { get; set; }
        byte Color { get; set; }
        ushort CurrentPosition { get; set; }
        void Send(INpcPlayer Player);
        ushort GetItemType();
    }
}
    