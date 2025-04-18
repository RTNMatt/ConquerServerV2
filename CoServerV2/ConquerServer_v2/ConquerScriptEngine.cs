using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using ScriptingEngine;
using ConquerServer_v2.Packet_Structures;
using ConquerServer_v2.Client;
using ConquerServer_v2.Database;
using ConquerServer_v2.Packet_Processor;
using ConquerServer_v2.GuildWar;
using ConquerServer_v2.Core;
using ConquerScriptLinker;

namespace ConquerServer_v2
{
    public unsafe partial class ConquerScriptEngine
    {
        public static ScriptEngine Engine;

        private static void ClearStringBuilder(ref StringBuilder sb)
        {
            sb.Remove(0, sb.Length);
        }
        private static string DumpEnumerationTypeVB(Type Type)
        {
            Type ec_type = Type;
            StringBuilder buf = new StringBuilder();
            buf.AppendLine("Public Class " + ec_type.Name);
            if (ec_type.IsEnum)
            {
                string const_type = Enum.GetUnderlyingType(ec_type).Name;
                foreach (object enum_val in Enum.GetValues(ec_type))
                {
                    buf.AppendLine(string.Format("\tPublic Const {0} As {1} = {2}", enum_val, const_type, Convert.ToInt64(enum_val)));
                }
            }
            else
            {
                foreach (FieldInfo element in ec_type.GetFields())
                {
                    if (element.IsLiteral && !element.FieldType.IsEnum)
                    {
                        buf.AppendLine(string.Format("\tPublic Const {0} As {1} = {2}", element.Name, element.FieldType.Name, element.GetValue(null)));
                    }
                }
            }
            buf.AppendLine("End Class");
            buf.AppendLine();
            return buf.ToString();
        }
        private static void CreateDefinedFiles()
        {
            StringBuilder StringWriter = new StringBuilder();

            ClearStringBuilder(ref StringWriter);
            StringWriter.AppendLine("Imports System");
            StringWriter.AppendLine(DumpEnumerationTypeVB(typeof(DataID)));
            StringWriter.AppendLine(DumpEnumerationTypeVB(typeof(DataGUIDialog)));
            StringWriter.AppendLine(DumpEnumerationTypeVB(typeof(DataSwitchArg)));
            File.WriteAllText(Engine.BuildPath + "define_data.vb", StringWriter.ToString());

            ClearStringBuilder(ref StringWriter);
            StringWriter.AppendLine("Imports System");
            StringWriter.AppendLine(DumpEnumerationTypeVB(typeof(ItemPosition)));
            StringWriter.AppendLine(DumpEnumerationTypeVB(typeof(GemsConst)));
            File.WriteAllText(Engine.BuildPath + "define_item.vb", StringWriter.ToString());

            ClearStringBuilder(ref StringWriter);
            StringWriter.AppendLine("Imports System");
            StringWriter.AppendLine(DumpEnumerationTypeVB(typeof(ServerFlags)));
            File.WriteAllText(Engine.BuildPath + "define_serverflags.vb", StringWriter.ToString());

            ClearStringBuilder(ref StringWriter);
            StringWriter.AppendLine("Imports System");
            StringWriter.AppendLine(DumpEnumerationTypeVB(typeof(UpdateID)));
            File.WriteAllText(Engine.BuildPath + "define_updateids.vb", StringWriter.ToString());

            ClearStringBuilder(ref StringWriter);
            StringWriter.AppendLine("Imports System");
            StringWriter.AppendLine(DumpEnumerationTypeVB(typeof(GuildRank)));
            File.WriteAllText(Engine.BuildPath + "define_guildranks.vb", StringWriter.ToString());

            ClearStringBuilder(ref StringWriter);
            StringWriter.AppendLine("Imports System");
            StringWriter.AppendLine(DumpEnumerationTypeVB(typeof(StringID)));
            File.WriteAllText(Engine.BuildPath + "define_string.vb", StringWriter.ToString());
        }
        private static ScriptExtension GenerateExtension()
        {
            ScriptExtension extend = new ScriptExtension("Engine", "VB");
            extend.AddPreprocess("'#new_assembly System.Core.dll");
            extend.AddPreprocess("'#assembly " + ServerDatabase.Startup + "\\ConquerScriptLinker.dll");

            extend.AddNamespace("Imports ConquerScriptLinker");

            extend.AddVariable("Public Shared DatabasePath As String = \"" + ServerDatabase.Path + "\"");
            extend.AddVariable("Public Shared Player As INpcPlayer");
            extend.AddVariable("Public Shared NativeDialog As Func(Of INpcPlayer, String(), Int32)");
            extend.AddVariable("Public Shared NativeCommand As Func(Of INpcPlayer, String, Int32)");
            extend.AddVariable("Public Shared FindPlayerByName As Func(Of String, INpcPlayer)");
            extend.AddVariable("Public Shared FindPlayerByUID As Func(Of UInt32, INpcPlayer)");
            extend.AddVariable("Public Shared QueryDatabase As Func(Of String, String, String, String, String)");
            extend.AddVariable("Public Shared WriteDatabase As Func(Of String, String, String, String, Int32)");
            extend.AddVariable("Public Shared timeGetTime As Func(Of UInt32)");
            extend.AddVariable("Public Shared GuildPoleID As Func(Of UInt16)");
            extend.AddVariable("Public Shared FlipGate As Func(Of UInt32, Int32)");
            extend.AddVariable("Public Shared GenerateLotteryItem As Func(Of Int32, INpcItem)");

            extend.AddFunction(
                "Public Shared Sub Dialog(ByVal Dlg As String())\r\n" +
                    "\tNativeDialog(Player, Dlg)\r\n" +
                "End Sub"
            );
            extend.AddFunction(
                "Public Shared Sub Command(ByVal Cmd As String)\r\n" +
                    "\tNativeCommand(Player, Cmd)\r\n" +
                "End Sub"
            );

            extend.AddExternalClass(
                "\r\nPartial Public Class NpcEngine\r\n" +
                    "Inherits Engine\r\n" +
                    "\tPublic Shared NpcID As UInt32\r\n" +
                    "\tPublic Shared OptionID As Byte\r\n" +
                    "\tPublic Shared Input As String\r\n" +
                "End Class\r\n"
            );

            extend.AddExternalClass(
                "\r\nPartial Public Class ItemEngine\r\n" +
                    "Inherits Engine\r\n" +
                    "\tPublic Shared Item as INpcItem\r\n" +
                "End Class\r\n"
            );
            return extend;
        }
        private static void LinkScriptMethods()
        {
            Engine.RegisterGlobalVariable("Engine", "NativeDialog", new Func<INpcPlayer, string[], int>(Dialog));
            Engine.RegisterGlobalVariable("Engine", "NativeCommand", new Func<INpcPlayer, string, int>(Command));
            Engine.RegisterGlobalVariable("Engine", "FindPlayerByName", new Func<string, INpcPlayer>(FindPlayerByName));
            Engine.RegisterGlobalVariable("Engine", "FindPlayerByUID", new Func<uint, INpcPlayer>(FindPlayerByUID));
            Engine.RegisterGlobalVariable("Engine", "QueryDatabase", new Func<string, string, string, string, string>(QueryDatabase));
            Engine.RegisterGlobalVariable("Engine", "WriteDatabase", new Func<string, string, string, string, int>(WriteDatabase));
            Engine.RegisterGlobalVariable("Engine", "timeGetTime", new Func<uint>(timeGetTime));
            Engine.RegisterGlobalVariable("Engine", "GuildPoleID", new Func<ushort>(GuildPoleID));
            Engine.RegisterGlobalVariable("Engine", "FlipGate", new Func<uint, int>(FlipGate));
            Engine.RegisterGlobalVariable("Engine", "GenerateLotteryItem", new Func<int, INpcItem>(GenerateLotteryItem));
        }

