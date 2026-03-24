Imports NAudio.Wave
Imports Highbyte.DotNet6502
Imports System.IO
Imports NAudio.CoreAudioApi
Imports System.Runtime.InteropServices
Imports NAudio.MediaFoundation
Imports FFMediaToolkit.Decoding
Imports FFMediaToolkit.Encoding
Public Class Form1
    Public SAMPLERATE As Integer = 88200
    Public sid As ShitSID
    Public sidfile As SidFile
    Public fakeClockCount = 0
    Public cpu As New CPU
    Dim delayMS = 20 ' this does not work
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
        sid = New ShitSID(SAMPLERATE)
        cpu.SP = 1
        mem(1) = &H37
        SidFile = SidFile.Load(OpenFileDialog1.FileName)
        If SidFile.FlagBits(2) = False AndAlso SidFile.FlagBits(3) = True Then
            mem(&H2A6) = 1 ' PAL
        End If

        ' Set song number from UI or default
        If NumericUpDown1.Value > 0 Then
            cpu.A = NumericUpDown1.Value - 1
        Else
            cpu.A = SidFile.StartSong - 1
        End If
        sid.Voices(0).MuteVoice = CheckBox2.Checked
        sid.Voices(1).MuteVoice = CheckBox3.Checked
        sid.Voices(2).MuteVoice = CheckBox4.Checked
        sid.MuteSamples = CheckBox8.Checked
        For Each x As Voice In sid.Voices
            x.LoFiDuty = CheckBox1.Checked
        Next
        sid.Filter.Mode6581 = CheckBox6.Checked
        sid.InternalAudioFilter = AudioOutputSettings.CheckBox1.Checked
        sid.Filter.CutoffBias = NumericUpDown3.Value
        sid.Filter.CutoffMultiplier = NumericUpDown2.Value
        sid.Filter.ResonanceDivider = NumericUpDown4.Value
        sid.BypassFilter = CheckBox7.Checked
        sid.VolumeSampleMode = RadioButton1.Checked
        If RadioButton3.Checked Then sid.FilterCurve = ShitSID.FilterCurveType.Dark
        If RadioButton4.Checked Then sid.FilterCurve = ShitSID.FilterCurveType.Average
        If RadioButton5.Checked Then sid.FilterCurve = ShitSID.FilterCurveType.Bright
        CheckBox6_CheckedChanged(Nothing, Nothing)
        cpu.PC = SidFile.InitAddress
        Console.WriteLine($"Init address: {SidFile.InitAddress.ToString("X4")}")
        If CheckBox5.Checked Then
            NumericUpDown6.Value = 60
        End If
        PSGViewer = New PSGView(sid)
        provider = New SidAudioProvider(sid, cpu, mem, PSGViewer, SAMPLERATE)
        AddHandler provider.PSGViewFrame, AddressOf provider_PSGViewFrame
        provider.TickRate = NumericUpDown6.Value
        provider.UseNTSC = CheckBox5.Checked
        PSGViewForm.Show()
        PSGViewFormHandle = PSGViewForm.Handle
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

    Private Sub NumericUpDown3_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown3.ValueChanged
        If sid IsNot Nothing Then
            sid.Filter.CutoffBias = NumericUpDown3.Value
        End If

    End Sub

    Private Sub NumericUpDown4_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown4.ValueChanged
        If sid IsNot Nothing Then
            sid.Filter.ResonanceDivider = NumericUpDown4.Value
        End If
    End Sub

    Private Sub CheckBox6_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox6.CheckedChanged
        If sid IsNot Nothing Then
            If CheckBox6.Checked Then
                sid.Filter.Mode6581 = True
                sid.Filter.Reset()
            Else
                sid.Filter.Mode6581 = False
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

    Private Sub NumericUpDown2_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown2.ValueChanged
        If sid IsNot Nothing Then
            sid.Filter.CutoffMultiplier = NumericUpDown2.Value
        End If
    End Sub
    Dim stamp As New TimeSpan
    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        If Not OpenFileDialog1.ShowDialog = DialogResult.OK Then
            Exit Sub
        End If
        Console.WriteLine("Init MF...")
        MediaFoundationApi.Startup()
        Console.WriteLine("Loading SID...")
        LoadSID()
        RemoveHandler provider.PSGViewFrame, AddressOf provider_PSGViewFrame
        AddHandler provider.PSGViewFrame, AddressOf EncodeFrameHandler
        provider.sidfile = sidfile
        provider.InitSIDFile()
        Console.WriteLine("Creating MediaType...")
        Dim type As New MediaType(provider.WaveFormat)
        Console.WriteLine("Creating MF encoder...")
        Dim wavProv As New MediaFoundationEncoder(type)
        wavProv.DefaultReadBufferSize = SAMPLERATE \ NumericUpDown6.Value
        Console.WriteLine("Creating video container...")
        Dim opts = New VideoEncoderSettings(512, 512, NumericUpDown6.Value, VideoCodec.H264) With {.Bitrate = 3200 * 1000, .EncoderPreset = EncoderPreset.Faster}
        opts.CodecOptions("profile") = "high444"
        opts.CodecOptions("pix_fmt") = "yuv444p"
        opts.VideoFormat = FFMediaToolkit.Graphics.ImagePixelFormat.Yuv444
        EncodeVid = MediaBuilder.CreateContainer("B:\source\repos\ShitSID\ShitSID\bin\Debug\net9.0-windows\output.mp4", ContainerFormat.MP4).
            WithVideo(opts).Create
        Console.WriteLine("Encoding...")
        provider.runCPU = True
        wavProv.Encode("output.wav", provider.Take(TimeSpan.FromSeconds(NumericUpDown5.Value)).ToWaveProvider)
        EncodeVid.Dispose()
        Console.WriteLine("Done")
        RemoveHandler provider.PSGViewFrame, AddressOf EncodeFrameHandler
        AddHandler provider.PSGViewFrame, AddressOf provider_PSGViewFrame

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
        If sid IsNot Nothing Then
            If RadioButton3.Checked Then
                sid.FilterCurve = ShitSID.FilterCurveType.Dark
            End If
            If RadioButton4.Checked Then
                sid.FilterCurve = ShitSID.FilterCurveType.Average
            End If
            If RadioButton5.Checked Then
                sid.FilterCurve = ShitSID.FilterCurveType.Bright
            End If
        End If
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
        'FastBitmapRenderer.RenderBitmapStretched(PSGViewFormHandle, frame, 0, 0, New Drawing.Size(512, 512))
    End Sub
End Class
