'#include ...\item_const.vb

' By: Hybrid
' Npc: 5004
' Name: MillionaireLee

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub Pack(ByVal ItemID As UInteger, ByVal ScrollCommand As String, ByRef dlg As String(), ByVal Item As String)
        If Player.CountItems(ItemID) >= 10 Then
            Player.RemoveItems(ItemID, 10)
            Command(ScrollCommand)
        Else
            ReDim dlg(1)
            dlg(0) = "TEXT It seems you didn't have the 10 " & Item & " as expected."
            dlg(1) = "OPTION-1 I see."
        End If
    End Sub
    Public Shared Sub MillionaireLee()
        Dim dlg As String() = Nothing

        Select Case OptionID
            Case 0
                ReDim dlg(4)
                ' You could have just used one long TEXT line,
                ' But that would look ugly :P
                dlg(0) = "TEXT I am the rich and proud Millionaire, Lee! "
                dlg(1) = "TEXT I can pack Dragonballs, and Meteors of 10 into scrolls, would you like me to do so?"
                dlg(2) = "OPTION1 Yes, Dragonballs."
                dlg(3) = "OPTION2 Yes, Meteors."
                dlg(4) = "OPTION-1 No."
            Case 1
                ' Don't even ask why it's Elite, lol
                Pack(ItemConst.Dragonball, "@item DBScroll Elite", dlg, "dragonballs")
            Case 2
                ' Don't even ask why it's Unique, lol
                Pack(ItemConst.Meteor, "@item MeteorScroll Unique", dlg, "meteors")
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class