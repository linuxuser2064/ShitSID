Public Class PSGViewForm
    Private Sub PSGViewForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        e.Cancel = True
        Form1.PSGViewEnableBox.Checked = False
        Me.Hide()
    End Sub
End Class