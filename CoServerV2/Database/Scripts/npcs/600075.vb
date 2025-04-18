' By: Alec / Mujo
' Npc: 600075
' Name: Boxer

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub BoxerLeave()
        Dim dlg As String() = Nothing
        Select Case OptionID
            Case 0
                ReDim dlg(2)
                dlg(0) = "TEXT Would you like to leave the Training Grounds?"
                dlg(1) = "OPTION1 Yes Please."
                dlg(2) = "OPTION-1 Nope!"
            Case 1
                Command("@mm " & Player.LastMapID.ToString() & " " & Player.LastX.ToString() & " " & Player.LastY.ToString())
        End Select
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class