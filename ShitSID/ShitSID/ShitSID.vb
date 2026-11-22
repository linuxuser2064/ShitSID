Public Class ShitSID
    Enum FilterCurveType
        Dark
        Average
        Bright
    End Enum
    <HideInStats>
    Public Voices(2) As Voice

    Public SampleRate As Int32 = 44100
    'Public CurrentTime As System.Decimal = 0

    Public Filter As SIDFilter
    Public FilterCurve As FilterCurveType = FilterCurveType.Average
    Private FilterMultiplier As Double = 330000.0
    Private FilterBias As Double = 0
    Public Mode6581 As Boolean = False

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

    <HideInStats>
    Dim SIDRegisters(31) As Byte
    Public Sub New(Optional samplerate As Int32 = 44100)
        Me.SampleRate = samplerate
        Filter = New SIDFilter(Me, samplerate * 2) ' run double oversampling

        For i As Integer = 0 To 2
            Voices(i) = New Voice(Me, i)
            SIDRegisters(i) = 0
        Next
        For i = 3 To 31 ' holy cycle saving
            SIDRegisters(i) = 0
        Next
    End Sub
    Public Overridable Sub Clock()
        'CurrentTime += 1.0 / ClocksPerFrame
        For i = 0 To 2
            Voices(i).Clock()
            Voices(i).Envelope.Clock()
        Next
    End Sub
    Public Overridable Function GetResonance() As Byte
        Return Filter.resonance * 15
    End Function
    Public Overridable Function GetCutoff() As Short
        Return Filter.cutoffVal
    End Function
    Public Overridable Function GetCutoffHz() As Double
        Return Filter.InterpolatedCutoff
    End Function
    Public Overridable Function LowPassSample(x As Double, cutoffFreq As Double, sampleRate As Double) As Double
        ' Low-pass state
        Static lpLastOutput As Double = 0

        ' High-pass state
        Static hpLastOutput As Double = 0
        Static hpLastInput As Double = 0

        ' ----- LOW PASS -----
        Dim lpOut As Double

        If sampleRate > cutoffFreq * 2 Then
            Dim dt As Double = 1.0 / sampleRate
            Dim rc As Double = 1.0 / (2 * Math.PI * cutoffFreq)
            Dim alpha As Double = dt / (rc + dt)

            lpLastOutput += alpha * (x - lpLastOutput)
            lpOut = lpLastOutput
        Else
            lpOut = x
        End If

        ' ----- HIGH PASS (fixed 10 Hz) -----
        Dim hpCutoff As Double = 10.0
        Dim hpOut As Double

        If sampleRate > hpCutoff * 2 Then
            Dim dt As Double = 1.0 / sampleRate
            Dim rc As Double = 1.0 / (2 * Math.PI * hpCutoff)
            Dim alpha As Double = rc / (rc + dt)

            hpOut = alpha * (hpLastOutput + lpOut - hpLastInput)

            hpLastInput = lpOut
            hpLastOutput = hpOut
        Else
            hpOut = lpOut
        End If

        Return hpOut
    End Function
    Public Overridable Function GetSample() As Double
        Dim output As Double = 0
        Dim filterInput As Double = 0
        For i = 0 To 2
            Dim v = Voices(i)
            Dim generated = v.Generate() ' generate anyway for ringmod accuracy
            If v.UseFilter AndAlso BypassFilter = False Then
                filterInput += generated
            Else
                If Not (MuteVoice3 AndAlso i = 2) Then
                    output += generated
                End If
            End If
        Next
        If Not BypassFilter Then
            output += {Filter.ApplyFilter(filterInput), Filter.ApplyFilter(filterInput)}.Average
        End If
        Dim PCMAttenuation = 0.1875
        If (Voices(0).UseFilter Or Voices(1).UseFilter Or Voices(2).UseFilter) And
            (Not (Voices(0).UseFilter And Voices(1).UseFilter And Voices(2).UseFilter)) Then
            PCMAttenuation *= 0.275
        End If
        If Not MuteSamples Then
            If Not VolumeSampleMode Then
                output *= VolumeRegister / 15.0
                output += (VolumeRegister - 15) * PCMAttenuation
            Else
                output += (VolumeRegister - 15) * PCMAttenuation * 1.33
            End If
        End If
        output /= 4
        If InternalAudioFilter Then
            Return LowPassSample(output, 20154, SampleRate)
        Else
            Return output
        End If
    End Function

    Public Overridable Function ReadRegister(addr As Integer) As Byte
        Select Case addr
            Case 54299
                Return (Voices(2).GenerateNoEnvelope() * 96) + 127
            Case 54300
                Return Voices(2).Envelope.Output
            Case Else
                Return 0
        End Select
    End Function
    Public Overridable Sub WriteRegister(addr As Integer, value As Byte)
        SIDRegisters(addr - &HD400) = value
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
            Case 1 ' FREQ HI
                voice.FreqHi = value
            Case 2 ' PW LO
                voice.PulseWidthLo = value
                voice.UpdateDutyCycle()
            Case 3 ' PW HI
                voice.PulseWidthHi = value
                voice.UpdateDutyCycle()
            Case 4 ' CONTROL REG
                voice.Control = value
                voice.Envelope.WriteControlReg(value)
                voice.Waveform = (value And &HF0) >> 4
            Case 5 ' ATTACK/DECAY
                voice.Envelope.WriteAttackDecay(value)
            Case 6 ' SUSTAIN/RELEASE
                voice.Envelope.WriteSustainRelease(value)
        End Select
    End Sub
    Protected Overridable Sub UpdateFilterSettings()
        Dim msbPart As Integer = FilterCutoffHi << 3
        Dim lsbPart As Integer = FilterCutoffLo And &H7 ' 11 bit is cursed
        Dim combined As UShort = CUInt(msbPart Or lsbPart)
        Dim cutoff = combined
        Filter.SetCutoff(cutoff)
        Filter.SetResonance(FilterResonance)
        Dim mode As SIDFilter.EFilterType = SIDFilter.EFilterType.None
        If (FilterMode And 1) <> 0 Then mode = mode Or SIDFilter.EFilterType.LowPass
        If (FilterMode And 2) <> 0 Then mode = mode Or SIDFilter.EFilterType.BandPass
        If (FilterMode And 4) <> 0 Then mode = mode Or SIDFilter.EFilterType.HighPass
        Filter.SetFilterType(mode)
    End Sub
    Public Overridable Sub Reset()
        'CurrentTime = 0
        Filter = New SIDFilter(Me, SampleRate)
        For i As Integer = 0 To 2
            Voices(i) = New Voice(Me, i)
        Next
    End Sub
End Class
