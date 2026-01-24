Imports NAudio.Wave
Imports System
Public Class ShitSID
    Enum FilterCurveType
        Dark
        Average
        Bright
    End Enum
    Public Voices(2) As Voice

    Public SampleRate As Int32 = 44100
    Public CurrentTime As Double = 0

    Public Filter As SIDFilter
    Public FilterCurve As FilterCurveType = FilterCurveType.Average


    Public FilterCutoffLo As Integer = 0
    Public FilterCutoffHi As Integer = 0
    Public FilterResonance As Integer = 0
    Public FilterMode As Integer = 0 ' 0-7 (bit flags)
    Public BypassFilter As Boolean = False

    Public VolumeSampleMode As Boolean = True
    Public VolumeRegister As Byte
    Public MuteSamples As Boolean = False

    Public MuteVoice3 As Boolean = False

    Public InternalAudioFilter As Boolean = True

    Const ClocksPerFrame = 985248.0
    Public Sub New(Optional samplerate As Int32 = 44100)
        Me.SampleRate = samplerate
        Filter = New SIDFilter(Me, samplerate)
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
            Dim generated = v.Generate(currentTime) ' generate anyway for ringmod accuracy
            If v.UseFilter AndAlso bypassFilter = False Then
                filterInput += generated
            Else
                If MuteVoice3 AndAlso i = 2 Then
                Else
                    output += generated
                End If
            End If
        Next
        If Not bypassFilter Then
            output += Filter.ApplyFilter(filterInput)
        End If
        If Not MuteSamples Then
            If Not VolumeSampleMode Then
                output *= VolumeRegister / 15.0
            End If
            output += (VolumeRegister - 15) / 4
        End If
        output /= 4
        'output = output - (output * output) ' holy distortion
        If InternalAudioFilter Then
            Return LowPassSample(output, 17640, SampleRate)
        Else
            Return output
        End If
    End Function

    Public Function ReadRegister(addr As Integer) As Byte
        Select Case addr
            Case 54299
                Return (Voices(2).GenerateNoEnvelope(CurrentTime) * 96) + 127
            Case 54300
                Return Voices(2).Envelope.Output
            Case Else
                Return 0
        End Select
    End Function
    Public Sub WriteRegister(addr As Integer, value As Byte)
        'Console.WriteLine($"SID Write {addr.ToString("X4")} to {value.ToString("X2")}")
        ' Handle register writes immediately
        Select Case addr
        ' Filter settings
            Case &HD415
                Dim bits As New BitArray({value})
                FilterCutoffLo = 0
                If bits(0) Then FilterCutoffLo += 1
                If bits(1) Then FilterCutoffLo += 2
                If bits(2) Then FilterCutoffLo += 4
                UpdateFilterSettings()
                Return
            Case &HD416
                FilterCutoffHi = value
                UpdateFilterSettings()
                Return
            Case &HD417
                FilterResonance = (value >> 4) And &HF
                Voices(0).UseFilter = (value And 1) <> 0
                Voices(1).UseFilter = (value And 2) <> 0
                Voices(2).UseFilter = (value And 4) <> 0
                UpdateFilterSettings()
                Return
            Case &HD418
                Dim bits As New BitArray({value})
                FilterMode = 0
                If bits(4) Then FilterMode += SIDFilter.EFilterType.LowPass
                If bits(5) Then FilterMode += SIDFilter.EFilterType.BandPass
                If bits(6) Then FilterMode += SIDFilter.EFilterType.HighPass
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
        Filter = New SIDFilter(Me, SampleRate)
        For i As Integer = 0 To 2
            Voices(i) = New Voice(Me, i)
        Next
    End Sub
