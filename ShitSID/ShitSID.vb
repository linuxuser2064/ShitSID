Public Class ShitSID
    Public Voices(2) As Voice
    Public currentTime As Double = 0
    Public Sub New()
        For i As Integer = 0 To 2
            Voices(i) = New Voice(Me, i)
        Next
    End Sub
    Public Sub Clock()
        currentTime += 1.0 / 985248.0
    End Sub

    Public Function GetSample() As Double
        Dim output As Double = 0
        For Each v In Voices
            output += v.Generate(currentTime)
        Next
        Return Math.Max(-1, Math.Min(1, output / 3))
    End Function
    Public Sub WriteRegister(addr As Integer, value As Byte)
        Dim reg = addr And &H1F
        Dim voiceNum = reg \ 7
        If voiceNum > 2 Then Return

        Dim voice = Voices(voiceNum)
        Dim subReg = reg Mod 7

        Select Case subReg
            Case 0 ' FREQ LO
                voice.FreqLo = value
            Case 1 ' FREQ HI
                voice.FreqHi = value
                voice.UpdateFrequency()
            Case 2 ' PW LO
                voice.PulseWidthLo = value
                voice.UpdateDutyCycle()
            Case 3 ' PW HI
                voice.PulseWidthHi = value And &HF
                voice.UpdateDutyCycle()
            Case 4 ' CONTROL REG
                voice.Control = value
                Dim bits As New BitArray({value})
                If bits(0) = True AndAlso voice.lastGateVal = False Then
                    voice.NoteOn(currentTime)
                ElseIf bits(0) = False AndAlso voice.lastGateVal = True Then
                    voice.NoteOff(currentTime)
                End If
                voice.lastGateVal = bits(0)
                If bits(4) AndAlso bits(5) Then
                    voice.Waveform = "tri+saw"
                ElseIf bits(4) AndAlso bits(6) Then
                    voice.Waveform = "tri+pulse"
                ElseIf bits(5) AndAlso bits(6) Then
                    voice.Waveform = "saw+pulse"
                ElseIf bits(4) Then
                    voice.Waveform = "tri"
                ElseIf bits(5) Then
                    voice.Waveform = "saw"
                ElseIf bits(6) Then
                    voice.Waveform = "square"
                ElseIf bits(7) Then
                    voice.Waveform = "noise"
                Else
                    voice.Waveform = "square" 'fallback
                End If
            Case 5 ' ATTACK/DECAY
                voice.Envelope.Attack = (value >> 4) And &HF
                voice.Envelope.Decay = value And &HF
            Case 6 ' SUSTAIN/RELEASE
                voice.Envelope.Sustain = ((value >> 4) And &HF)
                voice.Envelope.Release = value And &HF
        End Select
    End Sub
