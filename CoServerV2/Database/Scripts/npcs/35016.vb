'#include ...\define_data.vb
' By: Hybrid
' Npc: 35016
' Name: WuxingOven

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub WuxingOven()
        Dim dlg As String() = Nothing

        Select Case OptionID
            Case 0
                ReDim dlg(2)
                dlg(0) = "TEXT Your equipment can be very important in combat."
                dlg(1) = "OPTION1 Upgrade Enchant"
                dlg(2) = "OPTION2 Upgrade Purity"
            Case 1
                Player.SendData(DataID.Switch, DataSwitchArg.EnchantWindow, 0, 0)
            Case 2
                Player.SendData(DataID.GUIDialog, DataGUIDialog.Composition, 0, 0)
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class