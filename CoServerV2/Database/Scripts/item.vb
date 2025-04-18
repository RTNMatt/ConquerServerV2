Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Function GetItemName(ByVal ItemID As UInteger) As String
        Dim path As String = "\Items\" + ItemID.ToString() + ".ini"
        Return QueryDatabase("ItemInformation", "ItemName", "ERROR", path)
    End Function
End Class