        public static void Init()
        {
            Engine = new ScriptEngine("VB");
            Engine.Extension = GenerateExtension();
            Engine.BuildPath = ServerDatabase.Path + "\\Scripts\\";

            LinkScriptMethods();
            CreateDefinedFiles();
        }
        private static void DisplayErrors(CompiledScript res)
        {
            if (!res.Success)
            {
                if (res.Errors != null)
                {
                    if (res.Errors.Length > 0)
                    {
                        Console.WriteLine("Reporting Errors for Start File:");
                        Console.WriteLine("`{0}`", res.StartFile);
                        foreach (System.CodeDom.Compiler.CompilerError err in res.Errors)
                        {
                            Console.WriteLine(err);
                            Console.WriteLine();
                        }
                        Console.WriteLine("--------------------\n");
                    }
                }
            }
        }

        public static void ProcessNpc(GameClient Client, byte OptionID, string Input)
        {
            try
            {
                switch (Client.ActiveNpcID)
                {
                    case 422: TournamentNpc(Client, OptionID, Input); break;
                    default:
                        {
                            string processfile = ServerDatabase.Path + @"\Scripts\npcs\process.vb";
                            CompiledScript Script = Engine.CompileScript(processfile);
                            if (Script.Errors.Length > 0)
                                DisplayErrors(Script);
                            else
                            {
                                // Ensure synchoronization with the scripts, if the same script
                                // were to be running twice at the same time, we'd get quite a bit of problems!
                                lock (Script)
                                {
                                    Type StandardEngine = Script.GetCompiledType("Engine");
                                    Script.SetStaticVariable(StandardEngine, "Player", Client.NpcLink);

                                    Type NpcEngine = Script.GetCompiledType("NpcEngine");
                                    Script.SetStaticVariable(NpcEngine, "NpcID", Client.ActiveNpcID);
                                    Script.SetStaticVariable(NpcEngine, "OptionID", OptionID);
                                    Script.SetStaticVariable(NpcEngine, "Input", Input);
                                    Script.RunStaticFunction(NpcEngine, "Execute");
                                }
                            }
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Kernel.NotifyDebugMsg(string.Format("[Npc Processor Error - {0}, {1}, {2}]", Client.ActiveNpcID, OptionID, Input),
                    e.ToString(), true);
            }
        }
        public static bool ProcessItem(GameClient Client, Item Item)
        {
            string item_file = Engine.BuildPath + @"items\" + Item.ID.ToString() + ".vb";
            switch (Item.ID)
            {
                default:
                    {
                        string processfile = ServerDatabase.Path + @"\Scripts\items\process.vb";
                        CompiledScript Script = Engine.CompileScript(processfile);
                        if (Script.Errors.Length > 0)
                            DisplayErrors(Script);
                        else
                        {
                            // Ensure synchoronization with the scripts, if the same script
                            // were to be running twice at the same time, we'd get quite a bit of problems!
                            lock (Script)
                            {
                                Type StandardEngine = Script.GetCompiledType("Engine");
                                Script.SetStaticVariable(StandardEngine, "Player", Client.NpcLink);

                                Type ItemEngine = Script.GetCompiledType("ItemEngine");
                                Script.SetStaticVariable(ItemEngine, "Item", Item as INpcItem);
                                return (bool)Script.RunStaticFunction(ItemEngine, "Execute");
                            }
                        }
                        break;
                    }
            }
            return false;
        }
        public static int Dialog(INpcPlayer Player, string[] Dlg)
        {
            const int BaseSize = 0x11 + 8;
            byte* ptr = stackalloc byte[BaseSize];
            int string_size = 0;
            NpcClickPacket* Reply;

            foreach (string parse_dlg in Dlg)
            {
                if (parse_dlg.StartsWith("AVATAR"))
                {
                    string str_avatar = parse_dlg.Substring(7, parse_dlg.Length - 7);
                    Reply = NpcClickPacket.Create(ptr, 0);
                    Reply->ResponseID = NpcClickID.Avatar;
                    Reply->Avatar = ushort.Parse(str_avatar);
                    Player.Send(Reply);
                }
                else if (parse_dlg.StartsWith("TEXT"))
                {
                    string str_text = parse_dlg.Substring(5, parse_dlg.Length - 5);
                    if (string_size < str_text.Length)
                    {
                        string_size = str_text.Length;
                        byte* temp = stackalloc byte[BaseSize + string_size];
                        ptr = temp;
                    }
                    Reply = NpcClickPacket.Create(ptr, str_text.Length);
                    Reply->ResponseID = NpcClickID.Dialogue;
                    Reply->Input = str_text;
                    Player.Send(Reply);
                }
                else if (parse_dlg.StartsWith("OPTION"))
                {
                    string str_op_num = parse_dlg.Substring(6, parse_dlg.IndexOf(' ') - 6);
                    string str_op_text = parse_dlg.Substring(6 + str_op_num.Length + 1, parse_dlg.Length - 6 - str_op_num.Length - 1);
                    if (string_size < str_op_text.Length)
                    {
                        string_size = str_op_text.Length;
                        byte* temp = stackalloc byte[BaseSize + string_size];
                        ptr = temp;
                    }
                    Reply = NpcClickPacket.Create(ptr, str_op_text.Length);
                    Reply->ResponseID = NpcClickID.Option;
                    Reply->OptionID = (byte)short.Parse(str_op_num);
                    Reply->Input = str_op_text;
                    Player.Send(Reply);
                }
                else if (parse_dlg.StartsWith("INPUT"))
                {
                    string str_in_num = parse_dlg.Substring(5, parse_dlg.IndexOf(' ') - 5);
                    string str_txt_len = parse_dlg.Substring(5 + str_in_num.Length + 1, parse_dlg.Length - 5 - str_in_num.Length - 1);
                    Reply = NpcClickPacket.Create(ptr, 0);
                    Reply->ResponseID = NpcClickID.Input;
                    Reply->MaxInputLength = ushort.Parse(str_txt_len);
                    Reply->OptionID = (byte)short.Parse(str_in_num);
                    Player.Send(Reply);
                }
                else if (parse_dlg.StartsWith("NOP"))
                    continue;
                else
                    throw new ArgumentException("Failed to parse npc dialog statement `" + parse_dlg + "`");
            }
            Reply = NpcClickPacket.Create(ptr, 0);
            Reply->ResponseID = NpcClickID.Finish;
            Reply->DontDisplay = false;
            Player.Send(Reply);
            return 0;
        }
        public static int Command(INpcPlayer Player, string Command)
        {
            PacketProcessor.ProcessServerCommand((Player as ClientNpcLink).Owner, Command, false);
            return 0;
        }
        public static INpcPlayer FindPlayerByName(string Name)
        {
            GameClient Client = Kernel.FindClientByName(Name);
            if (Client != null)
                return Client.NpcLink;
            return null;
        }
        public static INpcPlayer FindPlayerByUID(uint UID)
        {
            GameClient Client = Kernel.FindClientByUID(UID);
            if (Client != null)
                return Client.NpcLink;
            return null;
        }
        public static string QueryDatabase(string Section, string Key, string Default, string File)
        {
            return new IniFile(ServerDatabase.Path + File).ReadString(Section, Key, Default);
        }
        public static int WriteDatabase(string Section, string Key, string Value, string File)
        {
            new IniFile(ServerDatabase.Path + File).WriteString(Section, Key, Value);
            return 0;
        }
        public static uint timeGetTime()
        {
            return TIME.Now.Time;
        }
        public static ushort GuildPoleID()
        {
            return GuildWarKernel.PoleGuildID;
        }
        public static int FlipGate(uint GateID)
        {
            SOBMonster Gate = GuildWarKernel.Search(GateID);
            if (Gate != null)
            {
                if (Gate.Dead)
                {
                    Gate.Dead = false;
                    GuildWarKernel.ReverseGate(Gate);
                }
                else
                {
                    Gate.Hitpoints = 0;
                    if (Gate.Killed != null)
                        Gate.Killed(null, Gate, 0);
                }
            }
            return 0;
        }
        public static INpcItem GenerateLotteryItem(int BoxColor)
        {
            int lucky = (Kernel.Random.Next(0, 1000) % 90) + 10;
            return Lottery.SelectItem(BoxColor, lucky);
        }
        
        //NpcID: 422
        public static void TournamentNpc(GameClient Client, byte OptionID, string Input)
        {
            if (TournamentAI.CanJoin && TournamentAI.Active)
            {
                if (!Client.Entity.Dead)
                {
                    Client.Entity.Hitpoints = 1;
                    UpdatePacket Update = UpdatePacket.Create();
                    Update.UID = Client.Entity.UID;
                    Update.ID = UpdateID.Hitpoints;
                    Update.Value = (uint)Client.Entity.Hitpoints;
                    Client.Send(&Update);

                    Client.Teleport(TournamentAI.MapID, TournamentAI.X, TournamentAI.Y);
                }
            }
        }
    }
}