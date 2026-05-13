Imports Microsoft.SqlServer.Server

Public Class Form1
    Dim sid As New SidWriter
    Dim data As Byte() = {}
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If OpenFileDialog1.ShowDialog = DialogResult.OK Then
            Dim inputdata = IO.File.ReadAllBytes(OpenFileDialog1.FileName)
            Dim loadAddr = BitConverter.ToUInt16(inputdata, 0)
            TextBox1.Text = loadAddr.ToString("X4")
            ReDim data(inputdata.Length - 3)
            For i = 2 To inputdata.Length - 1
                data(i - 2) = inputdata(i)
            Next
            Button3.Enabled = True
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If OpenFileDialog2.ShowDialog = DialogResult.OK Then
            data = IO.File.ReadAllBytes(OpenFileDialog2.FileName)
            Button3.Enabled = True
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If SaveFileDialog1.ShowDialog = DialogResult.OK Then
            If RSIDButton.Checked Then sid.Format = SidFormat.RSID
            sid.LoadAddress = Convert.ToUInt16(TextBox1.Text, 16)
            sid.InitAddress = Convert.ToUInt16(TextBox2.Text, 16)
            sid.PlayAddress = Convert.ToUInt16(TextBox3.Text, 16)
            sid.Songs = NumericUpDown1.Value
            sid.StartSong = NumericUpDown2.Value

            sid.VideoStandard = SidVideoStandard.Unknown
            If CheckBox1.Checked Then sid.VideoStandard += SidVideoStandard.PAL
            If CheckBox2.Checked Then sid.VideoStandard += SidVideoStandard.NTSC

            sid.Model = SidModel.Unknown
            If CheckBox4.Checked Then sid.Model += SidModel.MOS6581
            If CheckBox3.Checked Then sid.Model += SidModel.MOS8580

            sid.Name = TextBox6.Text
            sid.Author = TextBox5.Text
            sid.Released = TextBox4.Text

            sid.Data = data

            IO.File.WriteAllBytes(SaveFileDialog1.FileName, sid.GetBytes)
        End If
    End Sub
End Class
