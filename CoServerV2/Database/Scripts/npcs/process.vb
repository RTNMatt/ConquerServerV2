'#includedir ...\npcs\*.vb

Imports System
Imports ConquerScriptLinker

Partial Public Class NpcEngine
    Public Shared Sub Execute()
        Select Case NpcID
            Case 8, 44, 10011, 10012, 10027, 10028
                Warehouse()
            Case 41
                ArtisanOu()
            Case 20005
                Celestine()
            Case 300500
                Eternity()
            Case 380
                GuildController()
            Case 3604
                Hoang()
            Case 3602
                Bryan()
            Case 3600
                Alex()
            Case 30164
                Mike()
            Case 390
                LoveStone()
            Case 400, 10000, 10001, 10022, 8314
                JobCenter()
            Case 923
                LadyLuckEnter()
            Case 924
                LadyLuckLeaving()
            Case 925 To 945
                LotteryBox()
            Case 2071
                CPAdmin()
            Case 5004
                MillionaireLee()
            Case 10002
                Barber()
            Case 10003
                GuildDirector()
            Case 10021
                ArenaGuard()
            Case 10050
                Conductress_TC()
            Case 45
                Conductress_MA()
            Case 10054
                GeneralPeace()
            Case 10063
                Shopboy()
            Case 10064
                Tinter()
            Case 35015
                Ethereal()
            Case 35016
                WuxingOven()
            Case 104787, 104833, 104839, 104845, 104851
                Boxer()
            Case 108883, 121171
                GuildGate()
            Case 350050
                CelestialTao()
            Case 600075
                BoxerLeave()
        End Select
    End Sub
End Class