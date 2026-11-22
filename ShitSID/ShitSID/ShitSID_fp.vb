Public Class ShitSID_fp
    Inherits ShitSID
    'Enum FilterCurveType
    '    Dark
    '    Average
    '    Bright
    'End Enum
    'Public Voices(2) As Voice

    'Public SampleRate As Int32 = 44100
    'Public CurrentTime As System.Decimal = 0

    'Public Filter As SIDFilter
    Public FilterFP As Filter6581
    'Public FilterCurve As FilterCurveType = FilterCurveType.Average
    Private FilterMultiplier As Double = 330000.0
    Private FilterBias As Double = 0
    'Public Mode6581 As Boolean = True


    'Public FilterCutoffLo As Integer = 0
    'Public FilterCutoffHi As Integer = 0
    'Public FilterResonance As Integer = 0
    'Public FilterMode As Integer = 0 ' 0-7 (bit flags)
    'Public BypassFilter As Boolean = False

    'Public VolumeSampleMode As Boolean = True
    'Public VolumeRegister As Byte
    'Public MuteSamples As Boolean = False

    'Public MuteVoice3 As Boolean = False

    'Public InternalAudioFilter As Boolean = True

    Const ClocksPerFrame = 985248.0

    Dim SIDRegisters(31) As Byte
    Public Sub fp_setCurveAndDistortionDefaults()
        FilterFP.setCurveAndDistortionDefaults()
    End Sub

    Public Sub New(Optional samplerate As Int32 = 44100)
        Me.SampleRate = samplerate
        'Filter = New SIDFilter(Me, samplerate * 2) ' run double oversampling
        FilterFP = New Filter6581
        FilterFP.reset()
        FilterFP.setClockFrequency(samplerate * 2) ' oversampling

        For i As Integer = 0 To 2
            Voices(i) = New Voice(Me, i)
            SIDRegisters(i) = 0
        Next
        For i = 3 To 31 ' holy cycle saving
            SIDRegisters(i) = 0
        Next
    End Sub
    Public Overrides Sub Clock()
        'CurrentTime += 1.0 / ClocksPerFrame
        For i = 0 To 2
            Voices(i).Clock()
            Voices(i).Envelope.Clock()
        Next
    End Sub
    Public Overrides Function GetResonance() As Byte
        Return FilterFP.res ' / 15
    End Function
    Public Overrides Function GetCutoff() As Short
        Return FilterFP.fc
    End Function
    Public Overrides Function GetCutoffHz() As Double
        Return FilterFP.estimateFrequency
    End Function
    Public Overrides Function LowPassSample(x As Double, cutoffFreq As Double, sampleRate As Double) As Double
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
    Dim generateds(2) As Double
    Public Overrides Function GetSample() As Double
        Dim output As Double = 0
        For i = 0 To 2
            Dim v = Voices(i)
            Dim generated = v.Generate() ' generate anyway for ringmod accuracy
            generateds(i) = (generated + FilterBias) * FilterMultiplier
        Next
        If Not BypassFilter Then
            ' oversampling
            output += (FilterFP.Clock(generateds(0), generateds(1), generateds(2), 0) / FilterMultiplier +
                       FilterFP.Clock(generateds(0), generateds(1), generateds(2), 0) / FilterMultiplier) / 2
        Else
            output += (generateds(0) - FilterBias) / FilterMultiplier
            output += (generateds(1) - FilterBias) / FilterMultiplier
            output += (generateds(2) - FilterBias) / FilterMultiplier
        End If
        Dim PCMAttenuation = 0.1875
        If (Voices(0).UseFilter Or Voices(1).UseFilter Or Voices(2).UseFilter) And
            (Not (Voices(0).UseFilter And Voices(1).UseFilter And Voices(2).UseFilter)) Then ' actual logic based on observation

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

    Public Overrides Function ReadRegister(addr As Integer) As Byte
        Select Case addr
            Case 54299
                Return (Voices(2).GenerateNoEnvelope() * 96) + 127
            Case 54300
                Return Voices(2).Envelope.Output
            Case Else
                Return 0
        End Select
    End Function
    Public Overrides Sub WriteRegister(addr As Integer, value As Byte)
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
                MuteVoice3 = bits(7)
                VolumeRegister = value And &HF
                Return
        End Select

        Dim reg = addr And &H1F ' bitwise AND
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
    Protected Overrides Sub UpdateFilterSettings()
        FilterFP.writeFC_HI(SIDRegisters(&H16))
        FilterFP.writeFC_LO(SIDRegisters(&H15))
        FilterFP.writeRES_FILT(SIDRegisters(&H17))
        FilterFP.writeMODE_VOL(SIDRegisters(&H18))
    End Sub
    Public Overrides Sub Reset()
        'CurrentTime = 0
        FilterFP = New Filter6581
        For i As Integer = 0 To 2
            Voices(i) = New Voice(Me, i)
        Next
    End Sub
End Class
