'#include ...\define_item.vb

' By: Hybrid
' Npc: 10064
' Name: Tinter

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Function WearingShield() As Boolean
        Dim Left As INpcItem = Player.GetEquipment(ItemPosition.Left)
        If Not Left Is Nothing Then
            If Left.GetItemType() = 900 Then
                Return True
            End If
        End If
        Return False
    End Function
    Public Shared Sub Tinter()
        Dim dlg As String() = Nothing

        Select Case OptionID
            Case 0
                ReDim dlg(4)
                ' Note: The NOP command does nothing (no operator) when left intact
                dlg(0) = "TEXT I take it you are here to dye something. What do you wish to dye?"
                dlg(1) = "NOP" ' Headgear
                dlg(2) = "NOP" ' Armor
                dlg(3) = "NOP" ' Shield
                dlg(4) = "OPTION-1 Nothing"
                If Not Player.GetEquipment(ItemPosition.Head) Is Nothing Then
                    dlg(1) = "OPTION" & ItemPosition.Head.ToString() & " Headgear"
                End If
                If Not Player.GetEquipment(ItemPosition.Armor) Is Nothing Then
                    dlg(2) = "OPTION" & ItemPosition.Armor.ToString() & " Armor"
                End If
                If WearingShield() Then
                    dlg(3) = "OPTION" & ItemPosition.Left.ToString() & " Shield"
                End If
            Case ItemPosition.Head, ItemPosition.Armor, ItemPosition.Left
                Player.AddSession("DyeSlot", CUShort(OptionID))
                ReDim dlg(8)
                dlg(0) = "TEXT Select the color of your choice."
                dlg(1) = "OPTION12 Black"
                dlg(2) = "OPTION13 Orange"
                dlg(3) = "OPTION14 Cyan"
                dlg(4) = "OPTION15 Red"
                dlg(5) = "OPTION16 Blue"
                dlg(6) = "OPTION17 Yellow"
                dlg(7) = "OPTION18 Purple"
                dlg(8) = "OPTION19 White"
            Case 12 To 19
                If Player.VarExists("DyeSlot") Then
                    Dim DyeSlot As UShort = CUShort(Player.PopSession("DyeSlot"))
                    If DyeSlot <> ItemPosition.Left Or WearingShield() Then
                        Dim Item As INpcItem = Player.GetEquipment(DyeSlot)
                        If Not Item Is Nothing Then
                            Item.Color = CByte(OptionID - 10)
                            Item.Send(Player)
                            Player.SetEquipment(Item.CurrentPosition, Item)
                            Player.Respawn()
                        End If
                    End If
                End If
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class