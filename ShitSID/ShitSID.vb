Imports NAudio.Wave

Public Class ShitSID
    Public SampleRate As Int32 = 44100
    Public Filter As SIDFilter
    Public Voices(2) As Voice
    Public currentTime As Double = 0
    Public filterCutoffLo As Integer = 0
    Public filterCutoffHi As Integer = 0
    Public filterResonance As Integer = 0
    Public filterMode As Integer = 0 ' 0-7 (bit flags)
    Public bypassFilter As Boolean = False
    Public VolumeSampleMode As Boolean = True
    Public VolumeRegister As Byte
    Public MuteVoice3 As Boolean = False
    Const ClocksPerFrame = 985248.0
    Public Sub New(Optional samplerate As Int32 = 44100)
        Me.SampleRate = samplerate
        Filter = New SIDFilter(samplerate)
        For i As Integer = 0 To 2
            Voices(i) = New Voice(Me, i)
        Next
    End Sub
    Public Sub Clock()
        currentTime += 1.0 / ClocksPerFrame
        For i = 0 To 2
            Voices(i).Envelope.Clock()
        Next
    End Sub
    Public Function LowPassSample(x As Double, cutoffFreq As Double, sampleRate As Double) As Double
        Static lastOutput As Double = 0 ' persists across calls
        If sampleRate > cutoffFreq * 2 Then
            Dim dt As Double = 1.0 / sampleRate
            Dim rc As Double = 1.0 / (2 * Math.PI * cutoffFreq)
            Dim alpha As Double = dt / (rc + dt)

            lastOutput += alpha * (x - lastOutput)
            Return lastOutput
        Else
            Return x
        End If
    End Function
    Public Function GetSample() As Double
        Dim output As Double = 0
        Dim filterInput As Double = 0
        For i = 0 To 2
            Dim v = Voices(i)
            If v.UseFilter AndAlso bypassFilter = False Then
                filterInput += v.Generate(currentTime)
            Else
                If MuteVoice3 AndAlso i = 2 Then
                Else
                    output += v.Generate(currentTime)
                End If
            End If
        Next
        If Not bypassFilter Then
            output += Filter.ApplyFilter(filterInput)
        End If
        If VolumeSampleMode Then
            output += (VolumeRegister - 15) / 4
        Else
            output *= VolumeRegister / 15.0
            output += (VolumeRegister - 15) / 4
        End If
        Return LowPassSample(output / 4, 17640, SampleRate)
    End Function


    Public Sub WriteRegister(addr As Integer, value As Byte)
        ' Handle register writes immediately
        Select Case addr
        ' Filter settings
            Case &HD415
                Dim bits As New BitArray({value})
                filterCutoffLo = 0
                If bits(0) Then filterCutoffLo += 1
                If bits(1) Then filterCutoffLo += 2
                If bits(2) Then filterCutoffLo += 4
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
                If bits(4) Then filterMode += SIDFilter.EFilterType.LowPass
                If bits(5) Then filterMode += SIDFilter.EFilterType.BandPass
                If bits(6) Then filterMode += SIDFilter.EFilterType.HighPass
                MuteVoice3 = bits(7)
                VolumeRegister = value And &HF
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
                voice.Envelope.WriteControlReg(value)
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
                    voice.Waveform = "none" ' No audio
                End If
            Case 5 ' ATTACK/DECAY
                voice.Envelope.WriteAttackDecay(value)
            Case 6 ' SUSTAIN/RELEASE
                voice.Envelope.WriteSustainRelease(value)
        End Select
    End Sub
    Private Sub UpdateFilterSettings()
        Dim msbPart As Integer = filterCutoffHi << 3
        Dim lsbPart As Integer = filterCutoffLo And &H7 ' 11 bit is cursed
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
    Public Sub Reset()
        currentTime = 0
        Filter = New SIDFilter(SampleRate)
        For i As Integer = 0 To 2
            Voices(i) = New Voice(Me, i)
        Next
    End Sub
