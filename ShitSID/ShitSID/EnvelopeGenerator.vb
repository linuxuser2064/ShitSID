Public Class EnvelopeGenerator

    ' this is a conversion from the reSIDfp EnvelopeGenerator class
    ' reSIDfp is licensed under the GPLv2
    ' Copyright 2011-2020 Leandro Nini <drfiemost@users.sourceforge.net>
    ' Copyright 2018 VICE Project
    ' Copyright 2007-2010 Antti Lankila
    ' Copyright 2004,2010 Dag Lem <resid@nimrod.no>

    ' --- SID ADSR State ---
    Public Enum EState
        Attack
        DecaySustain
        Release
    End Enum

    Private Shared ReadOnly adsrTable As UInteger() = {
        &H7F, &H3000, &H1E00, &H660,
        &H182, &H5573, &HE, &H3805,
        &H2424, &H2220, &H90C, &HECD,
        &H10E, &H23F7, &H5237, &H64A8
    }

    Public Attack As Byte
    Public Decay As Byte
    Public Sustain As Byte
    Public Release As Byte

    Private gate As Boolean
    Private resetLfsr As Boolean
    Private lfsr As UInteger = &H7FFF
    Private rate As UInteger
    Private exponentialCounter As UInteger
    Private exponentialCounterPeriod As UInteger = 1
    Private newExponentialCounterPeriod As UInteger
    Private statePipeline As Integer
    Private envelopePipeline As Integer
    Private exponentialPipeline As Integer
    Private state As EState = EState.Release
    Private nextState As EState = EState.Release
    Private counterEnabled As Boolean = True
    Private envelopeCounter As Byte = &H0
    Private env3 As Byte

    Public Sub New()
        Reset()
    End Sub
    Public Overrides Function ToString() As String
        Return $"A: {Attack} D: {Decay} S: {Sustain \ 17} R: {Release} Lvl: {env3}"
    End Function
    Public Sub Reset()
        envelopePipeline = 0
        statePipeline = 0
        exponentialPipeline = 0

        Attack = 0
        Decay = 0
        Sustain = 0
        Release = 0

        gate = False
        resetLfsr = True
        lfsr = &H7FFF
        exponentialCounter = 0
        exponentialCounterPeriod = 1
        newExponentialCounterPeriod = 0
        state = EState.Release
        nextState = EState.Release
        counterEnabled = True
        rate = adsrTable(Release)
    End Sub

    Public Sub WriteControlReg(control As Byte)
        Dim gateNext As Boolean = (control And &H1) <> 0

        If gateNext <> gate Then
            gate = gateNext

            If gateNext Then
                'attac
                nextState = EState.Attack
                statePipeline = 2
                If resetLfsr OrElse envelopePipeline = 2 Then
                    envelopePipeline = If(exponentialCounterPeriod = 1 OrElse envelopePipeline = 2, 2, 4)
                ElseIf envelopePipeline = 1 Then
                    statePipeline = 3
                End If
            Else
                nextState = EState.Release
                statePipeline = If(envelopePipeline > 0, 3, 2)
            End If
        End If
    End Sub

    ' --- Attack/Decay ---
    Public Sub WriteAttackDecay(value As Byte)
        Attack = (value >> 4) And &HF
        Decay = value And &HF

        If state = EState.Attack Then
            rate = adsrTable(Attack)
        ElseIf state = EState.DecaySustain Then
            rate = adsrTable(Decay)
        End If
    End Sub

    ' --- Sustain/Release ---
    Public Sub WriteSustainRelease(value As Byte)
        Sustain = (value And &HF0) Or ((value >> 4) And &HF)
        Release = value And &HF

        If state = EState.Release Then
            rate = adsrTable(Release)
        End If
    End Sub

    ' --- Clock the ADSR ---
    Public Sub Clock()
        env3 = envelopeCounter

        ' Update exponential counter period if it changed
        If newExponentialCounterPeriod > 0 Then
            exponentialCounterPeriod = newExponentialCounterPeriod
            newExponentialCounterPeriod = 0
        End If

        ' State change pipeline
        If statePipeline > 0 Then
            StateChange()
        End If

        ' Envelope pipeline
        If envelopePipeline <> 0 Then
            envelopePipeline -= 1
            If envelopePipeline = 0 AndAlso counterEnabled Then
                Select Case state
                    Case EState.Attack
                        envelopeCounter = CByte(Math.Min(255, envelopeCounter + 1))
                        If envelopeCounter = 255 Then
                            nextState = EState.DecaySustain
                            statePipeline = 3
                        End If
                    Case EState.DecaySustain, EState.Release
                        envelopeCounter = CByte(Math.Max(0, envelopeCounter - 1))
                        If envelopeCounter = 0 Then counterEnabled = False
                End Select
                SetExponentialCounter()
            End If
            Return
        End If

        ' Exponential pipeline
        If exponentialPipeline <> 0 Then
            exponentialPipeline -= 1
            If exponentialPipeline = 0 Then
                exponentialCounter = 0
                If (state = EState.DecaySustain AndAlso envelopeCounter <> Sustain) OrElse state = EState.Release Then
                    envelopePipeline = 1
                End If
            End If
            Return
        End If

        ' Reset LFSR
        If resetLfsr Then
            lfsr = &H7FFF
            resetLfsr = False

            If state = EState.Attack Then
                exponentialCounter = 0
                envelopePipeline = 2
            Else
                If counterEnabled Then
                    exponentialCounter += 1
                    If exponentialCounter = exponentialCounterPeriod Then
                        exponentialPipeline = If(exponentialCounterPeriod <> 1, 2, 1)
                    End If
                End If
            End If
        End If

        ' LFSR clocking
        If lfsr <> rate Then
            Dim feedback As UInteger = ((lfsr << 14) Xor (lfsr << 13)) And &H4000
            lfsr = (lfsr >> 1) Or feedback
        Else
            resetLfsr = True
        End If
    End Sub

    ' --- State change ---
    Private Sub StateChange()
        statePipeline -= 1

        Select Case nextState
            Case EState.Attack
                If statePipeline = 1 Then
                    rate = adsrTable(Decay)
                ElseIf statePipeline = 0 Then
                    state = EState.Attack
                    rate = adsrTable(Attack)
                    counterEnabled = True
                End If
            Case EState.DecaySustain
                If statePipeline = 0 Then
                    state = EState.DecaySustain
                    rate = adsrTable(Decay)
                End If
            Case EState.Release
                If (state = EState.Attack AndAlso statePipeline = 0) _
                    OrElse (state = EState.DecaySustain AndAlso statePipeline = 1) Then
                    state = EState.Release
                    rate = adsrTable(Release)
                End If
        End Select
    End Sub

    ' --- Exponential counter ---
    Private Sub SetExponentialCounter()
        Select Case envelopeCounter
            Case &HFF, &H0
                newExponentialCounterPeriod = 1
            Case &H5D
                newExponentialCounterPeriod = 2
            Case &H36
                newExponentialCounterPeriod = 4
            Case &H1A
                newExponentialCounterPeriod = 8
            Case &HE
                newExponentialCounterPeriod = 16
            Case &H6
                newExponentialCounterPeriod = 30
        End Select
    End Sub

    ' --- Read current output ---
    Public Function Output() As Byte
        Return env3
    End Function

End Class
