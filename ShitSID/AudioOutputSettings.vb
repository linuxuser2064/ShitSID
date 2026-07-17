Public Class AudioOutputSettings
    Public HiddenControlNames As String()
    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Form1.SAMPLERATE = CInt(ComboBox1.Text)
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If Form1.sid IsNot Nothing Then
            Form1.sid.InternalAudioFilter = CheckBox1.Checked
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs)
        Dim loc = Form1.Location
        loc.Offset(2, 2)
        Form1.FormBorderStyle = FormBorderStyle.Sizable
        Form1.Location = loc
    End Sub

    Private Sub AudioOutputSettings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        My.Settings.Reload()
        CheckBox2.Checked = My.Settings.UseNewFilter
        AddHandler CheckBox2.CheckedChanged, AddressOf CheckBox2_CheckedChanged
    End Sub

    Private Sub CheckBox2_CheckedChanged(sender As Object, e As EventArgs)
        My.Settings.UseNewFilter = CheckBox2.Checked
        My.Settings.Save()
    End Sub
End Class