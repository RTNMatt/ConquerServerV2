'#include ...\define_guildranks.vb
' By: Andy, Hybrid
' Npc: 108883, 121171
' Name: LeftGate, RightGate

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub GuildGate()
        Dim dlg As String() = Nothing

        If Player.GuildID = GuildPoleID() And Player.GuildID <> 0 Then
            Select Case OptionID
                Case 0
                    ReDim dlg(3)
                    dlg(0) = "TEXT What is it you want?"
                    dlg(1) = "OPTION1 Let me in"
                    dlg(2) = "OPTION2 Open/Close Gate"
                    dlg(3) = "OPTION-1 Nothing"
                Case 1
                    If NpcID = 108883 Then ' Left Gate
                        Command("@mm 1038 159 202")
                    ElseIf NpcID = 121171 Then ' Right Gate
                        Command("@mm 1038 215 175")
                    End If
                Case 2
                    If Player.GuildRank = GuildRank.DeputyLeader Or Player.GuildRank = GuildRank.Leader Then
                        FlipGate(NpcID)
                    Else
                        ReDim dlg(1)
                        dlg(0) = "TEXT You must be a deputy, or the leader to do this."
                        dlg(1) = "OPTION-1 Oh"
                    End If
            End Select
        Else
            ReDim dlg(1)
            dlg(0) = "TEXT I'm sorry, I cannot help you."
            dlg(1) = "OPTION-1 Oh"
        End If

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class