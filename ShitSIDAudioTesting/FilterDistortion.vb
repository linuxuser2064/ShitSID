Public Class FilterDistortion
    Dim ImgBuf As New Bitmap(768, 768)
    Dim g As Graphics = Graphics.FromImage(ImgBuf)
    Dim filt As New Filter6581
    Dim Cutoff As UInteger = 0 ' 0-2047
    Dim Resonance As Byte = 0 ' 0-15
    Dim signalMultiplier As Double = 750000
    Private Sub FilterDistortion_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PictureBox1.Image = ImgBuf
        RunFilter()
    End Sub

    Public Sub RunFilter()
        filt.reset()
        filt.setClockFrequency(153600)
        filt.setCurveAndDistortionDefaults()
        filt.writeRES_FILT((Resonance << 4) Or &B1000)
        filt.writeFC_HI(Cutoff >> 3)
        filt.writeFC_LO(Cutoff And &B111)
        filt.writeMODE_VOL(&B11111)
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
            filt.Clock(0, 0, 0, waveForm(i) * signalMultiplier)
        Next
        ' pass 2
        g.Clear(Color.Black)
        For i = 0 To 767
            OutputWaveForm(i) = filt.Clock(0, 0, 0, waveForm(i) * signalMultiplier) / signalMultiplier
            Dim y = SIDFilter.Clamp((-OutputWaveForm(i) * 225) + 384, 0, 767)
            Dim prevY = SIDFilter.Clamp((-OutputWaveForm(Math.Max(i - 1, 0)) * 225) + 384, 0, 767)
            'ImgBuf.SetPixel(i, y, Color.White)
            g.DrawLine(New Pen(Color.White, 2), i - 1, CInt(Math.Floor(y)), i, CInt(Math.Floor(prevY)))
        Next
        PictureBox1.Invalidate()
    End Sub

    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        Cutoff = TrackBar1.Value * 20.47
        RunFilter()
    End Sub

    Private Sub TrackBar2_Scroll(sender As Object, e As EventArgs) Handles TrackBar2.Scroll
        Resonance = TrackBar2.Value
        RunFilter()
    End Sub
End Class