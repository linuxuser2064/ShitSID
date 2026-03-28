Public Class Voice
    Private lfsr As New SidNoiseLFSR
    Public MuteVoice As Boolean = False
    Public LoFiDuty As Boolean = False
    Public Frequency As Double = 0
    Public FreqLo As Byte
    Public FreqHi As Byte
    Public Control As Byte
    Public Waveform As String = "pulse"
    Public DutyCycle As Double = 0.5
    Public PulseWidthLo As Byte = 0
    Public PulseWidthHi As Byte = 0
    Public UseFilter As Boolean = False
    Public Envelope As New EnvelopeGenerator()
    Private lastNoiseUpdate As Double = 0
    Private currentNoise As Double = 0
    Private Index As Int32
    Private Parent As ShitSID
    Private phase As Double = 0.0
    Private lastTime As Double = 0.0
    Private prevMasterMSB As Boolean = False
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

    ' SID pitch formula: freq = (fregval * clock) / 16777216
    Public Sub UpdateFrequency()

        Dim NewFreqHi As Byte = FreqHi
        Dim NewFreqLo As Byte = FreqLo
        Dim freqVal = (CUInt(NewFreqHi) << 8) Or CUInt(NewFreqLo)
        Dim actualFrequency = freqVal * 985248.0 / 16777216.0
        Frequency = actualFrequency
        'Frequency = Math.Round(Frequency / 10) * 10
    End Sub
    Public Function GenerateNoEnvelope(time As Double) As Double
        Dim deltaTime As Double = time - lastTime
        lastTime = time
        Dim wave As Double

        If (Control And &H8) <> 0 Then ' test bit
            Return Envelope.Output / 255.0
        End If

        ' sync/ringmod stuff
        Dim sourceIndex As Integer = (Me.Index + 2) Mod 3
        Dim sourceVoice = Me.Parent.Voices(sourceIndex)

        Dim newFrequency = Frequency

        ' phase accumulator
        phase += newFrequency * deltaTime
        phase = phase Mod 1.0

        ' new 12 bit phase
        Dim acc As Integer = CInt(phase * &HFFF) ' 3 nybbles looks cursed
        ' sid accurate waveform generation
        Dim sawVal As Integer = acc
        Dim triVal As Integer = (acc << 1) And &HFFF
        If (acc And &H800) <> 0 Then ' MSB is bit 11
            triVal = triVal Xor &HFFF
        End If
        Dim pulseVal As Integer = If(acc > DutyCycle * &HFFF, &HFFF, 0)

        Dim dacInput As Integer = 0
        Select Case Waveform
            Case "saw"
                dacInput = sawVal
            Case "tri"
                dacInput = triVal
            Case "pulse"
                dacInput = pulseVal
            Case "noise"
                Dim cycleDuration As Double = 1.0 / Frequency
                Dim interval As Double = cycleDuration / 16.0
                If time - lastNoiseUpdate >= interval Then
                    lastNoiseUpdate = time
                    currentNoise = lfsr.Read(acc) 'rand.NextDouble() * 2 - 1
                End If
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
                dacInput = 0
        End Select
        ' xor ringmod
        If Waveform = "tri" AndAlso (Control And &H4) Then
            If sourceVoice.phase >= 0.5 Then
                dacInput = 4095 - dacInput
            End If
        End If
        If (Waveform = "saw+tri" Or Waveform = "tri+pulse" Or Waveform = "saw+pulse" Or Waveform = "saw+tri+pulse") AndAlso (Control And &H4) Then
            If sourceVoice.phase >= 0.5 Then
                dacInput *= 0
            End If
        End If
        ' Normalize the 12-bit DAC value to -1.0 to 1.0 float
        wave = (dacInput / &HFFF) * 2.0 - 1.0
        '  hardsync
        Dim masterPhaseMSB As Boolean = (sourceVoice.phase >= 0.5)
        If (Control And &H2) <> 0 Then
            If (Not prevMasterMSB) AndAlso masterPhaseMSB Then
                Me.phase = 0.0
            End If
        End If
        prevMasterMSB = masterPhaseMSB
        Return wave
    End Function
    Public Function Generate(time As Double) As Double
        Dim envLevel = Envelope.Output / 255.0
        Dim output = GenerateNoEnvelope(time) * envLevel

        If MuteVoice Then Return 0
        Return output
    End Function
End Class
