'#include ...\item.vb
' By: Hybrid
' Npc: Celestine, Eternity
' Name: 20005, 300500

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Function _20005_HasItems(ByRef dlg As String()) As Boolean
        Dim j As Integer
        For j = 0 To 6
            Dim gemid As UInteger = 700001 + (j * 10)
            If Not Player.CountItems(gemid) >= 1 Then
                ReDim dlg(1)
                dlg(0) = "TEXT You seem to be missing the item, " & GetItemName(gemid)
                dlg(1) = "OPTION-1 I see."
                Return False
            End If
        Next
        If Not Player.CountItems(721258) >= 1 Then ' clean water
            ReDim dlg(1)
            dlg(0) = "TEXT You seem to be missing the item, " & GetItemName(721258)
            dlg(1) = "OPTION-1 I see."
            Return False
        End If
        Return True
    End Function
    Public Shared Sub _20005_RemoveItems()
        Dim j As Integer
        For j = 0 To 6
            Dim gemid As UInteger = 700001 + (j * 10)
            Player.RemoveItems(gemid, 1)
        Next
        Player.RemoveItems(721258, 1)
    End Sub
    Public Shared Sub Celestine()
        Dim dlg As String() = Nothing
        Select Case OptionID
            Case 0
                ReDim dlg(4)
                dlg(0) = "TEXT Hello, my name is Celestine. Ah, I suppose your wanting me to make you a Celestial Stone? "
                dlg(1) = "TEXT Well, I don't mind making one for you but I'll need some things."
                dlg(2) = "OPTION1 I have everything"
                dlg(3) = "OPTION2 Such as?"
                dlg(4) = "OPTION-1 No thanks"
            Case 1
                If _20005_HasItems(dlg) Then
                    _20005_RemoveItems()
                    Command("@item CELESTIALSTONE SUPER")
                End If
            Case 2
                ReDim dlg(2)
                dlg(0) = "TEXT You will need one normal gem of every gem type (other than glory, thunder & tortoise gems)."
                dlg(1) = "TEXT I will also need a CleanWater, good luck in acquring these items."
                dlg(2) = "OPTION-1 Thanks!"
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
    Public Shared Function _300500_ValidJob(ByVal Job As Byte) As Boolean
        If Job Mod 10 = 2 Then
            Dim jType As Byte = CByte(Job / 10)
            Select Case jType
                Case 1
                    Return True
                Case 2
                    Return True
                Case 4
                    Return True
                Case 5
                    Return True
                Case 13
                    Return True
                Case 14
                    Return True
            End Select
        End If
        Return False
    End Function
    Public Shared Function _300500_MakeGemID(ByVal gType As Byte) As UInteger
        Return 700003 + (gType * 10)
    End Function
    Public Shared Sub Eternity()
        Dim dlg As String() = Nothing
        Select Case OptionID
            Case 0
                If Player.Reborn = 0 Then
                    ReDim dlg(2)
                    dlg(0) = "TEXT You must be here to get reborn, why else would you travel so far?"
                    dlg(1) = "OPTION1 Absolutely!"
                    dlg(2) = "OPTION-1 Just passing."
                Else
                    ReDim dlg(1)
                    dlg(0) = "TEXT Nice day isn't it?"
                    dlg(1) = "OPTION-1 Very."
                End If
            Case 1
                ReDim dlg(6)
                dlg(0) = "TEXT What class would you like to reborn into?"
                dlg(1) = "OPTION12 Trojan"
                dlg(2) = "OPTION22 Warrior"
                dlg(3) = "OPTION42 Archer"
                dlg(4) = "OPTION52 Ninja"
                dlg(5) = "OPTION132 Water Taoist"
                dlg(6) = "OPTION142 Fire Taoist"
            Case 2 To 9
                If Not Player.PopSession("NEW_REBORN") Is Nothing Then
                    Command("@item " & GetItemName(_300500_MakeGemID(OptionID - 2)) & " NormalV1")
                End If
            Case Else
                Dim ReqLevel As Short = 120
                If Player.Job >= 132 And Player.Job <= 135 Then
                    ReqLevel = 110
                End If
                If _300500_ValidJob(OptionID) And Player.Reborn = 0 Then
                    If Player.Level >= ReqLevel Then
                        If Player.CountItems(721259) >= 1 Then ' celestial stone
                            Player.RemoveItems(721259, 1)
                            Command("@reborn " & OptionID.ToString())
                            Player.AddSession("NEW_REBORN", True)

                            Dim j As Integer
                            ReDim dlg(8)
                            dlg(0) = "TEXT Now, you may select a super gem of your choice as a reward."
                            For j = 0 To 7
                                dlg(1 + j) = "OPTION" & (2 + j).ToString() & " " & GetItemName(_300500_MakeGemID(j))
                            Next
                        Else
                            ReDim dlg(1)
                            dlg(0) = "TEXT You seem to have forgotten a Celestial stone. Celestine in Twincity (365, 92) can make one."
                            dlg(1) = "OPTION-1 I shall seek her."
                        End If
                Else
                    ReDim dlg(1)
                    dlg(0) = "TEXT Sorry, come back when you are 120, or 110 and a water taoist, or above."
                    dlg(1) = "OPTION-1 I see"
                End If
                End If
        End Select
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class