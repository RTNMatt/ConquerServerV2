'#include ...\define_data.vb
'#include ...\define_serverflags.vb
' By: Hybrid
' Npc: 44, 8, 10012, 10011, 10027, 10028
' Name: Warehouses

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Function CheckPassword() As Boolean
        If (Player.ServerFlags And ServerFlags.WarehouseOpen) = ServerFlags.WarehouseOpen Then
            Return True
        Else
            Dim Password As Integer = Player.WarehousePassword
            If Password = 0 Then
                Return True
            Else
                If Int32.Parse(Input) = Password Then
                    Player.ServerFlags = Player.ServerFlags Or ServerFlags.WarehouseOpen
                    Return True
                End If
            End If
        End If
        Return False
    End Function
    Public Shared Sub Warehouse()
        Dim dlg As String() = Nothing
        Select Case OptionID
            Case 0
                If CheckPassword() Then
                    Player.SendData(DataID.GUIDialog, DataGUIDialog.Warehouse, 0, 0)
                Else
                    ReDim dlg(1)
                    dlg(0) = "TEXT Please enter the correct warehouse password."
                    dlg(1) = "INPUT0 4"
                End If
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class