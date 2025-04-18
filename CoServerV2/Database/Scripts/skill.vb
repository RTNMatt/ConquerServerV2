Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub LearnSpell(ByVal Spell As UShort, ByVal Level As UShort)
        If Player.GetSpell(Spell) Is Nothing Then
            Command("@spell " & Spell.ToString() & " " & Level.ToString())
        End If
    End Sub
    Public Shared Function GetSpellName(ByVal SpellID As UShort) As String
        Dim path As String = "\Spells\" + SpellID.ToString() + "[0].ini"
        Return QueryDatabase("SpellInformation", "Name", "ERROR", path)
    End Function
End Class