' Scripted by J0K3R
' Npc: Barber
' MapID: 1002 416 379

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub Barber()
        Dim dlg As String() = Nothing

        Select Case OptionID
            Case 0
                ReDim dlg(5)
                dlg(0) = "TEXT Hi, I can offer you 3 types of hairstyles, New Styles, Nostalgic Styles and last but not least Special Styles."
                dlg(1) = "TEXT Would you like to make a payement of 500 silvers to change your hairstyle?"
                dlg(2) = "OPTION1 Yes, New Styles"
                dlg(3) = "OPTION3 Yes, Nostalgic Styles"
                dlg(4) = "OPTION4 Yes, Special Styles"
                dlg(5) = "OPTION-1 No thanks, Just passing by."
            Case 1
                ReDim dlg(8)
                dlg(0) = "TEXT Which New HairStyle would you like?"
                dlg(1) = "OPTION30 New Hairstyle 1"
                dlg(2) = "OPTION31 New HairStyle 2"
                dlg(3) = "OPTION32 New HairStyle 3"
                dlg(4) = "OPTION33 New HairStyle 4"
                dlg(5) = "OPTION34 New HairStyle 5"
                dlg(6) = "OPTION35 New HairStyle 6"
                dlg(7) = "OPTION36 New HairStyle 7"
                dlg(8) = "OPTION2 Next"
            Case 2
                ReDim dlg(6)
                dlg(0) = "TEXT Which NewHairStyle would you like?"
                dlg(1) = "OPTION37 New HairStyle 1"
                dlg(2) = "OPTION38 New HairStyle 2"
                dlg(3) = "OPTION39 New HairStyle 3"
                dlg(4) = "OPTION40 New HairStyle 4"
                dlg(5) = "OPTION41 New HairStyle 5"
                dlg(6) = "OPTION0 Back"
            Case 3
                ReDim dlg(8)
                dlg(0) = "TEXT Which Nostalgic would you like?"
                dlg(1) = "OPTION11 Nostalgic 1"
                dlg(2) = "OPTION12 Nostalgic 2"
                dlg(3) = "OPTION13 Nostalgic 3"
                dlg(4) = "OPTION14 Nostalgic 4"
                dlg(5) = "OPTION15 Nostalgic 5"
                dlg(6) = "OPTION16 Nostalgic 6"
                dlg(7) = "OPTION17 Nostalgic 7"
                dlg(8) = "OPTION0 Back"
            Case 4
                ReDim dlg(6)
                dlg(0) = "TEXT Which Special HairStyle would you like?"
                dlg(1) = "OPTION21 Special HairStyle 1"
                dlg(2) = "OPTION22 Special HairStyle 2"
                dlg(3) = "OPTION23 Special HairStyle 3"
                dlg(4) = "OPTION24 Special HairStyle 4"
                dlg(5) = "OPTION25 Special HairStyle 5"
                dlg(6) = "OPTION0 Back"
            Case Else
                If Player.Money >= 500 Then
                    If OptionID >= 30 And OptionID <= 41 Then
                        ReDim dlg(1)
                        dlg(0) = "TEXT There you go, Your 'New' hairstyle as requested."
                        dlg(1) = "OPTION-1 Thank You."
                    ElseIf OptionID >= 11 And OptionID <= 17 Then
                        ReDim dlg(1)
                        dlg(0) = "TEXT There you go, Your 'Nostalgic' hairstyle as requested."
                        dlg(1) = "OPTION-1 Thank you."
                    ElseIf OptionID >= 21 And OptionID <= 25 Then
                        ReDim dlg(1)
                        dlg(0) = "TEXT There you go, Your 'Special' hairstyle as requested."
                        dlg(1) = "OPTION-1 Thank  You."
                    Else
                        ReDim dlg(1)
                        dlg(0) = "TEXT There you go, Your hairstyle as requested."
                        dlg(1) = "OPTION-1 Thank You."
                    End If
                    Player.Hairstyle = (CInt(Player.Hairstyle / 100) * 100) + OptionID
                End If
        End Select

        If Not dlg Is Nothing Then
            Dialog(dlg)
        End If
    End Sub
End Class