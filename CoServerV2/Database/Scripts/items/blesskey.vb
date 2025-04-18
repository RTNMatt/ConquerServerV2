' By: Hybrid
' Item: 722312
' Name: BlessKey (2nd RB Quest Item)

Imports System
Imports ConquerScriptLinker

Partial Public Class ItemEngine
    Public Shared Function BlessKey() As Boolean
        Command("@mm 1070 192 194")
        Return True
    End Function
End Class