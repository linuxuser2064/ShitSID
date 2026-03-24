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
    Public Sub DrawChannel(idx As Integer, yOffset As Integer)
        g.FillRectangle(SectionLabelBrush, 0, yOffset, 57, 11)
        g.DrawString($"Channel {idx + 1}:", CommonFont, Brushes.White, cX(2), cY(2 + yOffset))

        g.DrawString("Wave:", CommonFont, Brushes.White, cX(2), cY(14 + yOffset))
        ' note to self why did i have to make it this fucking way
        If SID.Voices(idx).Waveform.Contains("tri") Then g.FillRectangle(EnabledOptionBrush, 35, 13 + yOffset, 16, 9)
        If SID.Voices(idx).Waveform.Contains("saw") Then g.FillRectangle(EnabledOptionBrush, 52, 13 + yOffset, 16, 9)
        If SID.Voices(idx).Waveform.Contains("square") Then g.FillRectangle(EnabledOptionBrush, 69, 13 + yOffset, 16, 9)
        If SID.Voices(idx).Waveform.Contains("noise") Then g.FillRectangle(EnabledOptionBrush, 86, 13 + yOffset, 16, 9)
        g.DrawImageUnscaled(My.Resources.PSGViewResources.waveform_tri, 36, 14 + yOffset)
        g.DrawImageUnscaled(My.Resources.PSGViewResources.waveform_saw, 53, 14 + yOffset)
        g.DrawImageUnscaled(My.Resources.PSGViewResources.waveform_pulse, 70, 14 + yOffset)
        g.DrawImageUnscaled(My.Resources.PSGViewResources.waveform_noise, 87, 14 + yOffset)

        If SID.Voices(idx).Control And &H4 Then g.FillRectangle(EnabledOptionBrush, 114, 13 + yOffset, 43, 9)
        If SID.Voices(idx).Control And &H2 Then g.FillRectangle(EnabledOptionBrush, 164, 13 + yOffset, 27, 9)
        If SID.Voices(idx).UseFilter Then g.FillRectangle(EnabledOptionBrush, 198, 13 + yOffset, 35, 9)
        g.DrawString("Ringmod", CommonFont, Brushes.White, cX(116), cY(14 + yOffset))
        g.DrawString("Sync", CommonFont, Brushes.White, cX(166), cY(14 + yOffset))
        g.DrawString("Filter", CommonFont, Brushes.White, cX(200), cY(14 + yOffset))

        g.DrawString("Duty:", CommonFont, Brushes.White, cX(2), cY(24 + yOffset))
        ' ---
        Dim filled = CInt(SID.Voices(idx).DutyCycle * 160)
        g.FillRectangle(ScrollbarForegroundBrush, 35, 24 + yOffset, CInt(SID.Voices(idx).DutyCycle * 160), 7)
        Dim remainder = 160 - filled
        g.FillRectangle(BarBackgroundBrush, 194 - remainder, 24 + yOffset, remainder, 7)

        g.DrawRectangle(ScrollbarTrackOuterPen, 35 + filled - 1, 23 + yOffset, 2, 9)
        g.DrawLine(ScrollbarTrackInnerPen, 35 + filled, 24 + yOffset, 35 + filled, 31 + yOffset)
        g.DrawString(Math.Floor(SID.Voices(idx).DutyCycle * 4095), CommonFont, Brushes.White, cX(200), cY(24 + yOffset))


        g.DrawString("Pitch:", CommonFont, Brushes.White, cX(2), cY(34 + yOffset))
        ' ---
        Const magic = 2.30102999566
        Dim filledD As Double = 160.0 * (Math.Log10(SID.Voices(idx).Frequency / 20) / magic)
        If filledD = Double.NegativeInfinity Then filledD = 0
        If filledD < 0 Then filledD = 0
        g.FillRectangle(ScrollbarForegroundBrush, 35, 34 + yOffset, CInt(filledD), 7)
        remainder = 160 - CInt(filledD)
        g.FillRectangle(BarBackgroundBrush, 194 - remainder, 34 + yOffset, remainder, 7)

        g.DrawRectangle(ScrollbarTrackOuterPen, 35 + CInt(filledD) - 1, 33 + yOffset, 2, 9)
        g.DrawLine(ScrollbarTrackInnerPen, 35 + CInt(filledD), 34 + yOffset, 35 + CInt(filledD), 41 + yOffset)
        g.DrawString(Math.Floor(SID.Voices(idx).Frequency), CommonFont, Brushes.White, cX(200), cY(34 + yOffset))
        g.DrawString("Hz", CommonFont, Brushes.White, cX(239), cY(34 + yOffset))

        g.DrawString("ADSR:", CommonFont, Brushes.White, cX(2), cY(44 + yOffset))
        ' ---
        filled = CInt(SID.Voices(idx).Envelope.Output / 5.3125)
        remainder = 48 - filled
        g.FillRectangle(ProgressBarForegroundBrush, 35, 44 + yOffset, filled, 7)
        g.FillRectangle(BarBackgroundBrush, 83 - remainder, 44 + yOffset, remainder, 7)
        g.DrawString(
            $"Lvl {SID.Voices(idx).Envelope.Output}",
            CommonFont, Brushes.White, cX(89), cY(44 + yOffset))
        g.DrawString($"Params {SID.Voices(idx).Envelope.Attack.ToString("X")}{SID.Voices(idx).Envelope.Decay.ToString("X")}{(SID.Voices(idx).Envelope.Sustain \ 15.9375).ToString("X")}{SID.Voices(idx).Envelope.Release.ToString("X")}",
CommonFont, Brushes.White, cX(137), cY(44 + yOffset))
    End Sub
    Public Function Frame() As Bitmap
        g.Clear(Color.Black)
        DrawChannel(0, 0)
        DrawChannel(1, 54)
        DrawChannel(2, 108)

        '
        ' Draw filter and master volume graph
        '
        g.FillRectangle(SectionLabelBrush, 0, 162, 38, 11)
        g.DrawString($"Filter:", CommonFont, Brushes.White, cX(2), cY(164))

        g.DrawString("Mode:", CommonFont, Brushes.White, cX(2), cY(176))
        If SID.Filter.filterType.HasFlag(SIDFilter.EFilterType.LowPass) Then g.FillRectangle(EnabledOptionBrush, 43, 175, 16, 9)
        If SID.Filter.filterType.HasFlag(SIDFilter.EFilterType.BandPass) Then g.FillRectangle(EnabledOptionBrush, 60, 175, 16, 9)
        If SID.Filter.filterType.HasFlag(SIDFilter.EFilterType.HighPass) Then g.FillRectangle(EnabledOptionBrush, 77, 175, 16, 9)
        g.DrawImageUnscaled(My.Resources.PSGViewResources.filt_lp, 44, 176)
        g.DrawImageUnscaled(My.Resources.PSGViewResources.filt_bp, 61, 176)
        g.DrawImageUnscaled(My.Resources.PSGViewResources.filt_hp, 78, 176)

        g.DrawString("Resonance:", CommonFont, Brushes.White, cX(98), cY(176))
        Dim filled = CInt(SID.Filter.resonance * 48)
        Dim remainder = 48 - filled
        g.FillRectangle(ProgressBarForegroundBrush, 156, 176, filled, 7)
        g.FillRectangle(BarBackgroundBrush, 203 - remainder, 176, remainder, 7)
        g.DrawString(CInt(SID.Filter.resonance * 15), CommonFont, Brushes.White, cX(207), cY(176))

        g.DrawString("Cutoff:", CommonFont, Brushes.White, cX(2), cY(186))
        filled = CInt(SID.Filter.cutoffVal / 12.79375)
        g.FillRectangle(ScrollbarForegroundBrush, 43, 186, filled, 7)
        remainder = 160 - filled
        g.FillRectangle(BarBackgroundBrush, 202 - remainder, 186, remainder, 7)

        g.DrawRectangle(ScrollbarTrackOuterPen, 43 + filled - 1, 185, 2, 9)
        g.DrawLine(ScrollbarTrackInnerPen, 43 + filled, 186, 43 + filled, 186 + 7)
        g.DrawString(Math.Floor(SID.Filter.InterpolatedCutoff), CommonFont, Brushes.White, cX(208), cY(186))
        g.DrawString("Hz", CommonFont, Brushes.White, cX(239), cY(186))

        Return Framebuffer
    End Function
End Class
