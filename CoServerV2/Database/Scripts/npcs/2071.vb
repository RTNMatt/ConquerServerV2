'#include ...\item_const.vb
' By: Alec / Mujo
' Npc: 2071
' Name: CPAdmin

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub CPAdmin()
        Dim dlg As String() = Nothing

        Select Case OptionID
            Case 0
                ReDim dlg(2)
                dlg(0) = "TEXT Would you like to turn in a Dragonball for 215 CPs or Dragonball Scroll for 2150 cps?"
                dlg(1) = "OPTION1 Dragonball."
                dlg(2) = "OPTION2 Dragonball Scroll."
            Case 1
                If Player.CountItems(ItemConst.DragonBall) >= 1 Then
                    Player.RemoveItems(ItemConst.DragonBall, 1)
                    Player.ConquerPoints += 215
                Else
                    ReDim dlg(1)
                    dlg(0) = "TEXT You don't have a Dragonball, that's a shame."
                    dlg(1) = "OPTION-1 I see."
                End If
            Case 2
                If Player.CountItems(ItemConst.DBScroll) >= 1 Then
                    Player.RemoveItems(ItemConst.DBScroll, 1)
                    Player.ConquerPoints += 2150
                Else
                    ReDim dlg(1)
                    dlg(0) = "TEXT You don't have a Dragonball Scroll, that's a shame."
                    dlg(1) = "OPTION-1 I see."
                End If
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class