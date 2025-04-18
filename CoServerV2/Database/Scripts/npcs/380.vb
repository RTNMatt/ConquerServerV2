' By: Saint
' Npc: 380
' Name: GuildController

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub GuildController()
        Dim dlg As String() = Nothing

        Select Case OptionID
            Case 0
                ReDim dlg(2)
                dlg(0) = "TEXT Hello would you like to enter the guild arena?"
                dlg(1) = "OPTION1 Yes, some bitches need to suck my cock."
                dlg(2) = "OPTION-1 No, I heard them bitches got 9 inch cocks!"
            Case 1
                If Player.GuildWarTime <= timeGetTime() Then
                    Command("@mm 1038 350 340")
                Else
                    Dim wait As Integer = (Player.GuildWarTime - timeGetTime()) / (60 * 1000)
                    If wait > 10 Then
                        Command("@mm 1038 350 340")
                    Else
                        ReDim dlg(1)
                        dlg(0) = "TEXT You can't enter yet, fuck off. You have " & wait.ToString() & " minutes left to wait."
                        dlg(1) = "OPTION-1 I see, faggot."
                    End If
                End If
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class