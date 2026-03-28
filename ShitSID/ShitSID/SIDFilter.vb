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

    Public Function InterpolatedCutoff() As Double
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
        'lp = Math.Clamp(lp, -2, 2)
        'bp = Math.Clamp(bp, -2, 2)

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
            Dim asym As Double = 0.4              ' negative = compress negative peaks more
            Dim drive As Double = 1.0               ' more drive for distortion
            Dim clipInput As Double = output * drive * (1 + If(output > 0, asym, -asym))

            ' 3. Nonlinear “soft distortion” curve
            '    x / (1 + |x|) gives smooth saturation but still allows big peaks
            Dim shaped As Double = clipInput ' / (1 + Math.Abs(clipInput))

            ' Optional: add a little cubic for more harmonic content
            shaped += ((0.05 * (freq / 15000)) + 0.04) * clipInput ^ 3

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
