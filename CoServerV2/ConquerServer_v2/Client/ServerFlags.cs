using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Client
{
    [Flags]
    public enum ServerFlags : uint
    {
        None = 0x00,
        DHExchanged = 0x01,
        LoggedOut = 0x02,
        LoggedIn = 0x04,
        LoadedCharacter = 0x08,
        IsAutoAttacking = 0x10,
        //IsStandardAttack = 0x20,
        WarehouseOpen = 0x40,
        GotLotteryItem = 0x80,
        MagicAuto = 0x100,
        PhysicalAuto = 0x200,
        Mining = 0x400
    }
}
