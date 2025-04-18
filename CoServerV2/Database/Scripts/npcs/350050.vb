' By: Hybrid
' Npc: 350050
' Name: Celestial Tao

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub CelestialTao()
        Dim dlg As String() = Nothing

        Select Case OptionID
            Case 0
                If Player.Reborn = 0 Then
                    ReDim dlg(1)
                    dlg(0) = "TEXT You are not reborn, I cannot assist you"
                    dlg(1) = "OPTION-1 I see."
                Else
                    ReDim dlg(2)
                    dlg(0) = "TEXT Would you like to allot your stats?"
                    dlg(1) = "OPTION1 Yes."
                    dlg(2) = "OPTION-1 No."
                End If
            Case 1
                If Player.Reborn > 0 Then
                    Command("@reallot")
                End If
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class