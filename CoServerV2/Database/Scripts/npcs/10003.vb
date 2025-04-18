' By: Hybrid
' Npc: 10003
' Name: GuildDirector

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub GuildDirector()
        Dim dlg As String() = Nothing

        Select Case OptionID
            Case 0
                ReDim dlg(7)
                ' You could have just used one long TEXT line,
                ' But that would look ugly :P
                dlg(0) = "TEXT Hello, I'm the guild director, how may I help you?"
                dlg(1) = "OPTION1 Create Guild"
                dlg(2) = "OPTION2 Disband Guild"
                dlg(3) = "OPTION3 Kick Member"
                dlg(4) = "OPTION4 Deputy"
                dlg(5) = "OPTION5 Allies"
                dlg(6) = "OPTION6 Enemies"
                dlg(7) = "OPTION13 Pass Leadership"
            Case 1
                If String.Equals(Input, "") Then
                    ReDim dlg(2)
                    dlg(0) = "TEXT What would you like to call your guild? It will cost 1,000,000 gold to make it."
                    dlg(1) = "INPUT1 15"
                    dlg(2) = "OPTION-1 Never mind."
                Else
                    If Player.GuildID = 0 Then
                        If Player.Money >= 1000000 Then
                            Command("@createguild " & Input)
                        Else
                            ReDim dlg(1)
                            dlg(0) = "TEXT You don't seem to have 1,000,000 gold."
                            dlg(1) = "OPTION-1 I see."
                        End If
                    Else
                        ReDim dlg(1)
                        dlg(0) = "TEXT You seem to already be in a guild."
                        dlg(1) = "OPTION-1 I see."
                    End If
                End If
            Case 2
                If Not String.Equals(Input, "yes") Then
                    ReDim dlg(3)
                    dlg(0) = "TEXT Are you really sure you want to disband your guild? "
                    dlg(1) = "TEXT If you are, then type `yes` in the box"
                    dlg(2) = "INPUT2 3"
                    dlg(3) = "OPTION-1 Never mind."
                Else
                    ' This command itself will check if the person calling it is the guild leader
                    Command("@disbandguild")
                End If
            Case 3
                If String.Equals(Input, "") Then
                    ReDim dlg(2)
                    dlg(0) = "TEXT What is the name of the person you wish to kick out?"
                    dlg(1) = "INPUT3 15"
                    dlg(2) = "OPTION-1 Nevermind."
                Else
                    ' This command itself will check if the person calling it is the guild leader
                    ' This command will also ensure that you don't kickout yourself, lol.
                    Command("@kickguild " & Input)
                End If
            Case 4
                ' Deputies automatically capped at 5
                ReDim dlg(2)
                dlg(0) = "TEXT What would you like do to?"
                dlg(1) = "OPTION7 Add a deputy"
                dlg(2) = "OPTION8 Remove a deputy"
            Case 5
                ReDim dlg(2)
                dlg(0) = "TEXT What would you like do to?"
                dlg(1) = "OPTION9 Add an ally"
                dlg(2) = "OPTION10 Remove an ally"
            Case 6
                ReDim dlg(2)
                dlg(0) = "TEXT What would you like do to?"
                dlg(1) = "OPTION11 Add an enemy"
                dlg(2) = "OPTION12 Remove an enemy"
            Case 7
                If String.Equals(Input, "") Then
                    ReDim dlg(2)
                    dlg(0) = "TEXT What is the name of the person you want to make a deputy?"
                    dlg(1) = "INPUT7 15"
                    dlg(2) = "OPTION-1 Nevermind."
                Else
                    Command("@adddeputy " & Input)
                End If
            Case 8
                If String.Equals(Input, "") Then
                    ReDim dlg(2)
                    dlg(0) = "TEXT What is the name of the person you want to remove from being a deputy?"
                    dlg(1) = "INPUT8 15"
                    dlg(2) = "OPTION-1 Nevermind."
                Else
                    Command("@removedeputy " & Input)
                End If
            Case 9
                If String.Equals(Input, "") Then
                    ReDim dlg(2)
                    dlg(0) = "TEXT What is the name of the guild you want to make an ally?"
                    dlg(1) = "INPUT9 15"
                    dlg(2) = "OPTION-1 Nevermind."
                Else
                    Command("@addally " & Input)
                End If
            Case 10
                If String.Equals(Input, "") Then
                    ReDim dlg(2)
                    dlg(0) = "TEXT What is the name of the guild you want to remove from being an ally?"
                    dlg(1) = "INPUT10 15"
                    dlg(2) = "OPTION-1 Nevermind."
                Else
                    Command("@removeally " & Input)
                End If
            Case 11
                If String.Equals(Input, "") Then
                    ReDim dlg(2)
                    dlg(0) = "TEXT What is the name of the guild you want to make an enemy?"
                    dlg(1) = "INPUT11 15"
                    dlg(2) = "OPTION-1 Nevermind."
                Else
                    Command("@addenemy " & Input)
                End If
            Case 12
                If String.Equals(Input, "") Then
                    ReDim dlg(2)
                    dlg(0) = "TEXT What is the name of the guild you want to remove from being an enemy?"
                    dlg(1) = "INPUT12 15"
                    dlg(2) = "OPTION-1 Nevermind."
                Else
                    Command("@removeenemy " & Input)
                End If
            Case 13
                If String.Equals(Input, "") Then
                    ReDim dlg(1)
                    dlg(0) = "TEXT Type in the person's name you wish to pass leadership to."
                    dlg(1) = "INPUT13 15"
                Else
                    Command("@passleader " & Input)
                End If
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class