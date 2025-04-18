'#include ...\item_const.vb
'#include ...\skill.vb
' By: Hybrid
' The entire job center!

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Structure LearnSpellData
        Public SpellID As UShort
        Public ReqLevel As UShort
        Public OptionID As Byte
        Public Sub New(ByVal spell As UShort, ByVal level As UShort, ByVal opt As Byte)
            SpellID = spell
            ReqLevel = level
            OptionID = opt
        End Sub
    End Structure
    Public Shared Function SelectSpell(ByVal Spells As LearnSpellData()) As Integer
        Dim i As Integer
        For i = 0 To Spells.Length - 1
            If Spells(i).OptionID = OptionID Then
                Return i
            End If
        Next
        Return -1
    End Function
    Public Shared Sub ShowSpells(ByRef Dlg As String(), ByVal Spells As LearnSpellData())
        ReDim dlg(Spells.Length)
        dlg(0) = "TEXT What spells do you wish to learn?"
        Dim i As Integer
        For i = 0 To Spells.Length - 1
            Dim data As LearnSpellData = Spells(i)
            dlg(i + 1) = "OPTION" & data.OptionID & " " & GetSpellName(data.SpellID) & " (Level " & data.ReqLevel.ToString() & ")"
        Next
    End Sub
    Public Shared Function VerifyJob(ByVal JobStart As Byte, ByVal JobEnd As Byte) As Boolean
        If Player.Job >= JobStart And Player.Job <= JobEnd Then
            Return True
        Else
            Dim dlg(1) As String
            dlg(0) = "TEXT I'm sorry, but you are not of the required job."
            dlg(1) = "OPTION-1 Oh..."
            Dialog(dlg)
            Return False
        End If
    End Function
    Public Shared Function MaxedOutSpell(ByVal SpellID As UShort) As Boolean
        Dim spell As INpcSkill = Player.GetSpell(SpellID)
        If Not Spell Is Nothing Then
            Return Spell.MaxLevel
        End If
        Return False
    End Function
    Public Shared Sub FailedRequirements(ByVal Reqs As String)
        Dim dlg(2) As String
        dlg(0) = "TEXT I'm sorry, but you do not meet all the requirements such as "
        dlg(1) = "TEXT " & Reqs & "."
        dlg(2) = "OPTION-1 I see."
        Dialog(dlg)
    End Sub
    Public Shared Sub GetPromoted(ByVal newJob As Byte)
        Dim JobClass As Byte = CByte(newJob / 10)
        Dim JobNumber As Byte = CByte(newJob Mod 10)

        Select Case JobNumber
            Case 1
                If Player.Level >= 15 Then
                    Player.Job = newJob
                    Select Case JobClass
                        Case 1 ' Trojan
                            LearnSpell(1110, 0) ' Cyclone
                            LearnSpell(1015, 0) ' Accuracy
                        Case 2
                            LearnSpell(1015, 0) ' Accuracy
                            LearnSpell(1020, 0) ' XP Shield
                            LearnSpell(1025, 0) ' Superman
                            LearnSpell(1040, 0) ' Roar
                        Case 4 ' Archer
                            LearnSpell(8002, 0) ' XP Fly
                        Case 10 ' Taoist
                            LearnSpell(1010, 0) ' Lightning (XP)
                    End Select
                Else
                    FailedRequirements("being level 15, or above")
                End If
            Case 2
                If Player.Level >= 40 Then
                    Dim HasOres As Boolean = True
                    If JobClass = 4 Then
                        HasOres = (Player.CountItems(ItemConst.EuxeniteOre) >= 5)
                        If HasOres Then
                            Player.RemoveItems(ItemConst.EuxeniteOre, 5)
                        End If
                    End If

                    If HasOres Then
                        Player.Job = newJob
                        Select Case JobClass
                            Case 13 ' Water Taoist
                                LearnSpell(1050, 0) ' Revive (XP)
                                LearnSpell(1055, 0) ' Healing Rain
                            Case 14 ' Fire Taoist
                                LearnSpell(1125, 0) ' Volcano (XP)
                            Case 1 ' Trojan
                                LearnSpell(1270, 0) ' Golem
                                LearnSpell(1115, 0) ' Hercules
                                LearnSpell(1190, 0) ' Spirt Healing
                            Case 2 ' Warrior
                                LearnSpell(1320, 0) ' Flying Moon
                            Case 5 ' Ninja
                                LearnSpell(6000, 0) ' TwoFoldBlade
                        End Select
                    Else
                        FailedRequirements("having 5 Euxenite Ores")
                    End If
                Else
                    FailedRequirements("being level 40, or above")
                End If
            Case 3
                If Player.Level >= 70 Then
                    If Player.CountItems(ItemConst.Emerald) >= 1 Then
                        Player.RemoveItems(ItemConst.Emerald, 1)
                        Player.Job = newJob
                        Select Case JobClass
                            Case 13 ' Water Taoist
                                LearnSpell(1100, 0) ' Pray (Revive)
                            Case 4 ' Archer
                                LearnSpell(8003, 0) ' Fly (Non-XP)
                            Case 5 ' Ninja
                                LearnSpell(6001, 0) ' Toxic Fog
                                LearnSpell(6002, 0) ' Poison Star
                                LearnSpell(6010, 0) ' Shuriken Vortex
                        End Select
                    Else
                        FailedRequirements("having an Emerald")
                    End If
                Else
                    FailedRequirements("being level 70, or above")
                End If
            Case 4
                If Player.Level >= 100 Then
                    If Player.CountItems(ItemConst.Meteor) >= 1 Then
                        Player.RemoveItems(ItemConst.Meteor, 1)
                        Player.Job = newJob
                    End If
                Else
                    FailedRequirements("being level 100, or above")
                End If
            Case 5
                If Player.Level >= 110 Then
                    If Player.CountItems(ItemConst.MoonBox) >= 1 Then
                        Player.RemoveItems(ItemConst.MoonBox, 1)
                        Player.Job = newJob
                        Select Case JobClass
                            Case 5 ' Ninja
                                LearnSpell(6004, 0) ' Archer Bane
                        End Select
                    End If
                End If
        End Select
    End Sub
    Public Shared TrojanSpells As LearnSpellData() = {New LearnSpellData(1010, 1, 3), New LearnSpellData(1015, 1, 4), New LearnSpellData(1115, 40, 5), New LearnSpellData(1190, 40, 6)}
    Public Shared Sub TrojanStar()
        Dim dlg As String() = Nothing

        If VerifyJob(10, 15) Then
            Select Case OptionID
                Case 0
                    ReDim dlg(3)
                    dlg(0) = "TEXT I am the TrojanStar, I can assist you through your life as trojan. How may I help you?"
                    dlg(1) = "OPTION1 Get Promoted."
                    dlg(2) = "OPTION2 Learn Spells."
                    dlg(3) = "OPTION-1 Just Passing."
                Case 1
                    GetPromoted(Player.Job + 1)
                Case 2
                    ShowSpells(dlg, TrojanSpells)
                Case 3 To 6
                    Dim idx As Integer = SelectSpell(TrojanSpells)
                    If idx <> -1 Then
                        Dim data As LearnSpellData = TrojanSpells(idx)
                        If Player.Level >= data.ReqLevel Then
                            LearnSpell(data.SpellID, 0)
                        Else
                            ReDim dlg(1)
                            dlg(0) = "TEXT Sorry, you are not of a high enough level to learn this spell."
                            dlg(1) = "OPTION-1 I see."
                        End If
                    End If
            End Select
        End If
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
    Public Shared Sub WindSage()
        Dim dlg As String() = Nothing

        If VerifyJob(50, 55) Then
            Select Case OptionID
                Case 0
                    ReDim dlg(2)
                    dlg(0) = "TEXT I am the WindSage, I can assist you through your life as ninja. How may I help you?"
                    dlg(1) = "OPTION1 Get Promoted."
                    dlg(2) = "OPTION-1 Just Passing."
                    ' This NPC teaches all it's spells when getting promoted.
                Case 1
                    GetPromoted(Player.Job + 1)
            End Select
        End If
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
    Public Shared WarriorSpells As LearnSpellData() = {New LearnSpellData(1015, 1, 3), New LearnSpellData(1020, 1, 4), New LearnSpellData(1025, 1, 5), New LearnSpellData(1040, 15, 6), New LearnSpellData(1320, 40, 7)}
    Public Shared Sub WarriorGod()
        Dim dlg As String() = Nothing

        If VerifyJob(20, 25) Then
            Select Case OptionID
                Case 0
                    ReDim dlg(3)
                    dlg(0) = "TEXT I am the WarriorGod, I can assist you through your life as warrior. How may I help you?"
                    dlg(1) = "OPTION1 Get Promoted."
                    dlg(2) = "OPTION2 Learn Spells."
                    dlg(3) = "OPTION-1 Just Passing."
                    ' This NPC teaches all it's spells when getting promoted.
                Case 1
                    GetPromoted(Player.Job + 1)
                Case 2
                    ShowSpells(dlg, WarriorSpells)
                Case 3 To 7
                    Dim idx As Integer = SelectSpell(WarriorSpells)
                    If idx <> -1 Then
                        Dim data As LearnSpellData = WarriorSpells(idx)
                        If Player.Level >= data.ReqLevel Then
                            LearnSpell(data.SpellID, 0)
                        Else
                            ReDim dlg(1)
                            dlg(0) = "TEXT Sorry, you are not of a high enough level to learn this spell."
                            dlg(1) = "OPTION-1 I see."
                        End If
                    End If
            End Select
        End If
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
    Public Shared ArcherSpells As LearnSpellData() = {New LearnSpellData(8001, 23, 3), New LearnSpellData(8000, 46, 4), New LearnSpellData(9000, 71, 5), New LearnSpellData(8002, 15, 6), New LearnSpellData(8003, 70, 7)}
    Public Shared Sub ArcherGod()
        Dim dlg As String() = Nothing
        If VerifyJob(40, 45) Then
            Select Case OptionID
                Case 0
                    ReDim dlg(3)
                    dlg(0) = "TEXT I am the ArcherGod, I can assist you through your life as archer. How may I help you?"
                    dlg(1) = "OPTION1 Get Promoted."
                    dlg(2) = "OPTION2 Learn Skills."
                    dlg(3) = "OPTION-1 Just Passing."
                Case 1
                    GetPromoted(Player.Job + 1)
                Case 2
                    ShowSpells(dlg, ArcherSpells)
                Case 3 To 7
                    Dim idx As Integer = SelectSpell(ArcherSpells)
                    If idx <> -1 Then
                        Dim data As LearnSpellData = ArcherSpells(idx)
                        If Player.Level >= data.ReqLevel Then
                            LearnSpell(data.SpellID, 0)
                        Else
                            ReDim dlg(1)
                            dlg(0) = "TEXT Sorry, you are not of a high enough level to learn this spell."
                            dlg(1) = "OPTION-1 I see."
                        End If
                    End If
            End Select
        End If
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
    Public Shared FireTaoSpells As LearnSpellData() = {New LearnSpellData(1001, 40, 11), New LearnSpellData(1002, 40, 12), New LearnSpellData(1125, 40, 13)}
    Public Shared WaterTaoSpells As LearnSpellData() = {New LearnSpellData(1050, 40, 8), New LearnSpellData(1055, 40, 9), New LearnSpellData(1100, 70, 10)}
    Public Shared TaoistSpells As LearnSpellData() = {New LearnSpellData(1000, 1, 14), New LearnSpellData(1195, 44, 15), New LearnSpellData(1010, 1, 16)}
    Public Shared Sub TaoistMoon()
        Dim dlg As String() = Nothing
        If VerifyJob(100, 145) Then
            Select Case OptionID
                Case 0
                    ReDim dlg(3)
                    dlg(0) = "TEXT I am the TaoistMoon, I can assist you through your life as Taoist. How may I help you?"
                    dlg(1) = "OPTION1 Get Promoted."
                    dlg(2) = "OPTION4 Learn Skills."
                    dlg(3) = "OPTION-1 Just Passing."
                Case 1
                    If Player.Job = 101 Then
                        ReDim dlg(2)
                        dlg(0) = "TEXT The time has come, you must chose whether you will be a water taoist, or a fire taoist."
                        dlg(1) = "OPTION2 Water Taoist"
                        dlg(2) = "OPTION3 Fire Taoist"
                    Else
                        GetPromoted(Player.Job + 1)
                    End If
                Case 2 To 3
                    If Player.Job = 101 Then
                        If OptionID = 2 Then
                            GetPromoted(132)
                        ElseIf OptionID = 3 Then
                            GetPromoted(142)
                        End If
                    End If
                Case 4
                    ReDim dlg(3)
                    dlg(0) = "TEXT What kind of spells would you like to learn?"
                    dlg(1) = "OPTION5 Water Taoist"
                    dlg(2) = "OPTION6 Fire Taoist"
                    dlg(3) = "OPTION7 Taoist"
                Case 5
                    If Player.Job >= 132 And Player.Job <= 135 Then
                        ShowSpells(dlg, WaterTaoSpells)
                    Else
                        ReDim dlg(1)
                        dlg(0) = "TEXT I have no spells to teach you at this time."
                        dlg(1) = "OPTION-1 I see."
                    End If
                Case 6
                    If Player.Job >= 142 And Player.Job <= 145 Then
                        ShowSpells(dlg, FireTaoSpells)
                    Else
                        ReDim dlg(1)
                        dlg(0) = "TEXT I have no spells to teach you at this time."
                        dlg(1) = "OPTION-1 I see."
                    End If
                Case 7
                    ShowSpells(dlg, TaoistSpells)
                Case 8 To 10
                    If Player.Job >= 132 And Player.Job <= 135 Then
                        Dim idx As Integer = SelectSpell(WaterTaoSpells)
                        If idx <> -1 Then
                            Dim data As LearnSpellData = WaterTaoSpells(idx)
                            If Player.Level >= data.ReqLevel Then
                                LearnSpell(data.SpellID, 0)
                            Else
                                ReDim dlg(1)
                                dlg(0) = "TEXT Sorry, you are not of a high enough level to learn this spell."
                                dlg(1) = "OPTION-1 I see."
                            End If
                        End If
                    Else
                        ReDim dlg(1)
                        dlg(0) = "TEXT Sorry, you cannot learn these spells yet."
                        dlg(1) = "OPTION-1 I see."
                    End If
                Case 14 To 16
                    Dim idx As Integer = SelectSpell(TaoistSpells)
                    If idx <> -1 Then
                        Dim data As LearnSpellData = TaoistSpells(idx)
                        If Player.Level >= data.ReqLevel Then
                            LearnSpell(data.SpellID, 0)
                        Else
                            ReDim dlg(1)
                            dlg(0) = "TEXT Sorry, you are not of a high enough level to learn this spell."
                            dlg(1) = "OPTION-1 I see."
                        End If
                    End If
                Case 11 To 13
                    If Player.Job >= 142 And Player.Job <= 145 Then
                        Dim idx As Integer = SelectSpell(FireTaoSpells)
                        If idx <> -1 Then
                            Dim data As LearnSpellData = FireTaoSpells(idx)
                            If Player.Level >= data.ReqLevel Then
                                If MaxedOutSpell(data.SpellID - 1) Or (Not (data.SpellID >= 1000 And data.SpellID <= 1002)) Then
                                    LearnSpell(data.SpellID, 0)
                                Else
                                    ReDim dlg(1)
                                    dlg(0) = "TEXT You must first level " & GetSpellName(data.SpellID - 1) & " to it's highest level first."
                                    dlg(1) = "OPTION-1 I see."
                                End If
                            Else
                                ReDim dlg(1)
                                dlg(0) = "TEXT Sorry, you are not of a high enough level to learn this spell."
                                dlg(1) = "OPTION-1 I see."
                            End If
                        End If

                    Else
                        ReDim dlg(1)
                        dlg(0) = "TEXT Sorry, you cannot learn these spells yet."
                        dlg(1) = "OPTION-1 I see."
                    End If
            End Select
        End If
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
    Public Shared MonkSpells As LearnSpellData() = {New LearnSpellData(8001, 23, 3), New LearnSpellData(8000, 46, 4), New LearnSpellData(9000, 71, 5), New LearnSpellData(8002, 15, 6), New LearnSpellData(8003, 70, 7)}
    Public Shared Sub HeadAbbot()
        Dim dlg As String() = Nothing
        If VerifyJob(60, 65) Then
            Select Case OptionID
                Case 0
                    ReDim dlg(3)
                    dlg(0) = "TEXT I am the HeadAbbot, I can assist you through your life as Monk. How may I help you?"
                    dlg(1) = "OPTION1 Get Promoted."
                    dlg(2) = "OPTION2 Learn Skills."
                    dlg(3) = "OPTION-1 Just Passing."
                Case 1
                    GetPromoted(Player.Job + 1)
                Case 2
                    ShowSpells(dlg, MonkSpells)
                Case 3 To 7
                    Dim idx As Integer = SelectSpell(MonkSpells)
                    If idx <> -1 Then
                        Dim data As LearnSpellData = MonkSpells(idx)
                        If Player.Level >= data.ReqLevel Then
                            LearnSpell(data.SpellID, 0)
                        Else
                            ReDim dlg(1)
                            dlg(0) = "TEXT Sorry, you are not of a high enough level to learn this spell."
                            dlg(1) = "OPTION-1 I see."
                        End If
                    End If
            End Select
        End If
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
    Public Shared Sub JobCenter()
        Select Case NpcID
            Case 10022
                TrojanStar()
            Case 400
                ArcherGod()
            Case 10001
                WarriorGod()
            Case 10000
                TaoistMoon()
            Case 8314
                HeadAbbot() 'Monk
        End Select
    End Sub
End Class