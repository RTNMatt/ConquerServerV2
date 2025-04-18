' By: hybrid
' Npc: 924
' Name: LadyLuck (lottery, leaving)

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub LadyLuckLeaving()
        Dim dlg As String() = Nothing
        Select Case OptionID
            Case 0
                ReDim dlg(2)
                dlg(0) = "TEXT Bitch, I'mma pop a cap in yo' ass if you don' get your nigga ass out ma house."
                dlg(1) = "OPTION1 Help!"
                dlg(2) = "OPTION-1 Bring it, nigga."
            Case 1
                Command("@mm " & Player.LastMapID.ToString() & " " & Player.LastX.ToString() & " " & Player.LastY.ToString())
        End Select
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class