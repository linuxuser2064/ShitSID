Public Class ShitSID
    Public Filter As New SIDFilter(44100)
    Public Voices(2) As Voice
    Public currentTime As Double = 0
    Public filterCutoffLo As Integer = 0
    Public filterCutoffHi As Integer = 0
    Public filterResonance As Integer = 0
    Public filterMode As Integer = 0 ' 0-7 (bit flags)
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
        'Return Math.Max(-1, Math.Min(1, output))
        Return output
    End Function
    Public Sub WriteRegister(addr As Integer, value As Byte)
        ' das filter
        Select Case addr
            Case &HD415
                Dim bits As New BitArray({value})
                filterCutoffLo = 0
                If bits(0) Then
                    filterCutoffLo += 1
                End If
                If bits(1) Then
                    filterCutoffLo += 2
                End If
                If bits(2) Then
                    filterCutoffLo += 4
                End If
                UpdateFilterSettings()
                Return
            Case &HD416
                filterCutoffHi = value
                UpdateFilterSettings()
                Return
            Case &HD417
                filterResonance = (value >> 4) And &HF
                Voices(0).UseFilter = (value And 1) <> 0
                Voices(1).UseFilter = (value And 2) <> 0
                Voices(2).UseFilter = (value And 4) <> 0
                UpdateFilterSettings()
                Return
            Case &HD418
                Dim bits As New BitArray({value})
                filterMode = 0
                If bits(4) Then
                    filterMode += SIDFilter.EFilterType.LowPass
                End If
                If bits(5) Then
                    filterMode += SIDFilter.EFilterType.BandPass
                End If
                If bits(6) Then
                    filterMode += SIDFilter.EFilterType.HighPass
                End If
                UpdateFilterSettings()
                Return
        End Select
        Dim reg = addr And &H1F
        Dim voiceNum = reg \ 7
        If voiceNum > 2 Then Return

        Dim voice = Voices(voiceNum)
        Dim subReg = reg Mod 7

        Select Case subReg
            Case 0 ' FREQ LO
                voice.FreqLo = value
                voice.UpdateFrequency()
            Case 1 ' FREQ HI
                voice.FreqHi = value
                voice.UpdateFrequency()
            Case 2 ' PW LO
                voice.PulseWidthLo = value
                voice.UpdateDutyCycle()
            Case 3 ' PW HI
                voice.PulseWidthHi = value
                voice.UpdateDutyCycle()
            Case 4 ' CONTROL REG
                voice.Control = value
                Dim bits As New BitArray({value})
                'If bits(0) = True AndAlso voice.lastGateVal = False Then
                '    voice.NoteOn(currentTime)
                'ElseIf bits(0) = False AndAlso voice.lastGateVal = True Then
                '    voice.NoteOff(currentTime)
                'End If
                If bits(0) = True AndAlso voice.lastGateVal = False Then
                    voice.pendingNoteOn = True
                ElseIf bits(0) = False AndAlso voice.lastGateVal = True Then
                    voice.pendingNoteOff = True
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
    Private Sub UpdateFilterSettings()
        Dim msbPart As Integer = filterCutoffHi << 3
        Dim lsbPart As Integer = filterCutoffLo And &H7 ' Mask just the lowest 3 bits
        Dim combined As UShort = CUInt(msbPart Or lsbPart)
        Dim cutoff = combined
        Filter.SetCutoff(cutoff)
        Filter.SetResonance(filterResonance)

        Dim mode As SIDFilter.EFilterType = SIDFilter.EFilterType.None
        If (filterMode And 1) <> 0 Then mode = mode Or SIDFilter.EFilterType.LowPass
        If (filterMode And 2) <> 0 Then mode = mode Or SIDFilter.EFilterType.BandPass
        If (filterMode And 4) <> 0 Then mode = mode Or SIDFilter.EFilterType.HighPass

        Filter.SetFilterType(mode)
    End Sub
