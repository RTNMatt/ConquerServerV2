' By: Hybrid
' Npc: Mike, Alex, Hoang, Bryan
' Name: 30164, 3600, 3604, 3602

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub _30164_MakeBlessKey(ByRef dlg As String())
        Dim count As Integer = 0
        Dim j As Integer = 0
        For j = 0 To 7
            count += Player.CountItems(700002 + (j * 10))
        Next
        If count >= 5 Then
            If Player.CountItems(1072059) >= 5 Then ' Rate 10 gold ore
                Player.RemoveItems(1072059, 5)
                For j = 0 To 7
                    count -= Player.RemoveItems(700002 + (j * 10), count)
                    If count <= 0 Then
                        Exit For ' Break
                    End If
                Next

                Command("@item BlessKey Fixed")
            Else
                ReDim dlg(1)
                dlg(0) = "TEXT You do not have the 5 rate 10 gold ores."
                dlg(1) = "OPTION-1 I see."
            End If
        Else
            ReDim dlg(1)
            dlg(0) = "TEXT You do not have the 5 refined gems."
            dlg(1) = "OPTION-1 I see."
        End If
    End Sub
    Public Shared Sub Mike()
        Dim dlg As String() = Nothing
        'If Player.Reborn = 1 Then
        Select Case OptionID
            Case 0
                ReDim dlg(2)
                dlg(0) = "TEXT I see that you are reborn. Perhaps you are seeking to be reborn once again. "
                dlg(1) = "TEXT Perhaps I can assist you with this, I am an expert crafter."
                dlg(2) = "OPTION1 Can you create a BlessKey?"
            Case 1
                ReDim dlg(3)
                dlg(0) = "TEXT A bless key you say? Hmm... I suppose so, but I'll need somethings. "
                dlg(1) = "TEXT I'll need 5 refined gems, and 5 Rate 10 Gold ores."
                dlg(2) = "OPTION2 I have them."
                dlg(3) = "OPTION-1 I shall acquire them."
            Case 2
                _30164_MakeBlessKey(dlg)
        End Select
        'Else
        'ReDim dlg(1)
        'dlg(0) = "TEXT It's awful hot around these parts isn't it?"
        'dlg(1) = "OPTION-1 Sure is."
        'End If

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
    Public Shared Sub Alex()
        Dim dlg As String() = Nothing
        'If Player.Reborn = 1 Then
        Select Case OptionID
            Case 0
                ReDim dlg(3)
                dlg(0) = "TEXT I see that you are reborn. Perhaps you are seeking to be reborn once again. "
                dlg(1) = "TEXT Perhaps I can assist you with this, I am an old warrior with much knowledge. "
                dlg(2) = "TEXT Do you wish to visit my old training ground?"
                dlg(3) = "OPTION1 Send me to the Abyss"
            Case 1
                If Player.CountItems(723011) >= 1 Then
                    Player.RemoveItems(723011, 1)
                    Command("@mm 1700 700 700")
                Else
                    ReDim dlg(2)
                    dlg(0) = "TEXT You must acquire a pythons heart. You can obtain it in the pythons den. "
                    dlg(1) = "TEXT To get there, you must acquire a blessed key. Talk to Mike in Desert City for help creating one."
                    dlg(2) = "OPTION-1 Ok"
                End If
        End Select
        'Else
        'ReDim dlg(1)
        'dlg(0) = "TEXT I use to be one of the greatest warriors in the age of the New Dynasty"
        'dlg(1) = "OPTION-1 And now your an old man."
        'End If
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
    Public Shared _3605_Items As UInteger() = {723234, 721110, 721203, 721225, 721252, 722011, 722001, 722172}
    Public Shared Sub Hoang()
        Dim dlg As String() = Nothing
        If Player.Reborn = 1 Then
            Select Case OptionID
                Case 0
                    ReDim dlg(2)
                    dlg(0) = "TEXT It is time for you complete your 2nd rebirth, you must acquire the special items from "
                    dlg(1) = "TEXT my comrades turned evil. There is a total of 8 special items, good luck!"
                    dlg(2) = "OPTION1 I have the items."
                Case 1
                    Dim b As Boolean = True
                    Dim i As Integer
                    For i = 0 To _3605_Items.Length - 1
                        If Player.CountItems(_3605_Items(i)) = 0 Then
                            b = False
                            ReDim dlg(1)
                            dlg(0) = "TEXT It seems you are missing the item " & GetItemName(_3605_Items(i))
                            dlg(1) = "OPTION-1 I see."
                            Exit For
                        End If
                    Next
                    If b Then
                        For i = 0 To _3605_Items.Length - 1
                            Player.RemoveItems(_3605_Items(i), 1)
                        Next
                        Command("@item EXEMPTIONTOKEN FIXED")
                        ReDim dlg(0)
                        dlg(0) = "TEXT You have gotten an exemption token, give it Bryan to become 2nd reborn."
                    End If
            End Select
        Else
            ReDim dlg(1)
            dlg(0) = "TEXT Demons, bats, and giants are running wild here, you best get out while you can."
            dlg(1) = "OPTION-1 I see."
        End If
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
    Public Shared Sub Bryan()
        Dim dlg As String() = Nothing
        If Player.Reborn = 1 Then
            Select Case OptionID
                Case 0
                    ReDim dlg(3)
                    dlg(0) = "TEXT Do you wish to finally complete your 2nd Rebirth? "
                    dlg(1) = "TEXT If so, you must bring me the ExemptionToken first."
                    dlg(2) = "OPTION1 I have one"
                    dlg(3) = "OPTION-1 Ok."
                Case 1
                    ReDim dlg(6)
                    dlg(0) = "TEXT What class would you like to reborn into?"
                    dlg(1) = "OPTION12 Trojan"
                    dlg(2) = "OPTION22 Warrior"
                    dlg(3) = "OPTION42 Archer"
                    dlg(4) = "OPTION52 Ninja"
                    dlg(5) = "OPTION132 Water Taoist"
                    dlg(6) = "OPTION142 Fire Taoist"
                Case Else
                    If _300500_ValidJob(OptionID) Then
                        If Player.Level >= 120 Then
                            If Player.CountItems(723701) >= 1 Then ' Exemption Token
                                Player.RemoveItems(723701, 1)
                                Command("@2ndreborn " & OptionID.ToString())
                                Player.AddSession("NEW_REBORN2", True)
                            Else
                                ReDim dlg(1)
                                dlg(0) = "TEXT You seem to have forgotten the token, speak to Hoang to create one."
                                dlg(1) = "OPTION-1 Alright."
                            End If
                        Else
                            ReDim dlg(1)
                            dlg(0) = "TEXT Sorry, come back when you are 120, or above."
                            dlg(1) = "OPTION-1 I see"
                        End If
                    End If
            End Select
        Else
            ReDim dlg(1)
            dlg(0) = "TEXT Becarful around these parts, they can be quite dangerous."
            dlg(1) = "OPTION-1 I see."
        End If
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class