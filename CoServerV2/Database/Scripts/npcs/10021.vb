' By: Wolf
' Npc: 10021
' Name: ArenaGuard

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub ArenaGuard()
        Dim dlg As String() = Nothing

        Select Case OptionID
            Case 0
                ReDim dlg(2)
                dlg(0) = "TEXT Hello would you like to enter the PK Arena for 50 silvers?"
                dlg(1) = "OPTION1 Yes, here you go."
                dlg(2) = "OPTION-1 No thanks."
            Case 1
                If Player.Money >= 50 Then
                    Player.Money -= 50
                    Command("@mm 1005 50 50")
                Else
                    ReDim dlg(1)
                    dlg(0) = "TEXT I'm sorry you dont have 50 silvers."
                    dlg(1) = "OPTION-1 Oh, I will go work my corner more."
                End If
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class