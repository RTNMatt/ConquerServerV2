using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConquerServer_v2.Packet_Structures
{
    public enum MapEffectID : uint
    {
        DisableWeather = 0x00,
        EnableWeather = 0x20,
        MiniMap = 0x14
    }

    public enum MapEffectValue : uint
    {
        None = 0x00,
        MiniMapOn = 0x01,
        MinMapOff = 0x00,
        Rain = 0x02,
        Snow = 0x03,
        RainWithWind = 0x04,
        FallingLeaves = 0x05,
        CherryBlossoms = 0x07,
        FireFlies = 0x0A
    }

    public unsafe struct MapEffectPacket
    {
        public ushort Size;
        public ushort Type;
        public MapEffectValue Value;
        public int Density;
        public MapEffectID ID;
        public uint Appearance;
        public fixed byte TQServer[8];

        public static MapEffectPacket Create()
        {
            MapEffectPacket retn = new MapEffectPacket();
            retn.Size = 20;
            retn.Type = 0x3F8;
            PacketBuilder.AppendTQServer(retn.TQServer, 8);
            return retn;
        }
    }
}
