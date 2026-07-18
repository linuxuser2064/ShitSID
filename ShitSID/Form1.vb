Imports System.IO
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Button
Imports FFMediaToolkit.Encoding
Imports FFmpeg.AutoGen
Imports Highbyte.DotNet6502
Imports NAudio.CoreAudioApi
Imports NAudio.MediaFoundation
Imports NAudio.Wave
Public Class Form1
    Public SAMPLERATE As Integer = 176400
    Public sid As ShitSID
    Public sid_fp As ShitSID_fp
    Public sidfile As SidFile
    Public fakeClockCount = 0
    Public cpu As New CPU
    Public mem As New Memory(65536)
    Private waveOut As WasapiOut = Nothing
    Private WithEvents provider As SidAudioProvider = Nothing
    Private PSGViewer As PSGView
    Private PSGViewForm As New PSGViewForm
    Private PSGViewFormHandle As IntPtr
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If OpenFileDialog1.ShowDialog = DialogResult.OK Then
            ' if this dies im blaming chatgp
            If waveOut IsNot Nothing Then
                waveOut.Stop()
                waveOut.Dispose()
                waveOut = Nothing
            End If
            LoadSID()
            PlaySID()
        End If
    End Sub
    Private Sub LoadSID()
        ' Reinitialize components
        If My.Settings.UseNewFilter Then
            sid = New ShitSID_fp(SAMPLERATE)
        Else
            sid = New ShitSID(SAMPLERATE)
        End If
        Dim LoadCSV = OpenFileDialog1.FileName.ToLower.EndsWith(".csv")
        Dim sidDump As SidTraceReader = Nothing
        If LoadCSV Then
            ' oh god
            sidDump = New SidTraceReader(OpenFileDialog1.FileName)
        End If
        'cpu.SP = 1
        If Not LoadCSV Then
            mem(1) = &H37
            sidfile = SidFile.Load(OpenFileDialog1.FileName)
            If sidfile.FlagBits(15 - 2) = False AndAlso sidfile.FlagBits(15 - 3) = True Then
                mem(&H2A6) = 1 ' PAL
                CheckBox5.Checked = False
                'NumericUpDown6.Value = 50
            ElseIf sidfile.FlagBits(15 - 2) = True AndAlso sidfile.FlagBits(15 - 3) = False Then
                CheckBox5.Checked = True
            End If

            If sidfile.FlagBits(15 - 4) = False AndAlso sidfile.FlagBits(15 - 5) = True Then
                ' 6581
                CheckBox6.Checked = True
            ElseIf sidfile.FlagBits(15 - 4) = True AndAlso sidfile.FlagBits(15 - 5) = False Then
                CheckBox6.Checked = False
            End If

            ' Set song number from UI or default
            If NumericUpDown1.Value > 0 Then
                cpu.A = NumericUpDown1.Value - 1
            Else
                cpu.A = sidfile.StartSong - 1
            End If
        End If
        sid.Voices(0).MuteVoice = CheckBox2.Checked
        sid.Voices(1).MuteVoice = CheckBox3.Checked
        sid.Voices(2).MuteVoice = CheckBox4.Checked
        sid.MuteSamples = CheckBox8.Checked
        For Each x As Voice In sid.Voices
            x.LoFiDuty = CheckBox1.Checked
        Next
        sid.Mode6581 = CheckBox6.Checked
        sid.InternalAudioFilter = AudioOutputSettings.CheckBox1.Checked
        If My.Settings.UseNewFilter Then
            Dim newFetResistance = 16125.1553
            If RadioButton3.Checked Then
                newFetResistance = 11961.908870403166
            End If
            If RadioButton4.Checked Then
                newFetResistance = 14299.149638099827
            End If
            If RadioButton5.Checked Then
                newFetResistance = 12914.5661141159
            End If
            DirectCast(sid, ShitSID_fp).FilterFP.setCurveProperties(NumericUpDown2.Value, NumericUpDown3.Value, NumericUpDown4.Value, newFetResistance)
        Else
            sid.Filter.CutoffBias = NumericUpDown3.Value
            sid.Filter.ResonanceDivider = NumericUpDown4.Value
            sid.Filter.CutoffMultiplier = NumericUpDown2.Value
        End If
        sid.BypassFilter = CheckBox7.Checked
        sid.VolumeSampleMode = RadioButton1.Checked
        If RadioButton3.Checked Then sid.FilterCurve = ShitSID.FilterCurveType.Dark
        If RadioButton4.Checked Then sid.FilterCurve = ShitSID.FilterCurveType.Average
        If RadioButton5.Checked Then sid.FilterCurve = ShitSID.FilterCurveType.Bright
        CheckBox6_CheckedChanged(Nothing, Nothing)
        If Not LoadCSV Then
            cpu.PC = sidfile.InitAddress
            Console.WriteLine($"Init address: {sidfile.InitAddress.ToString("X4")}")
        End If
        If CheckBox5.Checked Then
            NumericUpDown6.Value = 60
        End If
        PSGViewer = New PSGView(sid)
        PSGViewer.ShowPCMGraph = PSGViewVolumeGraphBox.Checked
        If Not LoadCSV Then
            provider = New SidAudioProvider(sid, cpu, mem, PSGViewer, SAMPLERATE)
        Else
            provider = New SidAudioProvider(sid, sidDump, PSGViewer, SAMPLERATE)
        End If
        provider.Volume = TrackBar1.Value / 100
        AddHandler provider.PSGViewFrame, AddressOf provider_PSGViewFrame
        provider.EnablePSGView = PSGViewEnableBox.Checked
        provider.PSGViewDivider = PSGViewDividerBox.Value
        provider.TickRate = NumericUpDown6.Value
        provider.UseNTSC = CheckBox5.Checked
        If PSGViewEnableBox.Checked Then
            PSGViewForm.Show()
            PSGViewFormHandle = PSGViewForm.Handle
        End If
    End Sub
    Public Sub PlaySID()
        waveOut = New WasapiOut(AudioClientShareMode.Shared, True, 4)
        provider.sidfile = sidfile
        provider.InitSIDFile()
        waveOut.Init(provider)
        waveOut.Play()
        'PSGViewTimer.Start()
        provider.runCPU = True
        For Each x As Control In Me.Controls
            x.Enabled = True
        Next
        PlayPauseState = True
        Button4.Text = "Pause"
    End Sub
    Public Sub pr(str As String)
        Console.WriteLine(str)
    End Sub
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If sid IsNot Nothing Then
            If CheckBox1.Checked Then
                For Each x As Voice In sid.Voices
                    x.LoFiDuty = True
                Next
            Else
                For Each x As Voice In sid.Voices
                    x.LoFiDuty = False
                Next
            End If
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        OpenFileDialog2.ShowDialog()
    End Sub

    Private Sub OpenFileDialog2_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog2.FileOk
        Dim out = ""
        Dim newSidfile = SidFile.Load(OpenFileDialog2.FileName)
        MsgBox($"{Path.GetFileName(OpenFileDialog2.FileName)} info:
Song name: {newSidfile.SongName} by {newSidfile.SongArtist}
Released by: {newSidfile.SongStudio}
Amount of songs: {newSidfile.Songs}, default song: {newSidfile.StartSong}")
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        StatsViewer.Show()
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged, CheckBox4.CheckedChanged, CheckBox3.CheckedChanged, CheckBox8.CheckedChanged
        If sid IsNot Nothing Then
            If CheckBox2.Checked Then
                sid.Voices(0).MuteVoice = True
            Else
                sid.Voices(0).MuteVoice = False
            End If
            If CheckBox3.Checked Then
                sid.Voices(1).MuteVoice = True
            Else
                sid.Voices(1).MuteVoice = False
            End If
            If CheckBox4.Checked Then
                sid.Voices(2).MuteVoice = True
            Else
                sid.Voices(2).MuteVoice = False
            End If
            sid.MuteSamples = CheckBox8.Checked
        End If
    End Sub

    Private Sub CheckBox5_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox5.CheckedChanged
        If sid IsNot Nothing Then
            If CheckBox5.Checked Then
                NumericUpDown6.Value = 60
            Else
                NumericUpDown6.Value = 50
            End If
            provider.TickRate = NumericUpDown6.Value
            provider.UseNTSC = CheckBox5.Checked
        End If
    End Sub

    Private Sub NumericUpDown3_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown2.ValueChanged, NumericUpDown3.ValueChanged, NumericUpDown4.ValueChanged
        If sid IsNot Nothing Then
            If My.Settings.UseNewFilter Then
                DirectCast(sid, ShitSID_fp).FilterFP.setCurveProperties(NumericUpDown2.Value, NumericUpDown3.Value, NumericUpDown4.Value, 16125.1553F)
            Else
                sid.Filter.CutoffBias = NumericUpDown3.Value
                sid.Filter.ResonanceDivider = NumericUpDown4.Value
                sid.Filter.CutoffMultiplier = NumericUpDown2.Value
            End If
        End If
    End Sub
    Private Sub CheckBox6_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox6.CheckedChanged
        If sid IsNot Nothing Then
            sid.Mode6581 = CheckBox6.Checked
            sid.Filter.Reset()
            If My.Settings.UseNewFilter Then
                DirectCast(sid, ShitSID_fp).FilterFP.reset()
            Else
                sid.Filter.Reset()
            End If
        End If
        RadioButton3.Enabled = CheckBox6.Checked
        RadioButton4.Enabled = CheckBox6.Checked
        RadioButton5.Enabled = CheckBox6.Checked
    End Sub
    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
#If DEBUG Then
        Using fs As New FileStream("ramdump.bin", FileMode.Create, FileAccess.Write)
            For i = 0 To 65535
                fs.WriteByte(mem(i))
            Next
        End Using
#End If
        'provider.sid.Filter.fs.Dispose()
    End Sub

    Private Sub CheckBox7_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox7.CheckedChanged
        If sid IsNot Nothing Then
            sid.BypassFilter = CheckBox7.Checked
        End If
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        If sid IsNot Nothing Then
            sid.VolumeSampleMode = RadioButton1.Checked
        End If
    End Sub
    Dim PlayPauseState As Boolean = False
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If PlayPauseState Then ' go to paused
            PlayPauseState = False
            Button4.Text = "Play"
            waveOut.Pause()
        Else
            PlayPauseState = True
            Button4.Text = "Pause"
            waveOut.Play()
        End If
    End Sub

    Dim stamp As New TimeSpan
    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        If Not OpenFileDialog1.ShowDialog = DialogResult.OK Then
            Exit Sub
        End If
        If Not SaveFileDialog1.ShowDialog = DialogResult.OK Then
            Exit Sub
        End If
        Me.Enabled = False
        Dim outputFolder = Path.GetDirectoryName(SaveFileDialog1.FileName)
        Dim outputFileNoExt = Path.GetFileNameWithoutExtension(SaveFileDialog1.FileName)
        Console.WriteLine("Init MF...")
        MediaFoundationApi.Startup()
        Console.WriteLine("Loading SID...")
        LoadSID()
        If PSGViewEnableBox.Checked Then
            RemoveHandler provider.PSGViewFrame, AddressOf provider_PSGViewFrame
            AddHandler provider.PSGViewFrame, AddressOf EncodeFrameHandler
        End If
        provider.sidfile = sidfile
        provider.InitSIDFile()
        Console.WriteLine("Creating MediaType...")
        Dim type As New MediaType(provider.WaveFormat)
        Console.WriteLine("Creating MF encoder...")
        Dim wavProv As New MediaFoundationEncoder(type)
        wavProv.DefaultReadBufferSize = SAMPLERATE \ NumericUpDown6.Value
        If PSGViewEnableBox.Checked Then
            Console.WriteLine("Creating video container...")
            Dim opts = New VideoEncoderSettings(512, 512, NumericUpDown6.Value, VideoCodec.H264) With {.Bitrate = 3200 * 1000, .EncoderPreset = EncoderPreset.Faster}
            opts.CodecOptions("profile") = "high444"
            opts.CodecOptions("pix_fmt") = "yuv444p"
            opts.EncoderPreset = EncoderPreset.Faster
            opts.Bitrate = 3200 * 1000
            opts.FramerateRational = New AVRational With {.num = NumericUpDown6.Value, .den = PSGViewDividerBox.Value}
            opts.VideoFormat = FFMediaToolkit.Graphics.ImagePixelFormat.Yuv444
            EncodeVid = MediaBuilder.CreateContainer(Path.Combine(outputFolder, $"{outputFileNoExt}.mp4"), ContainerFormat.MP4).
            WithVideo(opts).Create
        End If
        Console.WriteLine("Encoding...")
        provider.runCPU = True
        wavProv.Encode(Path.Combine(outputFolder, $"{outputFileNoExt}.wav"), provider.Take(TimeSpan.FromSeconds(NumericUpDown5.Value)).ToWaveProvider)
        If PSGViewEnableBox.Checked Then
            EncodeVid.Dispose()
            RemoveHandler provider.PSGViewFrame, AddressOf EncodeFrameHandler
            AddHandler provider.PSGViewFrame, AddressOf provider_PSGViewFrame
        End If
        Console.WriteLine("Done")
        Me.Enabled = True
    End Sub
    Private EncodeVid As MediaOutput
    Private Sub EncodeFrameHandler(frame As Bitmap)
        Dim data = frame.LockBits(New Rectangle(0, 0, 512, 512), Imaging.ImageLockMode.ReadOnly, Imaging.PixelFormat.Format32bppArgb)
        Try
            Console.WriteLine($"Video time: {stamp.ToString}")
            BitmapToImageData.BMPtoBitmapData.AddBitmapFrame(EncodeVid, data)
            stamp = stamp.Add(TimeSpan.FromMilliseconds(1000 / provider.TickRate)) ' hardcoded ahh
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        Finally
            frame.UnlockBits(data)
        End Try
    End Sub
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        AudioOutputSettings.ShowDialog()
    End Sub
    Private Sub RadioButton5_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton5.CheckedChanged, RadioButton4.CheckedChanged, RadioButton3.CheckedChanged
        If My.Settings.UseNewFilter Then
            Dim newFetResistance = 16125.1553
            If RadioButton3.Checked Then
                NumericUpDown2.Value = 1399768.3253307983
                NumericUpDown3.Value = 553018906.8926692
                NumericUpDown4.Value = 1.0051493199361266
                newFetResistance = 11961.908870403166
            End If
            If RadioButton4.Checked Then
                NumericUpDown2.Value = 1522171.9229830841
                NumericUpDown3.Value = 21729926.667291082
                NumericUpDown4.Value = 1.0049948025374751
                newFetResistance = 14299.149638099827
            End If
            If RadioButton5.Checked Then
                NumericUpDown2.Value = 1164920.4999651583
                NumericUpDown3.Value = 12915042.165290257
                NumericUpDown4.Value = 1.0058853753357735
                newFetResistance = 12914.5661141159
            End If

            If sid IsNot Nothing Then
                Dim newsid = DirectCast(sid, ShitSID_fp)
                newsid.FilterFP.setCurveAndDistortionDefaults()
                newsid.FilterFP.setCurveProperties(NumericUpDown2.Value, NumericUpDown3.Value, NumericUpDown4.Value, newFetResistance)
            End If
        End If
        'End If
    End Sub

    Private Sub NumericUpDown6_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown6.ValueChanged
        If sid IsNot Nothing AndAlso provider IsNot Nothing Then
            provider.TickRate = NumericUpDown6.Value
        End If
    End Sub
    Private Sub provider_PSGViewFrame(frame As Bitmap)
        If PSGViewForm Is Nothing Then Exit Sub
        If PSGViewForm.IsDisposed Then Exit Sub
        If Not PSGViewForm.Visible Then Exit Sub
        FastBitmapRenderer.RenderBitmapOnForm(PSGViewFormHandle, frame, 0, 0)
        frame.Dispose()
        'FastBitmapRenderer.RenderBitmapStretched(PSGViewFormHandle, frame, 0, 0, New Drawing.Size(512, 512))
    End Sub

    Private Sub CheckBox9_CheckedChanged(sender As Object, e As EventArgs) Handles PSGViewVolumeGraphBox.CheckedChanged
        If PSGViewer Is Nothing Then Exit Sub
        PSGViewer.ShowPCMGraph = PSGViewVolumeGraphBox.Checked
    End Sub

    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        If provider Is Nothing Then Exit Sub
        provider.Volume = TrackBar1.Value / 100
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        If provider Is Nothing Then Exit Sub
        If My.Settings.UseNewFilter Then
            Dim newsid = DirectCast(sid, ShitSID_fp)
            newsid.FilterFP.reset()
            newsid.FilterFP.setCurveAndDistortionDefaults()
            Dim got = newsid.FilterFP.getCurveProperties
            NumericUpDown2.Value = got(0)
            NumericUpDown3.Value = got(1)
            NumericUpDown4.Value = got(2)
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        My.Settings.Reload()
        If Not My.Settings.UseNewFilter Then
            Label6.Text = "Cutoff multiplier"
            NumericUpDown2.DecimalPlaces = 3
            NumericUpDown2.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
            NumericUpDown2.Maximum = New Decimal(New Integer() {32767, 0, 0, 0})
            NumericUpDown2.Minimum = 0
            NumericUpDown2.Value = New Decimal(New Integer() {1, 0, 0, 0})

            Label5.Text = "Resonance divider"
            NumericUpDown3.Increment = New Decimal(New Integer() {50, 0, 0, 0})
            NumericUpDown3.Value = 0
            NumericUpDown3.Maximum = New Decimal(New Integer() {20154, 0, 0, 0})
            NumericUpDown3.Minimum = New Decimal(New Integer() {20154, 0, 0, Integer.MinValue})

            Label4.Text = "Cutoff bias"
            NumericUpDown4.DecimalPlaces = 2
            NumericUpDown4.Increment = New Decimal(New Integer() {5, 0, 0, 131072})
            NumericUpDown4.Maximum = New Decimal(New Integer() {32768, 0, 0, 0})
            NumericUpDown4.Value = New Decimal(New Integer() {2, 0, 0, 0})

            CheckBox6.Checked = False
        End If
    End Sub

    Private Sub PSGViewDividerBox_ValueChanged(sender As Object, e As EventArgs) Handles PSGViewDividerBox.ValueChanged
        If provider Is Nothing Then Exit Sub
        provider.PSGViewDivider = PSGViewDividerBox.Value
    End Sub

    Private Sub PSGViewEnableBox_CheckedChanged(sender As Object, e As EventArgs) Handles PSGViewEnableBox.CheckedChanged
        If provider Is Nothing Then Exit Sub
        If PSGViewEnableBox.Checked Then
            PSGViewForm.Show()
            PSGViewFormHandle = PSGViewForm.Handle
            provider.EnablePSGView = PSGViewEnableBox.Checked
        Else
            provider.EnablePSGView = PSGViewEnableBox.Checked
            PSGViewForm.Hide()
        End If
    End Sub
End Class
