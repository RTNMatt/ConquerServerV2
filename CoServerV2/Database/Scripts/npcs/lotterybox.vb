'#include ...\define_data.vb
'#include ...\define_serverflags.vb
' By: Hybrid
' Npc: All on MapID - 700

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared RedBoxes As UInt32() = {938, 936, 930, 925}
    Public Shared OrangeBoxes As UInt32() = {945, 933, 943, 927}
    Public Shared YellowBoxes As UInt32() = {937, 942, 939, 931}
    Public Shared BlueBoxes As UInt32() = {935, 944, 932, 928}
    Public Shared TealBoxes As UInt32() = {929, 934, 926, 940}
    Public Shared Function Contains(ByVal Boxes As UInt32()) As Boolean
        Dim i As Integer
        For i = 0 To Boxes.Length - 1
            If Boxes(i) = NpcID Then
                Return True
            End If
        Next
        Return False
    End Function
    Public Shared Function GetCurrentBoxID() As Int32
        If Contains(RedBoxes) Then
            Return 0
        ElseIf Contains(OrangeBoxes) Then
            Return 1
        ElseIf Contains(YellowBoxes) Then
            Return 2
        ElseIf Contains(BlueBoxes) Then
            Return 3
        ElseIf Contains(TealBoxes) Then
            Return 4
        End If
        Return -1
    End Function
    Public Shared Sub LotteryBox()
        Dim dlg As String() = Nothing
        If Not (Player.ServerFlags And ServerFlags.GotLotteryItem) = ServerFlags.GotLotteryItem Then
            If Player.InventorySpace >= 1 Then
                Player.GiveItem(GenerateLotteryItem(GetCurrentBoxID()))
                Player.ServerFlags = Player.ServerFlags Or ServerFlags.GotLotteryItem
            Else
                ReDim dlg(1)
                dlg(0) = "TEXT Please make some space in your inventory first"
                dlg(1) = "OPTION-1 I see"
            End If
        End If
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class