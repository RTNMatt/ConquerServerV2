' By: Hybrid
' Item: InBetween(1060030, 1060090)
' Name: HairDyes

Imports System
Imports ConquerScriptLinker

Partial Public Class ItemEngine
    Public Shared HairDyes As Byte() = {3, 9, 8, 7, 6, 5, 4}
    Public Shared Function HairDyesEx() As Boolean
        Dim Idx As Integer = ((Item.ID Mod 100) / 10) - 3
        Dim Color As Byte = HairDyes(Idx)
        Dim HairID As Byte = CByte(Player.Hairstyle Mod 100)
        Player.Hairstyle = (Color * 100) + HairID
        Return True
    End Function
End Class