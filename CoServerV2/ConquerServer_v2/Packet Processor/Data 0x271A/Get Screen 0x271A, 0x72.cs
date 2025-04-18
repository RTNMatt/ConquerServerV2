using System;
using ConquerServer_v2.Core;
using ConquerServer_v2.Client;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Database;

namespace ConquerServer_v2.Packet_Processor
{
    public unsafe partial class PacketProcessor
    {
        public static void LoadScreen(GameClient Client, DataPacket* Packet)
        {
            Client.Screen.FullWipe();
            if (Client.IsVendor)
                Client.Vendor.StopVending();

            MapSettings Settings = new MapSettings(Client.Entity.MapID);
            Packet->ID = DataID.SetMapColor;
            Packet->dwParam1 = Settings.Color;
            Client.Send(Packet);

            MapStatusPacket Region = MapStatusPacket.Create();
            Region.MapID = Client.Entity.MapID;
            Region.Status = Settings.Status;
            Client.Send(&Region);

            MapEffectPacket Effect = MapEffectPacket.Create();
            Effect.ID = MapEffectID.MiniMap;
            Effect.Value = MapEffectValue.MiniMapOn;
            Client.Send(&Effect);
            
            /*wPacket.wType = (WeatherType)Settings.Weather;
            if (wPacket.wType != WeatherType.None)
            {
                wPacket.wOptions = WeatherOptions.EnableWeather;
                wPacket.Density = 100;
                wPacket.Appearance = 50;
                Client.Send(&wPacket, wPacket.Size);
            }*/

            Kernel.GetScreen(Client, ConquerCallbackKernel.GetScreenReply);
        }
    }
}