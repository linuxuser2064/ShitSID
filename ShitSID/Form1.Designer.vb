<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Button1 = New Button()
        OpenFileDialog1 = New OpenFileDialog()
        BackgroundWorker1 = New ComponentModel.BackgroundWorker()
        CheckBox1 = New CheckBox()
        NumericUpDown1 = New NumericUpDown()
        Label1 = New Label()
        Label2 = New Label()
        Button2 = New Button()
        OpenFileDialog2 = New OpenFileDialog()
        Button3 = New Button()
        CheckBox2 = New CheckBox()
        CheckBox3 = New CheckBox()
        CheckBox4 = New CheckBox()
        CheckBox5 = New CheckBox()
        GroupBox1 = New GroupBox()
        Label5 = New Label()
        NumericUpDown4 = New NumericUpDown()
        Label4 = New Label()
        NumericUpDown3 = New NumericUpDown()
        Label3 = New Label()
        NumericUpDown2 = New NumericUpDown()
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).BeginInit()
        GroupBox1.SuspendLayout()
        CType(NumericUpDown4, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown3, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown2, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(14, 16)
        Button1.Margin = New Padding(3, 4, 3, 4)
        Button1.Name = "Button1"
        Button1.Size = New Size(105, 31)
        Button1.TabIndex = 0
        Button1.Text = "Load SID file"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' OpenFileDialog1
        ' 
        OpenFileDialog1.FileName = "OpenFileDialog1"
        OpenFileDialog1.Filter = "SID files|*.SID|All files|*"
        ' 
        ' BackgroundWorker1
        ' 
        ' 
        ' CheckBox1
        ' 
        CheckBox1.AutoSize = True
        CheckBox1.Location = New Point(125, 20)
        CheckBox1.Name = "CheckBox1"
        CheckBox1.Size = New Size(176, 24)
        CheckBox1.TabIndex = 1
        CheckBox1.Text = "4-bit duty cycle mode"
        CheckBox1.UseVisualStyleBackColor = True
        ' 
        ' NumericUpDown1
        ' 
        NumericUpDown1.Location = New Point(125, 51)
        NumericUpDown1.Name = "NumericUpDown1"
        NumericUpDown1.Size = New Size(150, 27)
        NumericUpDown1.TabIndex = 2
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(14, 51)
        Label1.Name = "Label1"
        Label1.Size = New Size(105, 20)
        Label1.TabIndex = 3
        Label1.Text = "Song number: "
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Font = New Font("Segoe UI", 7.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label2.Location = New Point(14, 75)
        Label2.Name = "Label2"
        Label2.Size = New Size(158, 17)
        Label2.TabIndex = 4
        Label2.Text = "(number 0 means default)"
        ' 
        ' Button2
        ' 
        Button2.Location = New Point(14, 95)
        Button2.Name = "Button2"
        Button2.Size = New Size(101, 29)
        Button2.TabIndex = 5
        Button2.Text = "View info"
        Button2.UseVisualStyleBackColor = True
        ' 
        ' OpenFileDialog2
        ' 
        OpenFileDialog2.FileName = "OpenFileDialog1"
        OpenFileDialog2.Filter = "SID files|*.SID|All files|*"
        ' 
        ' Button3
        ' 
        Button3.Location = New Point(125, 95)
        Button3.Name = "Button3"
        Button3.Size = New Size(129, 29)
        Button3.TabIndex = 6
        Button3.Text = "Open statistics"
        Button3.UseVisualStyleBackColor = True
        ' 
        ' CheckBox2
        ' 
        CheckBox2.AutoSize = True
        CheckBox2.Location = New Point(314, 12)
        CheckBox2.Name = "CheckBox2"
        CheckBox2.Size = New Size(132, 24)
        CheckBox2.TabIndex = 8
        CheckBox2.Text = "Mute channel 1"
        CheckBox2.UseVisualStyleBackColor = True
        ' 
        ' CheckBox3
        ' 
        CheckBox3.AutoSize = True
        CheckBox3.Location = New Point(314, 42)
        CheckBox3.Name = "CheckBox3"
        CheckBox3.Size = New Size(132, 24)
        CheckBox3.TabIndex = 9
        CheckBox3.Text = "Mute channel 2"
        CheckBox3.UseVisualStyleBackColor = True
        ' 
        ' CheckBox4
        ' 
        CheckBox4.AutoSize = True
        CheckBox4.Location = New Point(314, 72)
        CheckBox4.Name = "CheckBox4"
        CheckBox4.Size = New Size(132, 24)
        CheckBox4.TabIndex = 10
        CheckBox4.Text = "Mute channel 3"
        CheckBox4.UseVisualStyleBackColor = True
        ' 
        ' CheckBox5
        ' 
        CheckBox5.AutoSize = True
        CheckBox5.Location = New Point(314, 102)
        CheckBox5.Name = "CheckBox5"
        CheckBox5.Size = New Size(124, 24)
        CheckBox5.TabIndex = 11
        CheckBox5.Text = "Double speed"
        CheckBox5.UseVisualStyleBackColor = True
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Controls.Add(Label5)
        GroupBox1.Controls.Add(NumericUpDown4)
        GroupBox1.Controls.Add(Label4)
        GroupBox1.Controls.Add(NumericUpDown3)
        GroupBox1.Controls.Add(Label3)
        GroupBox1.Controls.Add(NumericUpDown2)
        GroupBox1.Location = New Point(14, 130)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(272, 155)
        GroupBox1.TabIndex = 12
        GroupBox1.TabStop = False
        GroupBox1.Text = "Filter controls"
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(6, 94)
        Label5.Name = "Label5"
        Label5.Size = New Size(130, 20)
        Label5.TabIndex = 5
        Label5.Text = "Resonance divider"
        ' 
        ' NumericUpDown4
        ' 
        NumericUpDown4.DecimalPlaces = 2
        NumericUpDown4.Increment = New Decimal(New Integer() {5, 0, 0, 131072})
        NumericUpDown4.Location = New Point(155, 92)
        NumericUpDown4.Maximum = New Decimal(New Integer() {32768, 0, 0, 0})
        NumericUpDown4.Name = "NumericUpDown4"
        NumericUpDown4.Size = New Size(85, 27)
        NumericUpDown4.TabIndex = 4
        NumericUpDown4.Value = New Decimal(New Integer() {3, 0, 0, 0})
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(6, 61)
        Label4.Name = "Label4"
        Label4.Size = New Size(81, 20)
        Label4.TabIndex = 3
        Label4.Text = "Cutoff bias"
        ' 
        ' NumericUpDown3
        ' 
        NumericUpDown3.Increment = New Decimal(New Integer() {50, 0, 0, 0})
        NumericUpDown3.Location = New Point(155, 59)
        NumericUpDown3.Maximum = New Decimal(New Integer() {20154, 0, 0, 0})
        NumericUpDown3.Name = "NumericUpDown3"
        NumericUpDown3.Size = New Size(85, 27)
        NumericUpDown3.TabIndex = 2
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(6, 28)
        Label3.Name = "Label3"
        Label3.Size = New Size(118, 20)
        Label3.TabIndex = 1
        Label3.Text = "Cutoff multiplier"
        ' 
        ' NumericUpDown2
        ' 
        NumericUpDown2.DecimalPlaces = 2
        NumericUpDown2.Increment = New Decimal(New Integer() {5, 0, 0, 131072})
        NumericUpDown2.Location = New Point(155, 26)
        NumericUpDown2.Maximum = New Decimal(New Integer() {2, 0, 0, 0})
        NumericUpDown2.Name = "NumericUpDown2"
        NumericUpDown2.Size = New Size(85, 27)
        NumericUpDown2.TabIndex = 0
        NumericUpDown2.Value = New Decimal(New Integer() {167, 0, 0, 131072})
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(557, 297)
        Controls.Add(GroupBox1)
        Controls.Add(CheckBox5)
        Controls.Add(CheckBox4)
        Controls.Add(CheckBox3)
        Controls.Add(CheckBox2)
        Controls.Add(Button3)
        Controls.Add(Button2)
        Controls.Add(NumericUpDown1)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Controls.Add(CheckBox1)
        Controls.Add(Button1)
        FormBorderStyle = FormBorderStyle.Fixed3D
        Margin = New Padding(3, 4, 3, 4)
        MaximizeBox = False
        Name = "Form1"
        Text = "ShitSID"
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).EndInit()
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        CType(NumericUpDown4, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown3, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown2, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Button1 As Button
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
    Friend WithEvents BackgroundWorker1 As System.ComponentModel.BackgroundWorker
    Friend WithEvents CheckBox1 As CheckBox
    Friend WithEvents NumericUpDown1 As NumericUpDown
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Button2 As Button
    Friend WithEvents OpenFileDialog2 As OpenFileDialog
    Friend WithEvents Button3 As Button
    Friend WithEvents CheckBox2 As CheckBox
    Friend WithEvents CheckBox3 As CheckBox
    Friend WithEvents CheckBox4 As CheckBox
    Friend WithEvents CheckBox5 As CheckBox
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents NumericUpDown2 As NumericUpDown
    Friend WithEvents Label3 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents NumericUpDown4 As NumericUpDown
    Friend WithEvents Label4 As Label
    Friend WithEvents NumericUpDown3 As NumericUpDown

End Class
