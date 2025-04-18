'
' ( HYBRID SCRIPT ENGINE 2009 GENERATED HEADER ) 
'
'#new_assembly System.Core.dll
'#assembly C:\Users\Redme\Desktop\CoServerV2\ConquerServer_v2\bin\Debug\ConquerScriptLinker.dll
Imports System
Imports ConquerScriptLinker
Partial Public Class Engine
Public Shared DatabasePath As String = "C:\Users\Redme\Desktop\CoServerV2\Database"
Public Shared Player As INpcPlayer
Public Shared NativeDialog As Func(Of INpcPlayer, String(), Int32)
Public Shared NativeCommand As Func(Of INpcPlayer, String, Int32)
Public Shared FindPlayerByName As Func(Of String, INpcPlayer)
Public Shared FindPlayerByUID As Func(Of UInt32, INpcPlayer)
Public Shared QueryDatabase As Func(Of String, String, String, String, String)
Public Shared WriteDatabase As Func(Of String, String, String, String, Int32)
Public Shared timeGetTime As Func(Of UInt32)
Public Shared GuildPoleID As Func(Of UInt16)
Public Shared FlipGate As Func(Of UInt32, Int32)
Public Shared GenerateLotteryItem As Func(Of Int32, INpcItem)
Public Shared Sub Dialog(ByVal Dlg As String())
	NativeDialog(Player, Dlg)
End Sub
Public Shared Sub Command(ByVal Cmd As String)
	NativeCommand(Player, Cmd)
End Sub
End Class

Partial Public Class NpcEngine
Inherits Engine
	Public Shared NpcID As UInt32
	Public Shared OptionID As Byte
	Public Shared Input As String
End Class


Partial Public Class ItemEngine
Inherits Engine
	Public Shared Item as INpcItem
End Class

