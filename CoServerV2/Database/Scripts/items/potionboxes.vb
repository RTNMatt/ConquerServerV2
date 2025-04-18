' By: Hybrid
' Item: InBetween(720010, 720017)
' Name: Potions

Imports System
Imports ConquerScriptLinker

Partial Public Class ItemEngine
    Public Shared Function PotionBoxes() As Boolean
        If Player.InventorySpace >= 3 Then
            Dim cmd As String = QueryDatabase("PotionBoxes", Item.ID.ToString(), "", "\Scripts\Items\PotionBoxes.ini")
            If Not String.Equals(cmd, "") Then
                Dim i As Integer
                For i = 1 To 3
                    Command(cmd)
                Next i
                Return True
            End If
        End If
        Return False
    End Function
End Class