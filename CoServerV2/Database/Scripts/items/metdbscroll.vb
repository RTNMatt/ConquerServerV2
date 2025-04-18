'#include ...\item_const.vb
' By: Hybrid
' Item: InBetween(720028, 720027)
' Name: Meteor/DBScroll

Imports System
Imports ConquerScriptLinker

Partial Public Class ItemEngine
    Public Shared Function MetDBScroll() As Boolean
        If Player.InventorySpace >= 10 Then
            Dim cmd As String = Nothing
            If Item.ID = ItemConst.MeteorScroll Then
                cmd = "@item Meteor Fixed"
            ElseIf Item.ID = ItemConst.DBScroll Then
                cmd = "@item DragonBall Fixed"
            End If
            If Not cmd Is Nothing Then
                Dim i As Integer
                For i = 1 To 10
                    Command(cmd)
                Next
                Return True
            End If
        Else
            Dim dlg(0) As String
            dlg(0) = "TEXT You must have 10 empty slots in your inventory to use this item."
            Dialog(dlg)
        End If
        Return False
    End Function
End Class