End Class
Public Class Voice
    Public pendingNoteOn As Boolean = False
    Public pendingNoteOff As Boolean = False
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
    Public UseFilter As Boolean = False
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
            Envelope.attackStartLevel = 0
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
        If UseFilter Then
            Return Parent.Filter.ApplyFilter(wave * envLevel)
        End If
        Return wave * envLevel
    End Function
End Class
Public Class ADSR
    Public Attack As Integer = 0     ' 0–15
    Public Decay As Integer = 0      ' 0–15
    Public Sustain As Double = 15   ' 0-15
    Public Release As Integer = 0    ' 0–15
    Public releaseStartLevel As Double = 0
    Public state As String = "idle"
    Public level As Double = 0
    Public startTime As Double = 0
    Public decayStartLevel As Double
    Public attackStartLevel As Double = 0
    Public Overrides Function ToString() As String
        Return $"A:{Attack} D:{Decay} S:{Sustain} R:{Release}, State: {state}, Level: {(CInt(level * 10) / 10).ToString}"
    End Function
    Private Function RateToTime(rate As Integer, stage As String) As Double
        ' SID ADSR map
        Dim attackTimes() As Double = {0.002, 0.008, 0.016, 0.024, 0.038, 0.056, 0.068, 0.08, 0.1, 0.25, 0.5, 0.8, 1.0, 3.0, 5.0, 8.0}
        Dim decayReleaseTimes() As Double = {0.0064, 0.024, 0.048, 0.072, 0.114, 0.168, 0.204, 0.24, 0.3, 0.75, 1.5, 2.4, 3.0, 9.0, 15.0, 24.0}

        rate = Math.Max(0, Math.Min(rate, 15))

        Select Case stage
            Case "attack"
                Return attackTimes(rate)
            Case "decay"
                Dim dur = decayReleaseTimes(rate) * 0.98 ' 0.98 because PAL speed
                Dim ε = 0.99 ' εεεεεεεεεε
                Return -Math.Log(1 - ε) / dur
            Case "release"
                Dim dur = decayReleaseTimes(rate) * 0.985 ' 0.98 because PAL speed
                Dim ε = 0.99 ' εεεεεεεεεε
                Return -Math.Log(1 - ε) / dur
            Case Else
                Return 0.1
        End Select
    End Function

    Public Function IsIdle() As Boolean
        Return state = "idle"
    End Function

    Public Sub NoteOn(currentTime As Double)
        attackStartLevel = level  ' Remember where we were
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
                level = attackStartLevel + (1.0 - attackStartLevel) * (t / dur)
                If level >= 1.0 Then
                    level = 1.0
                    state = "decay"
                    startTime = currentTime
                End If


            Case "decay"
                Dim k = RateToTime(Decay, "decay")
                Dim target = Sustain / 15.0
                level = target + (1.0 - target) * Math.Exp(-k * t)
                If level <= target + 0.001 Then ' close enough
                    level = target
                    state = "sustain"
                    startTime = currentTime
                End If

            Case "sustain"
                level = Sustain / 15.0 ' Sustain phase is constant

            Case "release"
                Dim k = RateToTime(Release, "release")
                Dim target = 0
                level = releaseStartLevel * Math.Exp(-k * t)
                If level <= target + 0.001 Then ' close enough
                    level = target
                    state = "idle"
                End If

            Case Else
                level = 0
        End Select

        Return level
    End Function
