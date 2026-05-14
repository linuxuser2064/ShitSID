Public Class Voice
    Private lfsr As New SidNoiseLFSR
    Public MuteVoice As Boolean = False
    Public LoFiDuty As Boolean = False
    Public FreqLo As Byte
    Public FreqHi As Byte
    Public Control As Byte
    Public Waveform As String = "pulse"
    Public DutyCycle As Double = 0.5
    Public PulseWidthLo As Byte = 0
    Public PulseWidthHi As Byte = 0
    Public UseFilter As Boolean = False
    Public Envelope As New EnvelopeGenerator()
    Private prevNoiseClock As Integer
    Private currentNoise As Integer
    Private Index As Int32
    Private Parent As ShitSID
    Private phase As Int32 = 0 ' actually 24-bit
    'Private lastTime As Double = 0.0
    Private prevMasterMSB As Boolean = False
    Private lastOutput As Double = 0
    ' 24-bit accumulator for SID-accurate phase timing
    Public Sub New(parent As ShitSID, i As Int32)
        Me.Parent = parent
        Me.Index = i
    End Sub
    Public Sub UpdateDutyCycle()
        If LoFiDuty Then
            ' 4 bit
            Dim val4 = PulseWidthHi And &HF
            DutyCycle = val4 / 15.0
        Else
            ' 12 bit
            Dim pulseVal = (CUInt(PulseWidthHi And &HF) << 8) Or PulseWidthLo
            DutyCycle = Math.Max(0.0, Math.Min(1.0, pulseVal / 4095.0))
        End If
    End Sub
    Public Sub Clock()
        ' phase accumulator (16777215 max)
        Dim accumulated = (CUInt(FreqHi) << 8) Or CUInt(FreqLo)
        phase += accumulated
        phase = phase Mod 16777216
        If (Control And &H8) <> 0 Then ' test bit
            phase = 0
        End If
    End Sub
    Public Function GenerateNoEnvelope() As Double
        Dim wave As Double = 0

        ' sync/ringmod stuff
        Dim sourceIndex As Integer = (Me.Index + 2) Mod 3
        Dim sourceVoice = Me.Parent.Voices(sourceIndex)


        ' new 12 bit phase
        Dim acc As Integer = (phase >> 12) And &HFFF
        ' sid accurate waveform generation
        Dim sawVal As Integer = acc
        Dim triVal As Integer = (acc << 1) And &HFFF
        If (acc And &H800) <> 0 Then ' MSB is bit 11
            triVal = triVal Xor &HFFF
        End If
        Dim pulseVal As Integer = If(acc > DutyCycle * &HFFF, &HFFF, 0)

        Dim dacInput As Integer = 0
        Dim accumulated = (CUInt(FreqHi) << 8) Or CUInt(FreqLo)
        Dim noWaveform As Boolean = False
        Select Case Waveform
            Case "saw"
                dacInput = sawVal
            Case "tri"
                dacInput = triVal
            Case "pulse"
                dacInput = pulseVal
            Case "noise"
                Dim noiseClock As Integer = (phase >> 19) And 1

                If prevNoiseClock = 0 AndAlso noiseClock = 1 Then
                    currentNoise = lfsr.Read(acc)
                End If

                prevNoiseClock = noiseClock

                dacInput = currentNoise
            Case "saw+tri"
                If Parent.Filter.Mode6581 Then
                    dacInput = If(acc < DutyCycle * &HFFF, 0, SawTriWF6581(acc) * 16)
                Else
                    dacInput = If(acc < DutyCycle * &HFFF, 0, SawTriWF8580(acc) * 16)
                End If
            Case "tri+pulse"
                If Parent.Filter.Mode6581 Then
                    dacInput = If(acc < DutyCycle * &HFFF, 0, TriPulseWF6581(acc) * 16)
                Else
                    dacInput = If(acc < DutyCycle * &HFFF, 0, TriPulseWF8580(acc) * 16)
                End If
            Case "saw+pulse"
                If Parent.Filter.Mode6581 Then
                    dacInput = If(acc < DutyCycle * &HFFF, 0, SawPulseWF6581(acc) * 16)
                Else
                    dacInput = If(acc < DutyCycle * &HFFF, 0, SawPulseWF8580(acc) * 16)
                End If
            Case "saw+tri+pulse"
                If Parent.Filter.Mode6581 Then
                    dacInput = If(acc < DutyCycle * &HFFF, 0, SawTriPulseWF6581(acc) * 16)
                Else
                    dacInput = If(acc < DutyCycle * &HFFF, 0, SawTriPulseWF8580(acc) * 16)
                End If
            Case Else
                noWaveform = True
        End Select
        ' xor ringmod
        If Waveform = "tri" AndAlso (Control And &H4) Then
            If sourceVoice.phase >= 8388608 Then
                dacInput = 4095 - dacInput
            End If
        End If
        If (Waveform = "saw+tri" Or Waveform = "tri+pulse" Or Waveform = "saw+pulse" Or Waveform = "saw+tri+pulse") AndAlso (Control And &H4) Then
            If sourceVoice.phase >= 8388608 Then
                dacInput = 0
            End If
        End If
        ' Normalize the 12-bit DAC value to -1.0 to 1.0 float
        wave = (dacInput / &HFFF) * 2.0 - 1.0
        If noWaveform Then
            wave = lastOutput
        End If
        '  hardsync
        Dim masterPhaseMSB As Boolean = (sourceVoice.phase >= 8388608)
        If (Control And &H2) <> 0 Then
            If (Not prevMasterMSB) AndAlso masterPhaseMSB Then
                Me.phase = 0
            End If
        End If
        prevMasterMSB = masterPhaseMSB
        lastOutput = wave
        Return wave
    End Function
    Public Function Generate() As Double
        Dim envLevel = Envelope.Output / 255.0
        Dim output = GenerateNoEnvelope() * envLevel

        If MuteVoice Then Return 0
        Return output
    End Function
End Class
