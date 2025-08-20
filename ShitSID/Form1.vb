Imports NAudio.Wave
Imports Highbyte.DotNet6502
Imports System.IO
Imports NAudio.CoreAudioApi
Imports System.Runtime.InteropServices
Public Class Form1
    Public sid As ShitSID
    Public cpu As New CPU
    Dim delayMS = 20 ' this does not work
    Public mem As New Memory(65536)
    Private waveOut As WasapiOut = Nothing
    Private provider As SidAudioProvider = Nothing
    ' QueryPerformanceCounter (import from Windows API)
    <DllImport("kernel32.dll", CharSet:=CharSet.Auto)>
    Public Shared Function QueryPerformanceCounter(ByRef lpPerformanceCount As Long) As Boolean
    End Function

    ' QueryPerformanceFrequency (import from Windows API)
    <DllImport("kernel32.dll", CharSet:=CharSet.Auto)>
    Public Shared Function QueryPerformanceFrequency(ByRef lpFrequency As Long) As Boolean
    End Function
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        OpenFileDialog1.ShowDialog()
    End Sub
    Public sidfile As SidFile
    Public fakeClockCount = 0
    Private Sub LoadAndPlaySID()
        ' Stop and dispose existing audio if playing
        If waveOut IsNot Nothing Then
            waveOut.Stop()
            waveOut.Dispose()
            waveOut = Nothing
        End If

        ' Cancel background worker if it's running
        If BackgroundWorker1.IsBusy Then
            BackgroundWorker1.CancelAsync()
            While BackgroundWorker1.IsBusy
                Application.DoEvents()
            End While
        End If

        ' Reinitialize components
        sid = New ShitSID()
        cpu = New CPU()
        mem = New Memory()
        cpu.SP = 2
        mem(1) = &H37
        sidfile = SidFile.Load(OpenFileDialog1.FileName)
        If sidfile.FlagBits(2) = True AndAlso sidfile.FlagBits(3) = False Then
            delayMS = 16 ' NTSC
        End If
        If sidfile.FlagBits(2) = False AndAlso sidfile.FlagBits(3) = True Then
            mem(&H2A6) = 1 ' PAL
        End If
        If sidfile.FlagBits(4) = False AndAlso sidfile.FlagBits(5) = True Then
            ' 6581 mode
            CheckBox6.Checked = True
            sid.Filter.Mode6581 = True
        End If
        If sidfile.Speed Or 4294967167 = 1 Then
            delayMS = 16 ' CIA timer - ~60hz
            mem(&H2A6) = 0
        Else
            delayMS = 20 ' raster interrupt - 50hz
            mem(&H2A6) = 1
        End If
        ' Set song number from UI or default
        If NumericUpDown1.Value > 0 Then
            cpu.A = NumericUpDown1.Value - 1
        Else
            cpu.A = sidfile.StartSong - 1
        End If
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
        sid.Filter.CutoffMultiplier = NumericUpDown2.Value
        sid.Filter.CutoffBias = NumericUpDown3.Value
        sid.Filter.ResonanceDivider = NumericUpDown4.Value
        CheckBox6_CheckedChanged(Nothing, Nothing)
        cpu.PC = sidfile.InitAddress
        Console.WriteLine($"Init address: {sidfile.InitAddress.ToString("X")}")
        provider = New SidAudioProvider(sid)
        If CheckBox5.Checked Then
            provider.UseNTSC = True
        Else
            provider.UseNTSC = False
        End If
        waveOut = New WasapiOut(AudioClientShareMode.Shared, True, 4)
        ' Run init routine
        ' Start background audio pumping
        'If initTimer.Elapsed.TotalSeconds <= 10 Then
        '    initTimer.Stop()
        'BackgroundWorker1.RunWorkerAsync()
        'End If
        provider.sidfile = sidfile
        provider.InitSIDFile()
        waveOut.Init(provider)
        waveOut.Play()
        provider.runCPU = True
    End Sub
    Public Sub pr(str As String)
        Console.WriteLine(str)
    End Sub
    Private Sub OpenFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk
        ' if this dies im blaming chatgp
        LoadAndPlaySID()
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Dim watch As New Stopwatch
        While Not e.Cancel
            watch.Start()
            cpu.PC = sidfile.PlayAddress
            Dim cycCount = 0
            While True
                Dim state = cpu.ExecuteOneInstruction(mem)
                'Console.WriteLine($"{state.LastInstructionExecResult.OpCodeByte.ToString("X2")} {mem(state.PCBeforeLastOpCodeExecuted + 1).ToString("X2")} {mem(state.PCBeforeLastOpCodeExecuted + 2).ToString("X2")} {mem(state.PCBeforeLastOpCodeExecuted + 3).ToString("X2")} at {state.PCBeforeLastOpCodeExecuted.ToString()}")
                cycCount += state.CyclesConsumed
                If cycCount >= 19709 Or watch.ElapsedMilliseconds >= delayMS Then
                    Exit While
                End If
            End While
            While watch.ElapsedMilliseconds < delayMS
                If BackgroundWorker1.CancellationPending Then
                    e.Cancel = True
                    Exit Sub
                End If
            End While
            watch.Stop()
            watch.Reset()

        End While
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked Then
            For Each x As Voice In sid.Voices
                x.LoFiDuty = True
            Next
        Else
            For Each x As Voice In sid.Voices
                x.LoFiDuty = False
            Next
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

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox2.CheckedChanged, CheckBox4.CheckedChanged, CheckBox3.CheckedChanged
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
    End Sub

    Private Sub CheckBox5_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox5.CheckedChanged
        If CheckBox5.Checked Then
            provider.UseNTSC = True
        Else
            provider.UseNTSC = False
        End If
    End Sub

    Private Sub NumericUpDown2_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown2.ValueChanged
        If sid IsNot Nothing Then

            sid.Filter.CutoffMultiplier = NumericUpDown2.Value
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
                'sid.Filter.CutoffMultiplier = 2
                'sid.Filter.CutoffBias = 425
                'sid.Filter.ResonanceDivider = 4
                sid.Filter.CutoffMultiplier = 2
                sid.Filter.CutoffBias = 0
                sid.Filter.ResonanceDivider = 2
                sid.Filter.Reset()
            Else
                sid.Filter.Mode6581 = False
                sid.Filter.Reset()
            End If
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Using fs As New FileStream("ramdump.bin", FileMode.Create, FileAccess.Write)
            For i = 0 To 65535
                fs.WriteByte(mem(i))
            Next
        End Using
        'provider.sid.Filter.fs.Dispose()
    End Sub

    Private Sub CheckBox7_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox7.CheckedChanged
        If sid IsNot Nothing Then
            sid.bypassFilter = CheckBox7.Checked
        End If
    End Sub

    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        If sid IsNot Nothing Then
            sid.VolumeRegisterSampleMode = RadioButton1.Checked
        End If
    End Sub
End Class
