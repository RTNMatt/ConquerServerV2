' By: Alec / Mujo
' Npc: 104797 - 104833 - 104845 - 104851 - 104839
' Name: Boxer

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub Boxer()
        Dim dlg As String() = Nothing
        Select Case OptionID
            Case 0
                ReDim dlg(2)
                dlg(0) = "TEXT Would you like to enter the Training Grounds for 1,000 silvers?"
                dlg(1) = "OPTION1 Yes."
                dlg(2) = "OPTION-1 No."
            Case 1
                If Player.Money >= 1000 Then
                    Player.Money -= 1000
                    Command("@mm 1039 218 216")
                Else
                    ReDim dlg(1)
                    dlg(0) = "TEXT You do not have 1000 silvers."
                    dlg(1) = "OPTION-1 Oh, I'm sorry."
                End If
        End Select
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class