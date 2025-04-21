Imports NAudio.Wave
Imports Highbyte.DotNet6502
Imports System.IO
Imports NAudio.CoreAudioApi
Public Class Form1
    Public sid As New ShitSID()
    Dim cpu As New CPU
    Dim mem As New Memory(65536)
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        OpenFileDialog1.ShowDialog()
    End Sub
    Dim sidfile As SidFile
    Dim provider As SidAudioProvider
    Private Sub OpenFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk
        If sidfile IsNot Nothing Then
            ' this means we're already playing
            Application.Restart()
        End If
        sidfile = SidFile.Load(OpenFileDialog1.FileName)
        If NumericUpDown1.Value > 0 Then
            cpu.A = NumericUpDown1.Value - 1
        Else
            cpu.A = sidfile.StartSong - 1
        End If
        cpu.PC = sidfile.InitAddress
        provider = New SidAudioProvider(sid)
        Dim WaveOut As New WasapiOut(AudioClientShareMode.Shared, True, 8)
        WaveOut.Init(provider)
        WaveOut.Play()

        For i = 0 To sidfile.Data.Length - 1
            mem(sidfile.LoadAddress + i) = sidfile.Data(i)
        Next
        While True
            Dim state = cpu.ExecuteOneInstruction(mem)
            If state.LastInstructionExecResult.OpCodeByte = &H60 Then
                Exit While
            End If
            If state.LastInstructionExecResult.OpCodeByte = &H0 Then
                MsgBox("BRK encountered")
                Exit While
            End If
        End While
        BackgroundWorker1.RunWorkerAsync()
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Dim watch As New Stopwatch
        While True
            watch.Start()
            cpu.PC = sidfile.PlayAddress
            While True
                Dim state = cpu.ExecuteOneInstruction(mem)
                If state.LastInstructionExecResult.OpCodeByte = &H60 Then
                    Exit While
                End If
            End While
            For i = 54272 To 54303
                sid.WriteRegister(i, mem(i))
            Next
            While watch.ElapsedMilliseconds < 20
                Threading.Thread.Sleep(TimeSpan.Zero)
            End While
            watch.Stop()
            watch.Reset()
        End While
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked Then
            For Each x As Voice In provider.sid.Voices
                x.LoFiDuty = True
            Next
        Else
            For Each x As Voice In provider.sid.Voices
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
            provider.sid.Voices(0).MuteVoice = True
        Else
            provider.sid.Voices(0).MuteVoice = False
        End If
        If CheckBox3.Checked Then
            provider.sid.Voices(1).MuteVoice = True
        Else
            provider.sid.Voices(1).MuteVoice = False
        End If
        If CheckBox4.Checked Then
            provider.sid.Voices(2).MuteVoice = True
        Else
            provider.sid.Voices(2).MuteVoice = False
        End If
    End Sub
End Class
