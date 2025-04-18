' By: Hybrid
' Item: InBetween(1000000, 1002050)
' Name: Potions

Imports System
Imports ConquerScriptLinker

Partial Public Class ItemEngine
    Public Shared Function Potions() As Boolean
        Dim ItemFile As String = "\Items\" & Item.ID.ToString() & ".ini"
        Dim GainHP As Integer = CInt(QueryDatabase("ItemInformation", "PotAddHP", "0", ItemFile))
        Dim GainMP As Integer = CInt(QueryDatabase("ItemInformation", "PotAddMP", "0", ItemFile))

        Player.HP = Math.Min(Player.HP + GainHP, Player.MaxHP)
        Player.MP = Math.Min(Player.MP + GainMP, Player.MaxMP)
        Return True
    End Function
End Class