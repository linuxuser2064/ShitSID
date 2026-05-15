Public Class SIDFilter
    <Flags>
    Public Enum EFilterType
        None = 0
        LowPass = 1
        BandPass = 2
        HighPass = 4
    End Enum
    Public Mode6581 As Boolean = False
    'Public parent As ShitSID
    Public filterType As EFilterType = EFilterType.LowPass
    Public resonance As Double = 0.0 ' 0.0 to 1.0
    Public cutoffVal As Integer = 0          ' 0..2047 (11-bit)
    Private sampleRate As Double = 44100.0

    ' State for your SVF path
    Private bandpass As Double = 0.0
    Private lowpass As Double = 0.0
    Private highpass As Double = 0.0

    Public CutoffMultiplier As Double = 1
    Public CutoffBias As Int32 = 0
    Public ResonanceDivider As Double = 1
    Public Sub New(Optional sampleRate As Double = 44100.0)
        'parent = pnt
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

    Public Function InterpolatedCutoff() As Double
        ' Keep your existing 6581/8580 curves for the SVF path
        If Mode6581 Then
            'If parent.FilterCurve = ShitSID.FilterCurveType.Dark Then Return cutoffCurve6581Dark(cutoffVal).Item2
            'If parent.FilterCurve = ShitSID.FilterCurveType.Average Then Return cutoffCurve6581(cutoffVal).Item2
            'If parent.FilterCurve = ShitSID.FilterCurveType.Bright Then Return cutoffCurve6581Bright(cutoffVal).Item2
            Return cutoffCurve6581(cutoffVal).Item2
        Else
            Return cutoffCurve8580(cutoffVal).Item2
        End If
    End Function
    Public Shared Function Clamp(val, min, max)
        If val < min Then Return min
        If val > max Then Return max
        Return val
    End Function
    Public Function ApplyFilter(input As Double) As Double
        ' === Basic filter math ===
        Dim freq = (InterpolatedCutoff() * CutoffMultiplier) + CutoffBias
        freq = Clamp(freq, 1, sampleRate / 2 - 1)

        Dim f As Double = 2 * Math.Sin(Math.PI * freq / sampleRate)

        ' 1. VCR MODULATION: The audio input directly modulates the cutoff.
        ' We do this BEFORE the feedback loop.
        If Mode6581 Then
            Dim vcrModAmount As Double = 0.05 ' Tweak this scalar
            Dim impact = 0.5
            f += (Math.Max(input, impact) - impact) * vcrModAmount
        End If

        f = Clamp(f, 0.0001, 0.99) ' Keep it stable

        Dim q As Double = 1.0 - (resonance / ResonanceDivider)
        q = Clamp(q, 0.01, 0.99)

        ' 2. DC OFFSET: The 6581 ground is terrible.
        Dim internalInput As Double = input
        'Const InternalOffset As Double = -0.3
        'If Mode6581 Then
        '    internalInput += InternalOffset ' Push the signal off-center so it hits the ceiling first
        'End If

        ' === State variable filter with INSIDE-THE-LOOP distortion ===
        Dim distortedBP = bandpass

        Dim hp As Double =
    internalInput - lowpass - q * distortedBP

        Dim bp = bandpass + f * NMOSStage(hp, bandpass)

        Dim lp = lowpass + f * NMOSStage(bp, lowpass)
        'If Mode6581 Then
        '    ' 3. ASYMMETRIC CLIPPING INSIDE THE INTEGRATORS
        '    ' This is what causes the chaotic bifurcations (octave drops)
        '    'bp = SoftSign(bp)
        '    Const divider = 6
        '    lp = SoftClipFeedback(lp / divider) * divider
        'End If

        highpass = hp
        bandpass = bp
        lowpass = lp

        ' === Combine outputs ===
        Dim output As Double = 0.0
        If (filterType And EFilterType.LowPass) <> 0 Then output += lowpass
        If (filterType And EFilterType.BandPass) <> 0 Then output += bandpass
        If (filterType And EFilterType.HighPass) <> 0 Then output += highpass

        ' Remove the DC offset so your final audio isn't resting above zero
        If Mode6581 Then
            'Const divider = 1.3
            'output = SoftClipFeedback(output / divider) * divider
            'output -= InternalOffset
        End If

        Return output
    End Function
    Function NMOSStage(input As Double,
                   state As Double) As Double

        ' Shift bias point
        Dim x = input - 1

        ' Gain collapses away from threshold
        Dim gain =
        1.0 / Math.Sqrt(1 + x * x * 0.4)

        Return x * gain + 0.85
    End Function
    Public Sub Reset()
        bandpass = 0
        lowpass = 0
        highpass = 0
    End Sub
End Class
