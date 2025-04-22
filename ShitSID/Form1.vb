Imports Highbyte.DotNet6502
Imports System.IO
Public Class Form1
    ' THIS IS EXPORT VERSION DO NOT USE FoR PLAYBACK ggsgsdgfgfdsdfdgfds
    Const PAL_FRAME_CYCLES As Integer = 63005
    Const SAMPLE_RATE As Integer = 44100
    Dim cyclesPerSample As Double = 985248.0 / SAMPLE_RATE
    Dim cycleCounter As Double = 0

    Dim frameCyclesRemaining As Integer = 0
    Dim sampleTimer As Double = 0.0
    Dim sampleRateFactor As Double = 985248.0 / SAMPLE_RATE ' PAL SID
    Public sid As New ShitSID()
    Dim cpu As New CPU
    Dim mem As New Memory(65536)
    Dim sidfile As SidFile
    Dim outfile As FileStream
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        OpenFileDialog1.ShowDialog()
    End Sub
    Private Sub LoadAndPlaySID()
        ' Cancel background worker if it's running
        If BackgroundWorker1.IsBusy Then
            BackgroundWorker1.CancelAsync()
            While BackgroundWorker1.IsBusy
                Application.DoEvents()
            End While
        End If

        ' Reinitialize components
        sid = New ShitSID()
        ' set SID settings
        If CheckBox1.Checked Then
            For Each x As Voice In sid.Voices
                x.LoFiDuty = True
            Next
        Else
            For Each x As Voice In sid.Voices
                x.LoFiDuty = False
            Next
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
        If CheckBox6.Checked Then
            sid.Filter.Mode6581 = True
        Else
            sid.Filter.Mode6581 = False
        End If
        cpu = New CPU()
        mem = New Memory()

        sidfile = SidFile.Load(OpenFileDialog1.FileName)

        Const SAMPLE_RATE As Integer = 44100
        Const CPU_FREQ As Double = 985248.0 ' PAL clock speed
        Const CYCLES_PER_FRAME As Integer = 19656 ' Number of cycles per frame (C64 frame)
        Dim cyclesPerSample As Double = CPU_FREQ / SAMPLE_RATE ' How many cycles per audio sample
        Dim watch As Stopwatch = Stopwatch.StartNew()

        ' Load SID data into memory
        For i = 0 To sidfile.Data.Length - 1
            mem(sidfile.LoadAddress + i) = sidfile.Data(i)
        Next

        ' Map SID registers for writing
        For i = &HD400 To &HD41F ' $D400-$D41F
            mem.MapWriter(i, Sub(addr As UShort, value As Byte)
                                 Debug.WriteLine($"{addr.ToString("X4")}, {value.ToString("X2")}")
                                 sid.WriteRegister(addr, value)
                             End Sub)
        Next

        ' Set initial song (either start song or from user input)
        If NumericUpDown1.Value > 0 Then
            cpu.A = NumericUpDown1.Value - 1
        Else
            cpu.A = sidfile.StartSong - 1
        End If
        cpu.PC = sidfile.InitAddress

        ' Run init routine to prepare the SID for playback
        While True
            Dim state = cpu.ExecuteOneInstruction(mem)
            If state.LastInstructionExecResult.OpCodeByte = &H60 Then Exit While
        End While
        ' Set play address and output path
        cpu.PC = sidfile.PlayAddress
        Dim path = "B:\sidOut.raw"

        Using outfile As New BinaryWriter(File.Create(path))
            Dim totalCycles As Double = 0

            ' Repeat until the specified time is reached
            While watch.Elapsed.TotalSeconds < 10 ' Export for 10 seconds
                Dim frameCycles As Integer = 0
                ' Run through one frame (19656 cycles per frame)
                While frameCycles < CYCLES_PER_FRAME
                    ' Execute a single instruction and get how many cycles it consumes
                    Dim state = cpu.ExecuteOneInstruction(mem)
                    Dim usedCycles = state.LastInstructionExecResult.CyclesConsumed

                    ' Clock the SID for the consumed cycles
                    For i = 0 To usedCycles - 1
                        sid.Clock()
                    Next

                    ' Increment frame cycles
                    frameCycles += usedCycles

                    ' Check for the end of the play routine and loop if necessary
                    If state.LastInstructionExecResult.OpCodeByte = &H60 Then
                        cpu.PC = sidfile.PlayAddress ' Loop play routine
                    End If
                End While

                ' Output a sample after the full frame of SID clocking
                Dim sample As Integer = sid.GetSample() * 127 ' Get SID sample (scaled)
                outfile.Write(Convert.ToByte(Math.Clamp(sample + 127, 0, 255))) ' Write the sample to the output file

                ' Adjust the total cycles to keep track of the current playback time
                totalCycles += CYCLES_PER_FRAME

                ' Sync playback with the audio sample rate
                If totalCycles >= cyclesPerSample Then
                    totalCycles -= cyclesPerSample
                End If
            End While

            ' Finalize the output file
            outfile.Flush()
            outfile.Dispose()
        End Using
        MsgBox("Export complete.")
    End Sub
    Private Sub OpenFileDialog1_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles OpenFileDialog1.FileOk
        ' if this dies im blaming chatgp
        LoadAndPlaySID()
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
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
        'If CheckBox5.Checked Then
        '    delayMS = 10
        'Else
        '    delayMS = 20
        'End If
    End Sub

    Private Sub NumericUpDown2_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown2.ValueChanged
        sid.Filter.CutoffMultiplier = NumericUpDown2.Value
    End Sub

    Private Sub NumericUpDown3_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown3.ValueChanged
        sid.Filter.CutoffBias = NumericUpDown3.Value
    End Sub

    Private Sub NumericUpDown4_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown4.ValueChanged
        sid.Filter.ResonanceDivider = NumericUpDown4.Value
    End Sub

    Private Sub CheckBox6_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox6.CheckedChanged
        If CheckBox6.Checked Then
            sid.Filter.Mode6581 = True
        Else
            sid.Filter.Mode6581 = False
        End If
    End Sub
End Class
