Public Class Voice
    <Flags>
    Public Enum EWaveform
        None = 0
        Triangle = 1
        Sawtooth = 2
        Pulse = 4
        Noise = 8
    End Enum
    Private lfsr As New SIDNoiseLFSR
    Public MuteVoice As Boolean = False
    Public LoFiDuty As Boolean = False
    Public FreqLo As Byte
    Public FreqHi As Byte
    Public Control As Byte
    Public Waveform As EWaveform = EWaveform.None
    Public DutyCycle As UShort = 0 ' 0-4095
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
            DutyCycle = (CUInt(PulseWidthHi And &HF) << 8)
        Else
            ' 12 bit
            DutyCycle = (CUInt(PulseWidthHi And &HF) << 8) Or PulseWidthLo
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
        Dim sourceIndex As Byte = (Me.Index + 2) Mod 3
        Dim sourceVoice = Me.Parent.Voices(sourceIndex)


        ' new 12 bit phase
        Dim acc As UShort = (phase >> 12) And &HFFF

        Dim masterPhaseMSB As Boolean = (sourceVoice.phase And &H800000) <> 0
        If (Control And &H2) <> 0 Then
            If (Not prevMasterMSB) AndAlso masterPhaseMSB Then
                Me.phase = 0
            End If
        End If
        prevMasterMSB = masterPhaseMSB

        ' sid accurate waveform generation
        Dim dacInput As UShort = 0
        Dim noWaveform As Boolean = False

        If Waveform = EWaveform.Sawtooth Then
            dacInput = acc
        ElseIf Waveform = EWaveform.Triangle Then
            dacInput = (acc << 1) And &HFFF
            If (acc And &H800) <> 0 Then ' MSB is bit 11
                dacInput = dacInput Xor &HFFF
            End If
        ElseIf waveform = EWaveform.Pulse Then
            dacInput = If(acc > DutyCycle, &HFFF, 0)
        ElseIf Waveform = EWaveform.Noise Then
            Dim noiseClock As Byte = (phase >> 19) And 1

            If prevNoiseClock = 0 AndAlso noiseClock = 1 Then
                currentNoise = lfsr.Read(acc)
            End If

            prevNoiseClock = noiseClock

            dacInput = currentNoise
        ElseIf Waveform = (EWaveform.Sawtooth Or EWaveform.Triangle) Then
            If Parent.Mode6581 Then
                dacInput = If(acc < DutyCycle, 0, SawTriWF6581(acc) << 4)
            Else
                dacInput = If(acc < DutyCycle, 0, SawTriWF8580(acc) << 4)
            End If
        ElseIf Waveform = (EWaveform.Triangle Or EWaveform.Pulse) Then
            If Parent.Mode6581 Then
                dacInput = If(acc < DutyCycle, 0, TriPulseWF6581(acc) << 4)
            Else
                dacInput = If(acc < DutyCycle, 0, TriPulseWF8580(acc) << 4)
            End If
        ElseIf Waveform = (EWaveform.Sawtooth Or EWaveform.Pulse) Then
            If Parent.Mode6581 Then
                dacInput = If(acc < DutyCycle, 0, SawPulseWF6581(acc) << 4)
            Else
                dacInput = If(acc < DutyCycle, 0, SawPulseWF8580(acc) << 4)
            End If
        ElseIf Waveform = (EWaveform.Sawtooth Or EWaveform.Triangle Or EWaveform.Pulse) Then
            If Parent.Mode6581 Then
                dacInput = If(acc < DutyCycle, 0, SawTriPulseWF6581(acc) << 4)
            Else
                dacInput = If(acc < DutyCycle, 0, SawTriPulseWF8580(acc) << 4)
            End If
        Else
            noWaveform = True
        End If
        ' xor ringmod
        If Waveform = EWaveform.Triangle AndAlso (Control And &H4) Then
            If (sourceVoice.phase And &H800000) <> 0 Then
                dacInput = 4095 - dacInput
            End If
        End If

        Dim w As Integer = CInt(Waveform)

        If (w And 8) = 0 AndAlso (w And (w - 1)) <> 0 AndAlso
           (Control And &H4) AndAlso
           (sourceVoice.phase And &H800000) <> 0 Then

            dacInput = 0
        End If

        wave = (dacInput / &HFFF) * 1.275 - 0.275 ' cursed

        If noWaveform Then
            wave = lastOutput
        End If

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
