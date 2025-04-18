' By: Hybrid
' Npc: 10063 
' Name: Shopboy

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub Shopboy()
        Dim dlg As String() = Nothing

        Select Case OptionID
            Case 0
                ReDim dlg(2)
                dlg(0) = "TEXT Hello, would you like to have your gear died for one meteor?"
                dlg(1) = "OPTION1 Yes."
                dlg(2) = "OPTION-1 No."
            Case 1
                If Player.CountItems(1088001) >= 1 Then ' Note: 1088001 is the ItemID for Meteors
                    Player.RemoveItems(1088001, 1)
                    Command("@mm 1008 25 25")
                Else
                    ReDim dlg(1)
                    dlg(0) = "TEXT You don't have a meteor, that's a shame."
                    dlg(1) = "OPTION-1 I see."
                End If
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class