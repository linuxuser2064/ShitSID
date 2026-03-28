Public Class ShitSID
    Enum FilterCurveType
        Dark
        Average
        Bright
    End Enum
    Public Voices(2) As Voice

    Public SampleRate As Int32 = 44100
    Public CurrentTime As System.Decimal = 0

    Public Filter As SIDFilter
    'Public Filter As Filter6581
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

    Dim SIDRegisters(31) As Byte
    Public Sub New(Optional samplerate As Int32 = 44100)
        Me.SampleRate = samplerate
        Filter = New SIDFilter(Me, samplerate * 2) ' run double oversampling
        'Filter = New Filter6581()
        'Filter.Reset()
        'Filter.SetClockFrequency(ClocksPerFrame)   ' PAL clock

        For i As Integer = 0 To 2
            Voices(i) = New Voice(Me, i)
            SIDRegisters(i) = 0
        Next
        For i = 3 To 31 ' holy cycle saving
            SIDRegisters(i) = 0
        Next
    End Sub
    Public Sub Clock()
        CurrentTime += 1.0 / ClocksPerFrame
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
            Dim generated = v.Generate(CurrentTime) ' generate anyway for ringmod accuracy
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
        If Not MuteSamples Then
            If Not VolumeSampleMode Then
                output *= VolumeRegister / 15.0
                output += (VolumeRegister - 15) / 6
            Else
                output += (VolumeRegister - 15) / 6
            End If
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
                    voice.Waveform = "pulse"
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
        'Dim msbPart As Integer = FilterCutoffHi << 3
        'Dim lsbPart As Integer = FilterCutoffLo And &H7
        'Dim cutoff As UShort = CUShort(msbPart Or lsbPart)

        '' Pack into the two FC register bytes the way the hardware does
        ''Console.WriteLine($"Lo: {CByte(cutoff And &H7)}  Hi: {CByte((cutoff >> 3) And &HFF)}")
        'Filter.WriteFC_LO(CByte(cutoff And &H7))
        'Filter.WriteFC_HI(CByte((cutoff >> 3) And &HFF))

        '' Pack resonance + voice routing into RES_FILT byte
        'Dim res_filt As Byte = CByte((FilterResonance And &HF) << 4)
        'If Voices(0).UseFilter Then res_filt = res_filt Or &H1
        'If Voices(1).UseFilter Then res_filt = res_filt Or &H2
        'If Voices(2).UseFilter Then res_filt = res_filt Or &H4
        'Filter.WriteRES_FILT(res_filt)

        '' Pack mode + volume into MODE_VOL byte
        'Dim mode_vol As Byte = VolumeRegister And &HF
        'If (FilterMode And SIDFilter.EFilterType.LowPass) <> 0 Then mode_vol = mode_vol Or &H10
        'If (FilterMode And SIDFilter.EFilterType.BandPass) <> 0 Then mode_vol = mode_vol Or &H20
        'If (FilterMode And SIDFilter.EFilterType.HighPass) <> 0 Then mode_vol = mode_vol Or &H40
        'If MuteVoice3 Then mode_vol = mode_vol Or &H80   ' bit 7 = voice3 mute (inverted in WriteMODE_VOL)
        'Filter.WriteMODE_VOL(mode_vol)
    End Sub
    Public Sub Reset()
        CurrentTime = 0
        Filter = New SIDFilter(Me, SampleRate)
        'Filter = New Filter6581()
        'Filter.SetClockFrequency(985248.0)
        For i As Integer = 0 To 2
            Voices(i) = New Voice(Me, i)
        Next
    End Sub
End Class
