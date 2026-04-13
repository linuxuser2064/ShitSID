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
    Public Output As Double = 0
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
        Dim freezeOutput As Boolean = Waveform = "noise"

        If (Control And &H8) <> 0 Then ' test bit
            freezeOutput = True
        End If


        ' sync stuff
        Dim sourceIndex As Integer = (Me.Index + 2) Mod 3
        Dim sourceVoice = Me.Parent.Voices(sourceIndex)

        Dim newFrequency = Frequency
        wave = Math.Sin(phase * 2 * Math.PI)
        Output = wave

        ' phase accumulator
        If (Control And &H2) <> 0 Then
            If sourceVoice.Frequency > Me.Frequency Then
                newFrequency = sourceVoice.Frequency
            End If
        End If
        ' ringmod (real)
        If Control And &H4 Then
            wave *= sourceVoice.Output
        End If
        If Not freezeOutput Then
            phase += newFrequency * deltaTime
        End If
        'If phase > 1 AndAlso (Not freezeOutput) Then
        '    phase -= 1
        'End If

        'If phase <= 1 Then
        '    phase += newFrequency * deltaTime
        'End If

        ' based on RMS
        If Waveform = "saw" Then wave *= 0.6
        If Waveform = "tri" Then wave *= 0.5

        Return wave
    End Function
    Public Function Generate(time As Double) As Double
        Dim envLevel = Envelope.Output / 255.0
        Dim output = GenerateNoEnvelope(time) * envLevel

        If MuteVoice Then Return 0
        Return output
    End Function
End Class