End Class
Public Class Voice
    Public MuteVoice As Boolean = False
    Public LoFiDuty As Boolean = False
    Public lastGateVal = False
    Public Frequency As Double = 440.0
    Public FreqLo As Byte
    Public FreqHi As Byte
    Public Control As Byte
    Public Waveform As String = "square"
    Public DutyCycle As Double = 0.5
    Public Envelope As New EnvelopeGenerator()
    Public PulseWidthLo As Byte = 0
    Public PulseWidthHi As Byte = 0
    Public UseFilter As Boolean = False
    Private lastNoiseUpdate As Double = 0
    Public currentNoise As Double = 0
    Private lastGateToggleTime As Double = -1.0 '''
    Private Index As Int32
    Private Parent As ShitSID
    Private phase As Double = 0.0
    Private lastTime As Double = 0.0
    Private prevMasterMSB As Boolean = False
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

    'Public Sub NoteOn(currentTime As Double)
    '    Envelope.NoteOn()
    'End Sub
    'Public Sub NoteOff(currentTime As Double)
    '    Envelope.NoteOff()
    'End Sub
    ' Add these fields to your Voice class
    Public Function Generate(time As Double) As Double
        Dim deltaTime As Double = time - lastTime
        lastTime = time
        Dim wave As Double

        If (Control And &H8) <> 0 Then ' test bit
            Return Envelope.Output / 255.0
        End If
        If MuteVoice Then Return 0

        ' sync/ringmod stuff
        Dim sourceIndex As Integer = (Me.Index + 2) Mod 3
        Dim sourceVoice = Me.Parent.Voices(sourceIndex)

        ' phase accumulator
        phase += Frequency * deltaTime
        phase = phase Mod 1.0

        '  hardsync
        Dim masterVoice As Voice = Me.Parent.Voices(sourceIndex)
        Dim masterPhaseMSB As Boolean = (masterVoice.phase >= 0.5)
        If (Control And &H2) <> 0 Then
            If (Not prevMasterMSB) AndAlso masterPhaseMSB Then
                Me.phase = 0.0
            End If
        End If
        prevMasterMSB = masterPhaseMSB

        Dim envLevel = Envelope.Output / 255.0
        'If envLevel <= 0 AndAlso Envelope.IsIdle() Then Return 0

        ' new 12 bit accumulator
        Dim acc As Integer = CInt(phase * &HFFF) ' 3 nybbles looks cursed

        ' sid accurate waveform generation
        Dim sawVal As Integer = acc
        Dim triVal As Integer = (acc << 1) And &HFFF
        If (acc And &H800) <> 0 Then ' MSB is bit 11
            triVal = triVal Xor &HFFF
        End If
        Dim pulseVal As Integer = If(acc < DutyCycle * &HFFF, &HFFF, 0)

        Dim dacInput As Integer = 0
        Select Case Waveform
            Case "saw"
                dacInput = sawVal
            Case "tri"
                dacInput = triVal
            Case "square"
                dacInput = pulseVal
            Case "noise"
                ' still float noise
                Dim cycleDuration As Double = 1.0 / Frequency
                Dim interval As Double = cycleDuration / 16.0
                If time - lastNoiseUpdate >= interval Then
                    lastNoiseUpdate = time
                    Static rand As New Random()
                    currentNoise = rand.NextDouble() * 2 - 1
                End If
                wave = currentNoise
            Case "tri+saw"
                dacInput = triVal Or sawVal
            Case "tri+pulse"
                dacInput = If(acc < DutyCycle * &HFFF, 0, TriPulseWF(acc) * 16)
            Case "saw+pulse"
                dacInput = sawVal And pulseVal
            Case Else
                dacInput = 0
        End Select

        ' if not noise then 12-bit -> float
        If Waveform <> "noise" Then
            ' Normalize the 12-bit DAC value to -1.0 to 1.0 float
            wave = (dacInput / &HFFF) * 2.0 - 1.0
        End If

        ' xor ringmod
        If (Control And &H4) <> 0 Then
            Dim modulator As Double = 1.0 - 4 * Math.Abs(sourceVoice.phase - 0.5)
            wave *= If(modulator > 0, 1, -1)
        End If
        Return wave * envLevel
    End Function
End Class
Public Class EnvelopeGenerator

    ' this is a conversion from the reSIDfp EnvelopeGenerator class
    ' credit them, not me
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
    Private envelopeCounter As Byte = &HAA
    Private env3 As Byte

    Public Sub New()
        Reset()
    End Sub
    Public Overrides Function ToString() As String
        Return $"A: {Attack} D: {Decay} S: {Sustain} R: {Release} Lvl: {env3}"
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
    Public CutoffMultiplier As Double = 1
    Public CutoffBias As Int32 = 0
    Public ResonanceDivider As Double = 1
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
        If Mode6581 Then
            Return cutoffCurve6581(cutoffVal).Item2 + CutoffBias
        Else
            Return cutoffCurve8580(cutoffVal).Item2 + CutoffBias
        End If
    End Function
    Public distortionOffset As Double = -0.4
    Public distortionMult As Double = 1
    Public LastInput As Double = 0
    'Public fs As New FileStream("filterlog.txt", FileMode.Create)
    Public Function ApplyFilter(input As Double) As Double
        ' Calculate cutoff frequency and resonance
        Dim freq = InterpolatedCutoff()
        'fs.Write(Encoding.UTF8.GetBytes(freq.ToString & vbCrLf))
        freq = Math.Clamp(freq, 1, sampleRate / 2 - 1)

        ' f: frequency coefficient, q: resonance damping
        Dim f As Double = 2 * Math.Sin(Math.PI * freq / sampleRate)
        f = Math.Max(f, 0.0001)
        Dim q As Double = 1.0 - (resonance / ResonanceDivider)
        q = Math.Clamp(q, 0.01, 0.99)

        ' Standard SVF structure
        Dim hp As Double = input - lowpass - q * bandpass
        'hp *= 0.67 ' damping makes a comeback
        ' nvm i upped sample rate lol
        Dim bp As Double = bandpass + f * hp
        Dim lp As Double = lowpass + f * bp

        ' Store state for next iteration
        highpass = hp
        highpass = Math.Clamp(highpass, -2, 2)
        bandpass = bp
        bandpass = Math.Clamp(bandpass, -2, 2)
        lowpass = lp
        lowpass = Math.Clamp(lowpass, -2, 2)


        ' Output based on selected filter mode
        Dim output As Double = 0.0
        If (filterType And EFilterType.LowPass) <> 0 Then output += lowpass
        If (filterType And EFilterType.BandPass) <> 0 Then output += bandpass
        If (filterType And EFilterType.HighPass) <> 0 Then output += highpass
        If Mode6581 Then
            output = (Math.Tanh((output * distortionMult) + distortionOffset) - distortionOffset) / distortionMult
        End If

        Return output
    End Function

    Public Sub Reset()
        bandpass = 0
        lowpass = 0
        highpass = 0
    End Sub
End Class