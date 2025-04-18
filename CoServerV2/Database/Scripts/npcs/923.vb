'#include ...\define_serverflags.vb
' By: Hybrid
' Npc: 923
' Name: LadyLuck (lottery, leaving)

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub EnterLottery()
        Player.ServerFlags = Player.ServerFlags And (Not ServerFlags.GotLotteryItem)
        Command("@mm 700 40 50")
    End Sub
    Public Shared Sub LadyLuckEnter()
        Dim dlg As String() = Nothing
        Select Case OptionID
            Case 0
                ReDim dlg(4)
                dlg(0) = "TEXT Hello, I am LadyLuck. You can enter the lottery through me for a price of 27cps. "
                dlg(1) = "TEXT You may also enter for free if you have a lottery ticket."
                dlg(2) = "TEXT You can win many great prizes from the lottery, would you like to play?"
                dlg(3) = "OPTION1 Yes."
                dlg(4) = "OPTION-1 No."
            Case 1
                If Player.ConquerPoints >= 27 Then
                    Player.ConquerPoints -= 27
                    EnterLottery()
                ElseIf Player.CountItems(710212) > 0 Then ' Lottery Ticket
                    Player.RemoveItems(710212, 1)
                    EnterLottery()
                Else
                    ReDim dlg(2)
                    dlg(0) = "TEXT It seems you don't have 27 cps, and you do not have a lottery ticket. "
                    dlg(1) = "TEXT I cannot let you enter."
                    dlg(2) = "OPTION-1 I see."
                End If
        End Select
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class