Public Class PSGView
    Public Property SID As ShitSID
    Public Property Framebuffer As New Bitmap(256, 256)
    Dim g As Graphics = Graphics.FromImage(Framebuffer)
    Dim CommonFont As New Font("GenericLCD", 12, FontStyle.Regular)

    Dim SectionLabelBrush As New SolidBrush(Color.FromArgb(0, 127, 14))
    Dim EnabledOptionBrush As New SolidBrush(Color.FromArgb(0, 74, 127))
    Dim BarBackgroundBrush As New SolidBrush(Color.FromArgb(0, 7, 51))

    Dim ScrollbarForegroundBrush As New SolidBrush(Color.FromArgb(0, 19, 127))
    Dim ScrollbarTrackInnerPen As New Pen(Color.FromArgb(0, 255, 33))
    Dim ScrollbarTrackOuterPen As Pen = New Pen(SectionLabelBrush)

    Dim ProgressBarForegroundBrush As New SolidBrush(Color.FromArgb(0, 148, 255))

    Public Sub New(iSID As ShitSID)
        SID = iSID
        g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit
    End Sub
    Private Function cX(x As Integer) As Integer
        Return x - 3
    End Function
    Private Function cY(y As Integer) As Integer
        Return y - 4
    End Function
    Public Function Frame() As Bitmap
        g.Clear(Color.Black)
        '
        '  Draw channel 1
        '
        g.FillRectangle(SectionLabelBrush, 0, 0, 57, 11)
        g.DrawString("Channel 1:", CommonFont, Brushes.White, cX(2), cY(2))

        g.DrawString("Wave:", CommonFont, Brushes.White, cX(2), cY(14))
        ' note to self why did i have to make it this fucking way
        If SID.Voices(0).Waveform.Contains("tri") Then g.FillRectangle(EnabledOptionBrush, 35, 13, 16, 9)
        If SID.Voices(0).Waveform.Contains("saw") Then g.FillRectangle(EnabledOptionBrush, 52, 13, 16, 9)
        If SID.Voices(0).Waveform.Contains("square") Then g.FillRectangle(EnabledOptionBrush, 69, 13, 16, 9)
        If SID.Voices(0).Waveform.Contains("noise") Then g.FillRectangle(EnabledOptionBrush, 86, 13, 16, 9)
        g.DrawImageUnscaled(My.Resources.PSGViewResources.waveform_tri, 36, 14)
        g.DrawImageUnscaled(My.Resources.PSGViewResources.waveform_saw, 53, 14)
        g.DrawImageUnscaled(My.Resources.PSGViewResources.waveform_pulse, 70, 14)
        g.DrawImageUnscaled(My.Resources.PSGViewResources.waveform_noise, 87, 14)

        If SID.Voices(0).Control And &H4 Then g.FillRectangle(EnabledOptionBrush, 114, 13, 43, 9)
        If SID.Voices(0).Control And &H2 Then g.FillRectangle(EnabledOptionBrush, 164, 13, 27, 9)
        If SID.Voices(0).UseFilter Then g.FillRectangle(EnabledOptionBrush, 198, 13, 35, 9)
        g.DrawString("Ringmod", CommonFont, Brushes.White, cX(116), cY(14))
        g.DrawString("Sync", CommonFont, Brushes.White, cX(166), cY(14))
        g.DrawString("Filter", CommonFont, Brushes.White, cX(200), cY(14))

        g.DrawString("Duty:", CommonFont, Brushes.White, cX(2), cY(24))
        ' ---
        Dim filled = CInt(SID.Voices(0).DutyCycle * 160)
        g.FillRectangle(ScrollbarForegroundBrush, 35, 24, CInt(SID.Voices(0).DutyCycle * 160), 7)
        Dim remainder = 160 - filled
        g.FillRectangle(BarBackgroundBrush, 194 - remainder, 24, remainder, 7)

        g.DrawRectangle(ScrollbarTrackOuterPen, 35 + filled - 1, 23, 2, 9)
        g.DrawLine(ScrollbarTrackInnerPen, 35 + filled, 24, 35 + filled, 31)
        g.DrawString(Math.Floor(SID.Voices(0).DutyCycle * 4095), CommonFont, Brushes.White, cX(200), cY(24))


        Return Framebuffer
    End Function
End Class