End Class
Public Class SIDFilter
    <Flags>
    Public Enum EFilterType
        None = 0
        LowPass = 1
        BandPass = 2
        HighPass = 4
    End Enum
    Public Mode6581 As Boolean = False
    Private filterType As EFilterType = EFilterType.LowPass
    Private resonance As Double = 0.0 ' 0.0 to 1.0
    Private cutoffVal As Integer = 0
    Private sampleRate As Double = 44100.0
    Private bandpass As Double = 0.0
    Private lowpass As Double = 0.0
    Private highpass As Double = 0.0
    Public CutoffMultiplier As Double = 1.67
    Public CutoffBias As Int32 = 0
    Public ResonanceDivider As Double = 3
    ' approx filter curve
    Private ReadOnly cutoffCurve As New List(Of Tuple(Of Integer, Double)) From {
        Tuple.Create(0, 200.0),
        Tuple.Create(250, 1600.0),
        Tuple.Create(900, 6000.0),
        Tuple.Create(1000, 7000.0),
        Tuple.Create(1250, 8100.0),
        Tuple.Create(1500, 10000.0),
        Tuple.Create(1750, 12500.0),
        Tuple.Create(2000, 13500.0),
        Tuple.Create(2047, 13900.0)
    }

    Public Sub New(Optional sampleRate As Double = 44100.0)
        Me.sampleRate = sampleRate
    End Sub
    Public Overrides Function ToString() As String
        Return $"Type: {filterType}, Reso: {resonance}, Cutoff: {cutoffVal}"
    End Function
    Public Sub SetCutoff(rawCutoff As Integer)
        rawCutoff = Math.Max(0, Math.Min(2047, rawCutoff))
        cutoffVal = rawCutoff
    End Sub

    Public Sub SetResonance(reso As Integer)
        reso = Math.Max(0, Math.Min(15, reso))
        resonance = reso / 15.0 ' reso needs to be 0-1 not 0-15
    End Sub

    Public Sub SetFilterType(ftype As EFilterType)
        filterType = ftype
    End Sub

    Private Function InterpolatedCutoff() As Double
        ' clamp just in case
        cutoffVal = Math.Clamp(cutoffVal, 0, 2047)
        ' interpolation of cutoffVal to actual freq
        For i As Integer = 0 To cutoffCurve.Count - 2
            Dim x0 = cutoffCurve(i).Item1
            Dim y0 = cutoffCurve(i).Item2
            Dim x1 = cutoffCurve(i + 1).Item1
            Dim y1 = cutoffCurve(i + 1).Item2

            If cutoffVal >= x0 AndAlso cutoffVal <= x1 Then
                Dim t = (cutoffVal - x0) / (x1 - x0)
                Return (y0 + t * (y1 - y0)) + CutoffBias
            End If
        Next
        ' out of bounds management
        Return cutoffCurve.Last().Item2 + CutoffBias
    End Function


    Public Function ApplyFilter(input As Double) As Double
        Dim f As Double = CutoffMultiplier * Math.Sin(Math.PI * InterpolatedCutoff() / sampleRate)

        ' resonance managment
        Dim adjustedResonance As Double = resonance
        Dim q As Double = 1.0 - (adjustedResonance / ResonanceDivider)

        ' distortion🔥🔥🔥🔥
        Dim cutoffFactor As Double = 1.0 - (cutoffVal / 2047.0)
        Dim distortedInput As Double = input + Math.Tanh(input * cutoffFactor * 8.0) * cutoffFactor * 0.3
        If Mode6581 Then
            input = distortedInput
        End If
        ' svf filter
        Dim hp As Double = input - lowpass - q * bandpass
        If (filterType And EFilterType.BandPass) <> 0 AndAlso Mode6581 Then
            hp = Math.Clamp(hp, -0.7, 2) ' crust
        End If
        Dim bp As Double = bandpass + f * hp
        If (filterType And EFilterType.LowPass) <> 0 AndAlso Mode6581 Then
            bp = Math.Clamp(bp, -1, 2) ' crust v2
        End If
        Dim lp As Double = lowpass + f * bp
        highpass = hp
        bandpass = bp
        lowpass = lp

        ' output
        Dim output As Double = 0.0
        If (filterType And EFilterType.LowPass) <> 0 Then output += lowpass
        If (filterType And EFilterType.BandPass) <> 0 Then output += bandpass
        If (filterType And EFilterType.HighPass) <> 0 Then output += highpass
        Return output
    End Function

    Public Sub Reset()
        bandpass = 0
        lowpass = 0
        highpass = 0
    End Sub
End Class