End Class
Public Class Voice
    Private lfsr As New SidNoiseLFSR
    Public MuteVoice As Boolean = False
    Public LoFiDuty As Boolean = False
    Public Frequency As Double = 440.0
    Public FreqLo As Byte
    Public FreqHi As Byte
    Public Control As Byte
    Public Waveform As String = "square"
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
    Private envelopeCounter As Byte = &H0
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
    Public parent As ShitSID
    Private filterType As EFilterType = EFilterType.LowPass
    Private resonance As Double = 0.0 ' 0.0 to 1.0
    Private cutoffVal As Integer = 0          ' 0..2047 (11-bit)
    Private sampleRate As Double = 44100.0

    ' State for your SVF path
    Private bandpass As Double = 0.0
    Private lowpass As Double = 0.0
    Private highpass As Double = 0.0

    Public CutoffMultiplier As Double = 1
    Public CutoffBias As Int32 = 0
    Public ResonanceDivider As Double = 1
    Public Sub New(pnt As ShitSID, Optional sampleRate As Double = 44100.0)
        parent = pnt
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
        resonance = reso / 15.0
    End Sub

    Public Sub SetFilterType(ftype As EFilterType)
        filterType = ftype
    End Sub

    Private Function InterpolatedCutoff() As Double
        ' Keep your existing 6581/8580 curves for the SVF path
        If Mode6581 Then
            If parent.FilterCurve = ShitSID.FilterCurveType.Dark Then Return cutoffCurve6581Dark(cutoffVal).Item2
            If parent.FilterCurve = ShitSID.FilterCurveType.Average Then Return cutoffCurve6581(cutoffVal).Item2
            If parent.FilterCurve = ShitSID.FilterCurveType.Bright Then Return cutoffCurve6581Bright(cutoffVal).Item2
            Return cutoffCurve6581(cutoffVal).Item2
        Else
            Return cutoffCurve8580(cutoffVal).Item2
        End If
    End Function
    Public Function ApplyFilter(input As Double) As Double
        ' === Basic filter math ===
        Dim freq = (InterpolatedCutoff() * CutoffMultiplier) + CutoffBias
        freq = Math.Clamp(freq, 1, sampleRate / 2 - 1)

        Dim f As Double = 2 * Math.Sin(Math.PI * freq / sampleRate)
        f = Math.Max(f, 0.0001)
        Dim q As Double = 1.0 - (resonance / ResonanceDivider)
        q = Math.Clamp(q, 0.01, 0.99)

        ' === State variable filter ===
        Dim hp As Double = input - lowpass - q * bandpass
        Dim bp As Double = bandpass + f * hp
        Dim lp As Double = lowpass + f * bp

        highpass = hp
        bandpass = bp
        lowpass = lp

        ' === Combine outputs according to filter type ===
        Dim output As Double = 0.0
        If (filterType And EFilterType.LowPass) <> 0 Then output += lowpass
        If (filterType And EFilterType.BandPass) <> 0 Then output += bandpass
        If (filterType And EFilterType.HighPass) <> 0 Then output += highpass

        ' === Nonlinear SID-style behaviour ===
        If Mode6581 Then
            ' 1. Slight signal-dependent cutoff shift (MOSFET resistance)
            Dim modAmount As Double = 0.001 * Math.Abs(output)
            f *= 1.0 + modAmount

            ' 2. Asymmetric, amplitude-dependent distortion (more top than bottom)
            Dim asym As Double = 0.5              ' negative = compress negative peaks more
            Dim drive As Double = 1.0               ' more drive for distortion
            Dim clipInput As Double = output * drive * (1 + If(output > 0, asym, -asym))

            ' 3. Nonlinear “soft distortion” curve
            '    x / (1 + |x|) gives smooth saturation but still allows big peaks
            Dim shaped As Double = clipInput ' / (1 + Math.Abs(clipInput))

            ' Optional: add a little cubic for more harmonic content
            shaped += ((0.05 * (freq / 15000)) + 0.02) * clipInput ^ 3

            ' 4. High-frequency bias sag (HP gain droop)
            Dim biasHF As Double = Math.Sqrt(freq / (sampleRate / 2))
            shaped *= (1.0 - 0.25 * biasHF)

            output = shaped
        End If

        Return output
    End Function
    Function SoftClip(x As Double) As Double
        If x < -1 Then
            Return -2 / 3
        ElseIf x > 1 Then
            Return 2 / 3
        Else
            Return x - (x ^ 3) / 3
        End If
    End Function
    Public Sub Reset()
        bandpass = 0
        lowpass = 0
        highpass = 0
    End Sub
End Class
Public Class SidNoiseLFSR
    ' 23-bit shift register (SID initial state, all ones except LSBs)
    Private noiseSR As Integer = &H7FFFFC

    ' Track previous accumulator to detect rising edge
    Private prevAcc As Integer = 0

    ' Bit mask for rising edge detection (SID uses bit 19)
    Private Const BIT19 As Integer = &H80000

    ' Call once per sample, passing the oscillator accumulator (24-bit integer)
    Public Function Read(acc As Integer) As Integer
        ' Detect rising edge of accumulator bit 19
        'If ((prevAcc And BIT19) = 0) AndAlso ((acc And BIT19) <> 0) Then
        Shift()
        'End If

        prevAcc = acc

        ' Convert shift register state to SID-style 8-bit noise
        Dim noise8 As Integer =
    ((noiseSR >> 22) And 1) << 7 Or
    ((noiseSR >> 20) And 1) << 6 Or
    ((noiseSR >> 16) And 1) << 5 Or
    ((noiseSR >> 13) And 1) << 4 Or
    ((noiseSR >> 11) And 1) << 3 Or
    ((noiseSR >> 7) And 1) << 2 Or
    ((noiseSR >> 4) And 1) << 1 Or
    ((noiseSR >> 2) And 1)

        ' Convert to 12-bit output (0–4095)
        Return (noise8 << 4) ' multiply by 16
    End Function

    ' Single SID noise shift step
    Private Sub Shift()
        ' New bit = XOR taps at bit22 and bit17
        Dim newBit As Integer = ((noiseSR >> 22) Xor (noiseSR >> 17)) And 1
        noiseSR = ((noiseSR << 1) And &H7FFFFF) Or newBit ' Keep 23 bits
    End Sub
End Class