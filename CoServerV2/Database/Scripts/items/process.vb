'#includedir ...\items\*.vb

Imports System
Imports ConquerScriptLinker

Partial Public Class ItemEngine
    Public Shared Function Execute() As Boolean
        Dim DeleteItem As Boolean = False
        Select Case Item.ID
            Case 720010 To 720017
                DeleteItem = PotionBoxes()
            Case 1060030 To 1060090
                DeleteItem = HairDyesEx()
            Case 1000000 To 1000020, 1001000 To 1001040, 1002020 To 1002050
                DeleteItem = Potions()
            Case 720027, 720028
                DeleteItem = MetDBScroll()
            Case 722312
                DeleteItem = BlessKey()
        End Select
        Return DeleteItem
    End Function
End Class