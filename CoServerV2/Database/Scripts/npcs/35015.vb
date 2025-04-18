'#include ...\define_item.vb
' By: Hybrid
' Npc: 35015
' Name: Ethereal

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Function _35015_TortoiseCount(ByVal Position As UShort) As SByte
        Dim item As INpcItem = Player.GetEquipment(Position)
        If Not item Is Nothing Then
            If item.Bless >= 0 And item.Bless <= 2 Then
                Return 1
            ElseIf item.Bless >= 3 And item.Bless <= 4 Then
                Return 3
            ElseIf item.Bless >= 5 And item.Bless <= 6 Then
                Return 5
            End If
        End If
        Return -1
    End Function
    Public Shared Sub Ethereal()
        Dim dlg As String() = Nothing
        Select Case OptionID
            Case 0
                ReDim dlg(4)
                dlg(0) = "TEXT Hello there, my name is Ethereal. I assume you've come to this part of the market for some reason. "
                dlg(1) = "TEXT Ah! I know, you must be here for my blessing skills. I smelt super tortoise gems into your weapons to increase "
                dlg(2) = "TEXT their blessing value, how about it?"
                dlg(3) = "OPTION10 Bless my gear."
                dlg(4) = "OPTION-1 Just passing."
            Case 1 To 6, 8
                Dim torts As SByte = _35015_TortoiseCount(OptionID)
                If torts <> -1 Then
                    ReDim dlg(2)
                    dlg(0) = "TEXT It will cost " & torts.ToString() & " super-tortoise gem(s) to upgrade the bless of this item, is this ok?"
                    dlg(1) = "OPTION11 Bless Upgrade"
                    dlg(2) = "OPTION-1 Never mind"

                    Player.AddSession("BLESS_SLOT", CUShort(OptionID))
                Else
                    ReDim dlg(1)
                    dlg(0) = "TEXT This slot is empty, or cannot be upgraded any furthur."
                    dlg(1) = "OPTION-1 I see."
                End If
            Case 10
                ReDim dlg(7)
                dlg(0) = "TEXT So what gear is it you wish to be blessed?"
                dlg(1) = "OPTION1 Headgear"
                dlg(2) = "OPTION2 Necklace"
                dlg(3) = "OPTION3 Armor"
                dlg(4) = "OPTION4 Righthand"
                dlg(5) = "OPTION5 Lefthand"
                dlg(6) = "OPTION6 Ring"
                dlg(7) = "OPTION8 Boots"
            Case 11
                Dim obj_Position As Object = Player.PopSession("BLESS_SLOT")
                If Not obj_Position Is Nothing Then
                    Dim Position As UShort = CUShort(obj_Position)
                    Dim item As INpcItem = Player.GetEquipment(Position)
                    Dim count As Integer = Player.CountItems(700073) ' STGs
                    Dim need As SByte = _35015_TortoiseCount(Position)
                    If count >= need Then
                        Player.RemoveItems(700073, need)
                        If item.Bless >= 0 And item.Bless <= 2 Then
                            item.Bless = 3
                        ElseIf item.Bless >= 3 And item.Bless <= 4 Then
                            item.Bless = 5
                        ElseIf item.Bless >= 5 And item.Bless <= 6 Then
                            item.Bless = 7
                        End If
                        item.Send(Player)
                        Player.SetEquipment(item.CurrentPosition, item)
                    Else
                        ReDim dlg(1)
                        dlg(0) = "TEXT I'm sorry you only have " & count.ToString() & " super-tortoise gems, and you need " & need.ToString()
                        dlg(1) = "OPTION-1 I see."
                    End If
                End If
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class