' By: Wolf
' Npc: 10054
' Name: GeneralPeace

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub GeneralPeace()
        Dim dlg As String() = Nothing

        Select Case OptionID
            Case 0
                ReDim dlg(2)
                dlg(0) = "TEXT Hello, If you are brave I will send you to Desert City"
                dlg(1) = "OPTION1 I am brave, take me there."
                dlg(2) = "OPTION-1 I do not wish to go."
            Case 1
                Command("@mm 1000 971 666")
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class