End Class
Public Class Voice
    Private Index As Int32
    Private Parent As ShitSID
    Public MuteVoice As Boolean = False
    Private phase As Double = 0.0
    Private lastTime As Double = 0.0
    Private prevMasterMSB As Boolean = False
    Public LoFiDuty As Boolean = False
    Public lastGateVal = False
    Public Frequency As Double = 440.0
    Public FreqLo As Byte
    Public FreqHi As Byte
    Public Control As Byte
    Public Waveform As String = "square"
    Public DutyCycle As Double = 0.5
    Public Envelope As New ADSR()
    Public PulseWidthLo As Byte = 0
    Public PulseWidthHi As Byte = 0
    Private lastNoiseUpdate As Double = 0
    Private currentNoise As Double = 0
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
        Dim freqVal = (CUInt(FreqHi) << 8) Or FreqLo
        Frequency = freqVal * 985248.0 / 16777216.0
    End Sub

    Public Sub NoteOn(currentTime As Double)
        Envelope.NoteOn(currentTime)
    End Sub

    Public Sub NoteOff(currentTime As Double)
        Envelope.NoteOff(currentTime)
    End Sub
    Public Function Generate(time As Double) As Double
        Dim deltaTime As Double = time - lastTime
        lastTime = time

        Dim wave As Double

        If (Control And &H8) <> 0 Then ' test bit
            Return Envelope.GetLevel(time)
        End If
        If MuteVoice Then Return 0

        ' source voices
        Dim sourceIndex As Integer = (Me.Index + 2) Mod 3 ' mod because the order
        Dim sourceVoice = Me.Parent.Voices(sourceIndex)

        Dim oldSourcePhase As Double = sourceVoice.phase

        ' phase accumulator
        phase += Frequency * deltaTime
        phase = phase Mod 1.0
        ' osc sync
        Dim masterVoice As Voice = Me.Parent.Voices(sourceIndex) 'aaaaaas
        Dim masterPhaseMSB As Boolean = (masterVoice.phase >= 0.5)
        If (Control And &H2) <> 0 Then
            If (Not prevMasterMSB) AndAlso masterPhaseMSB Then
                Me.phase = 0.0 ' phase reset
            End If
        End If
        prevMasterMSB = masterPhaseMSB
        Dim envLevel = Envelope.GetLevel(time)
        If envLevel <= 0 AndAlso Envelope.IsIdle() Then Return 0
        Select Case Waveform
            Case "saw"
                wave = 2 * phase - 1
            Case "tri"
                wave = 1.0 - 4 * Math.Abs(phase - 0.5)
            Case "square"
                wave = If(phase < DutyCycle, 1, -1)
            Case "noise"
                Dim cycleDuration As Double = 1.0 / Frequency
                Dim interval As Double = cycleDuration / 16.0
                If time - lastNoiseUpdate >= interval Then
                    lastNoiseUpdate = time
                    Static rand As New Random()
                    currentNoise = rand.NextDouble() * 2 - 1
                End If
                wave = currentNoise
            Case "tri+saw"
                wave = (1.0 - 4 * Math.Abs(phase - 0.5)) Xor (2 * phase - 1)
            Case "tri+pulse"
                wave = (1.0 - 4 * Math.Abs(phase - 0.5)) * If(phase < DutyCycle, 1, -1)
            Case "saw+pulse"
                wave = (2 * phase - 1) * If(phase < DutyCycle, 1, -1)
            Case Else
                wave = 0
        End Select

        ' xor ringmod
        If (Control And &H4) <> 0 Then
            Dim modulator As Double = 1.0 - 4 * Math.Abs(sourceVoice.phase - 0.5)
            wave *= If(modulator > 0, 1, -1)
        End If

        Return wave * envLevel
    End Function
End Class
Public Class ADSR
    Public Attack As Integer = 0     ' 0–15
    Public Decay As Integer = 0      ' 0–15
    Public Sustain As Double = 15   ' 0-15
    Public Release As Integer = 0    ' 0–15
    Private releaseStartLevel As Double = 0
    Private state As String = "idle"
    Private level As Double = 0
    Private startTime As Double = 0

    Private Function RateToTime(rate As Integer, stage As String) As Double
        ' SID ADSR map
        Dim attackTimes() As Double = {0.002, 0.008, 0.016, 0.024, 0.038, 0.056, 0.068, 0.08, 0.1, 0.25, 0.5, 0.8, 1.0, 3.0, 5.0, 8.0}
        Dim decayReleaseTimes() As Double = {0.006, 0.024, 0.048, 0.072, 0.114, 0.168, 0.204, 0.24, 0.3, 0.75, 1.5, 2.4, 3.0, 9.0, 15.0, 24.0}

        Select Case stage
            Case "attack"
                Return attackTimes(Math.Max(0, Math.Min(rate, 15)))
            Case "decay", "release"
                Return decayReleaseTimes(Math.Max(0, Math.Min(rate, 15))) / 4
            Case Else
                Return 0.1 ' no change
        End Select
    End Function
    Public Function IsIdle() As Boolean
        Return state = "idle"
    End Function
    Public Sub NoteOn(currentTime As Double)
        startTime = currentTime
        state = "attack"
    End Sub

    Public Sub NoteOff(currentTime As Double)
        releaseStartLevel = level
        startTime = currentTime
        state = "release"
    End Sub

    Public Function GetLevel(currentTime As Double) As Double
        Dim t As Double = currentTime - startTime

        Select Case state
            Case "attack"
                Dim dur = RateToTime(Attack, "attack")
                level = Math.Min(1.0, t / dur)
                If level >= 1.0 Then
                    state = "decay"
                    startTime = currentTime
                End If

            Case "decay"
                Dim dur = RateToTime(Decay, "decay")
                Dim delta = 1.0 - (Sustain / 15.0)
                level = 1.0 - delta * Math.Min(1.0, t / dur)
                If t >= dur Then
                    state = "sustain"
                    startTime = currentTime
                End If

            Case "sustain"
                level = Sustain / 15.0 ' this took ages to get working

            Case "release"
                Dim dur = RateToTime(Release, "release")
                level = releaseStartLevel * (1.0 - Math.Min(1.0, t / dur))
                If level <= 0.001 Then
                    level = 0
                    state = "idle"
                End If

            Case Else
                level = 0
        End Select

        Return level
    End Function
End Class