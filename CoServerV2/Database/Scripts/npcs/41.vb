'#include ...\define_item.vb
' By: Hybrid
' Npc: 41
' Name: ArtisanOu (Weapon Socket)

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Function _41_GetSocketDBPrice(ByVal hand As INpcItem) As Byte
        Dim Price As Byte = 0
        If hand.SocketOne = GemsConst.NoSocket Then
            Price = 1
        ElseIf hand.SocketTwo = GemsConst.NoSocket Then
            Price = 5
        End If
        Return Price
    End Function
    Public Shared Sub _41_GetSocketDialog(ByRef dlg As String())
        Dim hand As INpcItem = Player.GetEquipment(ItemPosition.Right + (OptionID - 1))
        If Not hand Is Nothing Then
            Dim Price As Byte = _41_GetSocketDBPrice(hand)
            If Price = 0 Then
                ReDim dlg(1)
                dlg(0) = "TEXT You already have both sockets open in this weapon."
                dlg(1) = "OPTION-1 I see."
            Else
                ReDim dlg(2)
                dlg(0) = "TEXT It will cost " & Price.ToString() & " dragonball(s) to open the next socket."
                dlg(1) = "OPTION" & hand.CurrentPosition.ToString() & " Here you are."
                dlg(2) = "OPTION-1 Nevermind."
            End If
        Else
            ReDim dlg(1)
            dlg(0) = "TEXT I can't place a socket in your arm now can I? Haha."
            dlg(1) = "OPTION-1 Oops!"
        End If
    End Sub
    Public Shared Sub _41_AwardWeaponSocket(ByRef dlg As String())
        Dim hand As INpcItem = Player.GetEquipment(OptionID)
        If Not hand Is Nothing Then
            Dim Price As Byte = _41_GetSocketDBPrice(hand)
            If Player.CountItems(ItemConst.Dragonball) >= Price Then
                Player.RemoveItems(ItemConst.DragonBall, Price)
                If Price = 1 Then
                    hand.SocketOne = GemsConst.OpenSocket
                ElseIf Price = 5 Then
                    hand.SocketTwo = GemsConst.OpenSocket
                End If
                hand.Send(Player)
                Player.SetEquipment(hand.CurrentPosition, hand)
            End If
        End If
    End Sub
    Public Shared Sub ArtisanOu()
        Dim dlg As String() = Nothing

        Select Case OptionID
            Case 0
                ReDim dlg(4)
                dlg(0) = "TEXT Hello, I am the great Artian Ou. I can socket your weapons. "
                dlg(1) = "TEXT Which hand would you like socketed?"
                dlg(2) = "OPTION1 Right"
                dlg(3) = "OPTION2 Left"
                dlg(4) = "OPTION-1 Neither"
            Case 1, 2
                _41_GetSocketDialog(dlg)
            Case ItemPosition.Left
            Case ItemPosition.Right
                _41_AwardWeaponSocket(dlg)
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class