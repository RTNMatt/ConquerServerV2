'#include ...\define_data.vb
'#include ...\define_string.vb
' By: Hybrid
' Npc: 390
' Name: Love Stone

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub LoveStone()
        Dim dlg As String() = Nothing

        Select Case OptionID
            Case 0
                ReDim dlg(3)
                dlg(0) = "TEXT I am the all mighty digital-pimp, what is it you wish?"
                dlg(1) = "OPTION1 Get married."
                dlg(2) = "OPTION2 Get divorced."
                dlg(3) = "OPTION-1 Nothing."
            Case 1
                Player.SendData(DataID.Switch, DataSwitchArg.MarriageMouse, 0, 0)
            Case 2
                Dim spouse As INpcPlayer = FindPlayerByName(Player.Spouse)
                If Not spouse Is Nothing Then
                    Player.SpouseAccount = ""
                    Player.Spouse = "None"
                    spouse.SpouseAccount = ""
                    spouse.Spouse = "None"

                    Player.SendString(StringID.Spouse, "None")
                    spouse.SendString(StringID.Spouse, "None")
                Else
                    Dim SpouseFile As String = DatabasePath & "\Accounts\" & Player.SpouseAccount & ".ini"
                    If String.Compare(QueryDatabase("Character", "Spouse", "", SpouseFile), Player.Account, True) Then
                        Player.SpouseAccount = ""
                        Player.Spouse = "None"
                        Player.SendString(StringID.Spouse, "None")

                        WriteDatabase("Character", "Spouse", "", SpouseFile)
                    End If
                End If
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class