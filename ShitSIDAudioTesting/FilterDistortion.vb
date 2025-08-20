Public Class FilterDistortion
    Dim ImgBuf As New Bitmap(768, 768)
    Dim g As Graphics = Graphics.FromImage(ImgBuf)
    Dim filt As New SIDFilter(153600)
    Dim Cutoff = 100 ' 0-2047
    Dim Resonance = 15 ' 0-15
    Private Sub FilterDistortion_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PictureBox1.Image = ImgBuf
        RunFilter()
    End Sub

    Public Sub RunFilter()
        filt.Mode6581 = True
        filt.Reset()
        filt.SetCutoff(Cutoff)
        filt.SetResonance(Resonance)
        Dim waveForm(768) As Double
        For i = 0 To 767
            waveForm(i) = -1
        Next
        For i = 192 To 576
            waveForm(i) = 1
        Next
        Dim OutputWaveForm(768) As Double
        ' pass 1
        For i = 0 To 767
            OutputWaveForm(i) = filt.ApplyFilter(waveForm(i))
        Next
        ' pass 2
        g.Clear(Color.Black)
        For i = 0 To 767
            OutputWaveForm(i) = filt.ApplyFilter(waveForm(i))
            Dim y = filt.Clamp((-OutputWaveForm(i) * 225) + 384, 0, 767)
            Dim prevY = filt.Clamp((-OutputWaveForm(Math.Max(i - 1, 0)) * 225) + 384, 0, 767)
            'ImgBuf.SetPixel(i, y, Color.White)
            g.DrawLine(New Pen(Color.White, 2), i, CInt(y), i - 1, CInt(prevY))
        Next
        PictureBox1.Invalidate()
    End Sub

    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        Cutoff = TrackBar1.Value * 10
        RunFilter()
    End Sub

    Private Sub TrackBar2_Scroll(sender As Object, e As EventArgs) Handles TrackBar2.Scroll
        Resonance = TrackBar2.Value
        RunFilter()
    End Sub
End Class




Public Class SIDFilter
    <Flags>
    Public Enum EFilterType
        None = 0
        LowPass = 1
        BandPass = 2
        HighPass = 4
    End Enum
    Public Function Clamp(orig, min, max)
        Return Math.Min(Math.Max(orig, min), max)
    End Function
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
    Dim logCount = 0
    Public distortionOffset As Double = -0.4
    Public distortionMult As Double = 0.7

    Public Function ApplyFilter(input As Double) As Double
        ' Calculate cutoff frequency and resonance
        Dim freq = InterpolatedCutoff()
        freq = Clamp(freq, 1, sampleRate / 2 - 1)

        ' f: frequency coefficient, q: resonance damping
        Dim f As Double = 2 * Math.Sin(Math.PI * freq / sampleRate)
        Dim q As Double = 1.0 - (resonance / ResonanceDivider)
        q = Clamp(q, 0.05, 0.99)

        ' Standard SVF structure
        Dim hp As Double = input - lowpass - q * bandpass
        Dim bp As Double = bandpass + f * hp
        Dim lp As Double = lowpass + f * bp

        ' Store state for next iteration
        highpass = hp
        bandpass = bp
        lowpass = lp

        ' Output based on selected filter mode
        Dim output As Double = 0.0
        If (filterType And EFilterType.LowPass) <> 0 Then output += lowpass
        If (filterType And EFilterType.BandPass) <> 0 Then output += bandpass
        If (filterType And EFilterType.HighPass) <> 0 Then output += highpass

        output = (Math.Tanh((output * distortionMult) + distortionOffset) - distortionOffset) / distortionMult

        Return output
    End Function

    Public Sub Reset()
        bandpass = 0
        lowpass = 0
        highpass = 0
    End Sub
End Class