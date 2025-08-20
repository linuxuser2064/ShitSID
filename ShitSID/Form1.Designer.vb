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
        components = New ComponentModel.Container()
        Button1 = New Button()
        OpenFileDialog1 = New OpenFileDialog()
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
        CheckBox7 = New CheckBox()
        CheckBox6 = New CheckBox()
        Label5 = New Label()
        NumericUpDown4 = New NumericUpDown()
        Label4 = New Label()
        NumericUpDown3 = New NumericUpDown()
        ClockTimer = New Timer(components)
        CPUWorker = New ComponentModel.BackgroundWorker()
        GroupBox2 = New GroupBox()
        RadioButton2 = New RadioButton()
        RadioButton1 = New RadioButton()
        Button4 = New Button()
        ComboBox1 = New ComboBox()
        Label3 = New Label()
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).BeginInit()
        GroupBox1.SuspendLayout()
        CType(NumericUpDown4, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown3, ComponentModel.ISupportInitialize).BeginInit()
        GroupBox2.SuspendLayout()
        SuspendLayout()
        ' 
        ' Button1
        ' 
        Button1.Location = New Point(12, 12)
        Button1.Name = "Button1"
        Button1.Size = New Size(92, 23)
        Button1.TabIndex = 0
        Button1.Text = "Load SID file"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' OpenFileDialog1
        ' 
        OpenFileDialog1.FileName = "OpenFileDialog1"
        OpenFileDialog1.Filter = "SID files|*.SID|All files|*"
        ' 
        ' CheckBox1
        ' 
        CheckBox1.AutoSize = True
        CheckBox1.Location = New Point(109, 15)
        CheckBox1.Margin = New Padding(3, 2, 3, 2)
        CheckBox1.Name = "CheckBox1"
        CheckBox1.Size = New Size(142, 19)
        CheckBox1.TabIndex = 1
        CheckBox1.Text = "4-bit duty cycle mode"
        CheckBox1.UseVisualStyleBackColor = True
        ' 
        ' NumericUpDown1
        ' 
        NumericUpDown1.Location = New Point(109, 38)
        NumericUpDown1.Margin = New Padding(3, 2, 3, 2)
        NumericUpDown1.Name = "NumericUpDown1"
        NumericUpDown1.Size = New Size(131, 23)
        NumericUpDown1.TabIndex = 2
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(12, 38)
        Label1.Name = "Label1"
        Label1.Size = New Size(85, 15)
        Label1.TabIndex = 3
        Label1.Text = "Song number: "
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Font = New Font("Segoe UI", 7.8F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label2.Location = New Point(12, 56)
        Label2.Name = "Label2"
        Label2.Size = New Size(138, 13)
        Label2.TabIndex = 4
        Label2.Text = "(number 0 means default)"
        ' 
        ' Button2
        ' 
        Button2.Location = New Point(12, 71)
        Button2.Margin = New Padding(3, 2, 3, 2)
        Button2.Name = "Button2"
        Button2.Size = New Size(88, 22)
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
        Button3.Location = New Point(109, 71)
        Button3.Margin = New Padding(3, 2, 3, 2)
        Button3.Name = "Button3"
        Button3.Size = New Size(113, 22)
        Button3.TabIndex = 6
        Button3.Text = "Open statistics"
        Button3.UseVisualStyleBackColor = True
        ' 
        ' CheckBox2
        ' 
        CheckBox2.AutoSize = True
        CheckBox2.Location = New Point(275, 9)
        CheckBox2.Margin = New Padding(3, 2, 3, 2)
        CheckBox2.Name = "CheckBox2"
        CheckBox2.Size = New Size(108, 19)
        CheckBox2.TabIndex = 8
        CheckBox2.Text = "Mute channel 1"
        CheckBox2.UseVisualStyleBackColor = True
        ' 
        ' CheckBox3
        ' 
        CheckBox3.AutoSize = True
        CheckBox3.Location = New Point(275, 32)
        CheckBox3.Margin = New Padding(3, 2, 3, 2)
        CheckBox3.Name = "CheckBox3"
        CheckBox3.Size = New Size(108, 19)
        CheckBox3.TabIndex = 9
        CheckBox3.Text = "Mute channel 2"
        CheckBox3.UseVisualStyleBackColor = True
        ' 
        ' CheckBox4
        ' 
        CheckBox4.AutoSize = True
        CheckBox4.Location = New Point(275, 54)
        CheckBox4.Margin = New Padding(3, 2, 3, 2)
        CheckBox4.Name = "CheckBox4"
        CheckBox4.Size = New Size(108, 19)
        CheckBox4.TabIndex = 10
        CheckBox4.Text = "Mute channel 3"
        CheckBox4.UseVisualStyleBackColor = True
        ' 
        ' CheckBox5
        ' 
        CheckBox5.AutoSize = True
        CheckBox5.Location = New Point(275, 76)
        CheckBox5.Margin = New Padding(3, 2, 3, 2)
        CheckBox5.Name = "CheckBox5"
        CheckBox5.Size = New Size(101, 19)
        CheckBox5.TabIndex = 11
        CheckBox5.Text = "NTSC tick rate"
        CheckBox5.UseVisualStyleBackColor = True
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Controls.Add(CheckBox7)
        GroupBox1.Controls.Add(CheckBox6)
        GroupBox1.Controls.Add(Label5)
        GroupBox1.Controls.Add(NumericUpDown4)
        GroupBox1.Controls.Add(Label4)
        GroupBox1.Controls.Add(NumericUpDown3)
        GroupBox1.Location = New Point(12, 98)
        GroupBox1.Margin = New Padding(3, 2, 3, 2)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Padding = New Padding(3, 2, 3, 2)
        GroupBox1.Size = New Size(238, 97)
        GroupBox1.TabIndex = 12
        GroupBox1.TabStop = False
        GroupBox1.Text = "Filter controls"
        ' 
        ' CheckBox7
        ' 
        CheckBox7.AutoSize = True
        CheckBox7.Location = New Point(119, 71)
        CheckBox7.Name = "CheckBox7"
        CheckBox7.Size = New Size(91, 19)
        CheckBox7.TabIndex = 7
        CheckBox7.Text = "Disable filter"
        CheckBox7.UseVisualStyleBackColor = True
        ' 
        ' CheckBox6
        ' 
        CheckBox6.AutoSize = True
        CheckBox6.Location = New Point(2, 71)
        CheckBox6.Margin = New Padding(3, 2, 3, 2)
        CheckBox6.Name = "CheckBox6"
        CheckBox6.Size = New Size(111, 19)
        CheckBox6.TabIndex = 6
        CheckBox6.Text = "6581 filter mode"
        CheckBox6.UseVisualStyleBackColor = True
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.Location = New Point(5, 44)
        Label5.Name = "Label5"
        Label5.Size = New Size(103, 15)
        Label5.TabIndex = 5
        Label5.Text = "Resonance divider"
        ' 
        ' NumericUpDown4
        ' 
        NumericUpDown4.DecimalPlaces = 2
        NumericUpDown4.Increment = New Decimal(New Integer() {5, 0, 0, 131072})
        NumericUpDown4.Location = New Point(136, 43)
        NumericUpDown4.Margin = New Padding(3, 2, 3, 2)
        NumericUpDown4.Maximum = New Decimal(New Integer() {32768, 0, 0, 0})
        NumericUpDown4.Name = "NumericUpDown4"
        NumericUpDown4.Size = New Size(74, 23)
        NumericUpDown4.TabIndex = 4
        NumericUpDown4.Value = New Decimal(New Integer() {3, 0, 0, 0})
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.Location = New Point(5, 23)
        Label4.Name = "Label4"
        Label4.Size = New Size(65, 15)
        Label4.TabIndex = 3
        Label4.Text = "Cutoff bias"
        ' 
        ' NumericUpDown3
        ' 
        NumericUpDown3.Increment = New Decimal(New Integer() {50, 0, 0, 0})
        NumericUpDown3.Location = New Point(136, 21)
        NumericUpDown3.Margin = New Padding(3, 2, 3, 2)
        NumericUpDown3.Maximum = New Decimal(New Integer() {20154, 0, 0, 0})
        NumericUpDown3.Minimum = New Decimal(New Integer() {20154, 0, 0, Integer.MinValue})
        NumericUpDown3.Name = "NumericUpDown3"
        NumericUpDown3.Size = New Size(74, 23)
        NumericUpDown3.TabIndex = 2
        ' 
        ' ClockTimer
        ' 
        ClockTimer.Interval = 1
        ' 
        ' CPUWorker
        ' 
        CPUWorker.WorkerSupportsCancellation = True
        ' 
        ' GroupBox2
        ' 
        GroupBox2.Controls.Add(RadioButton2)
        GroupBox2.Controls.Add(RadioButton1)
        GroupBox2.Location = New Point(256, 131)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(144, 64)
        GroupBox2.TabIndex = 13
        GroupBox2.TabStop = False
        GroupBox2.Text = "Master volume register"
        ' 
        ' RadioButton2
        ' 
        RadioButton2.AutoSize = True
        RadioButton2.Location = New Point(6, 39)
        RadioButton2.Name = "RadioButton2"
        RadioButton2.Size = New Size(99, 19)
        RadioButton2.TabIndex = 1
        RadioButton2.Text = "Volume mode"
        RadioButton2.UseVisualStyleBackColor = True
        ' 
        ' RadioButton1
        ' 
        RadioButton1.AutoSize = True
        RadioButton1.Checked = True
        RadioButton1.Location = New Point(6, 17)
        RadioButton1.Name = "RadioButton1"
        RadioButton1.Size = New Size(98, 19)
        RadioButton1.TabIndex = 0
        RadioButton1.TabStop = True
        RadioButton1.Text = "Sample mode"
        RadioButton1.UseVisualStyleBackColor = True
        ' 
        ' Button4
        ' 
        Button4.Location = New Point(325, 201)
        Button4.Name = "Button4"
        Button4.Size = New Size(75, 23)
        Button4.TabIndex = 14
        Button4.Text = "Pause"
        Button4.UseVisualStyleBackColor = True
        ' 
        ' ComboBox1
        ' 
        ComboBox1.FormattingEnabled = True
        ComboBox1.Items.AddRange(New Object() {"8000", "11025", "16000", "22050", "32000", "44100", "48000", "64000", "65536", "88200", "96000", "176400", "192000"})
        ComboBox1.Location = New Point(198, 202)
        ComboBox1.Name = "ComboBox1"
        ComboBox1.Size = New Size(121, 23)
        ComboBox1.TabIndex = 15
        ComboBox1.Text = "88200"
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Location = New Point(120, 205)
        Label3.Name = "Label3"
        Label3.Size = New Size(72, 15)
        Label3.TabIndex = 16
        Label3.Text = "Sample rate:"
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(409, 233)
        Controls.Add(Label3)
        Controls.Add(ComboBox1)
        Controls.Add(Button4)
        Controls.Add(GroupBox2)
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
        MaximizeBox = False
        Name = "Form1"
        Text = "ShitSID"
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).EndInit()
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        CType(NumericUpDown4, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown3, ComponentModel.ISupportInitialize).EndInit()
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Button1 As Button
    Friend WithEvents OpenFileDialog1 As OpenFileDialog
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
    Friend WithEvents Label5 As Label
    Friend WithEvents NumericUpDown4 As NumericUpDown
    Friend WithEvents Label4 As Label
    Friend WithEvents NumericUpDown3 As NumericUpDown
    Friend WithEvents CheckBox6 As CheckBox
    Friend WithEvents ClockTimer As Timer
    Friend WithEvents CPUWorker As System.ComponentModel.BackgroundWorker
    Friend WithEvents CheckBox7 As CheckBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents RadioButton2 As RadioButton
    Friend WithEvents RadioButton1 As RadioButton
    Friend WithEvents Button4 As Button
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents Label3 As Label

End Class
