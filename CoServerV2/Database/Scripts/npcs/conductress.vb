' By: Wolf
' Npc: 10050
' Name: Conductress

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub Teleport(ByRef dlg As String(), ByVal cmd As String)
        If Player.Money >= 100 Then
            Player.Money -= 100
            Command(cmd)
        Else
            ReDim dlg(1)
            dlg(0) = "TEXT You do not have 100 silvers, get rich bitch."
            dlg(1) = "OPTION-1 Well, fuck you, I'll walk."
        End If
    End Sub
    Public Shared Sub Conductress_MA()
        Dim dlg As String() = Nothing
        Select Case OptionID
            Case 0
                ReDim dlg(2)
                dlg(0) = "TEXT Would you like to leave the market?"
                dlg(1) = "OPTION1 Yes"
                dlg(2) = "OPTION-1 No thanks"
            Case 1
                Command("@mm " & Player.LastMapID.ToString() & " " & Player.LastX.ToString() & " " & Player.LastY.ToString())
        End Select
        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
    Public Shared Sub Conductress_TC()
        Dim dlg As String() = Nothing
        Select Case OptionID
            Case 0
                ReDim dlg(6)
                dlg(0) = "TEXT Where are you heading for? I can teleport you for the price of 100 silver."
                dlg(1) = "OPTION1 Phoenix Castle"
                dlg(2) = "OPTION2 Desert City"
                dlg(3) = "OPTION3 Ape Mountain"
                dlg(4) = "OPTION4 Bird Island"
                dlg(5) = "OPTION5 Mine Cave"
                dlg(6) = "OPTION6 Market"

            Case 1
                Teleport(dlg, "@mm 1002 958 555")
            Case 2
                Teleport(dlg, "@mm 1002 069 473")
            Case 3
                Teleport(dlg, "@mm 1002 555 957")
            Case 4
                Teleport(dlg, "@mm 1002 232 190")
            Case 5
                Teleport(dlg, "@mm 1002 053 399")
            Case 6
                Teleport(dlg, "@mm 1036 213 195")
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class