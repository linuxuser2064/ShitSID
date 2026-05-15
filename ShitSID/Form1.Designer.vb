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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
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
        Button7 = New Button()
        RadioButton5 = New RadioButton()
        RadioButton4 = New RadioButton()
        RadioButton3 = New RadioButton()
        Label6 = New Label()
        NumericUpDown2 = New NumericUpDown()
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
        Button5 = New Button()
        GroupBox3 = New GroupBox()
        NumericUpDown5 = New NumericUpDown()
        Label3 = New Label()
        Button6 = New Button()
        CheckBox8 = New CheckBox()
        GroupBox4 = New GroupBox()
        NumericUpDown6 = New NumericUpDown()
        Label7 = New Label()
        Label8 = New Label()
        SaveFileDialog1 = New SaveFileDialog()
        CheckBox9 = New CheckBox()
        GroupBox5 = New GroupBox()
        TrackBar1 = New TrackBar()
        Label9 = New Label()
        CheckBox10 = New CheckBox()
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).BeginInit()
        GroupBox1.SuspendLayout()
        CType(NumericUpDown2, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown4, ComponentModel.ISupportInitialize).BeginInit()
        CType(NumericUpDown3, ComponentModel.ISupportInitialize).BeginInit()
        GroupBox2.SuspendLayout()
        GroupBox3.SuspendLayout()
        CType(NumericUpDown5, ComponentModel.ISupportInitialize).BeginInit()
        GroupBox4.SuspendLayout()
        CType(NumericUpDown6, ComponentModel.ISupportInitialize).BeginInit()
        GroupBox5.SuspendLayout()
        CType(TrackBar1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' Button1
        ' 
        Button1.FlatStyle = FlatStyle.System
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
        CheckBox1.FlatStyle = FlatStyle.System
        CheckBox1.Location = New Point(109, 15)
        CheckBox1.Margin = New Padding(3, 2, 3, 2)
        CheckBox1.Name = "CheckBox1"
        CheckBox1.Size = New Size(148, 20)
        CheckBox1.TabIndex = 1
        CheckBox1.Text = "4-bit duty cycle mode"
        CheckBox1.UseVisualStyleBackColor = True
        ' 
        ' NumericUpDown1
        ' 
        NumericUpDown1.Location = New Point(109, 35)
        NumericUpDown1.Margin = New Padding(3, 2, 3, 2)
        NumericUpDown1.Name = "NumericUpDown1"
        NumericUpDown1.Size = New Size(131, 23)
        NumericUpDown1.TabIndex = 2
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.FlatStyle = FlatStyle.System
        Label1.Location = New Point(16, 38)
        Label1.Name = "Label1"
        Label1.Size = New Size(85, 15)
        Label1.TabIndex = 3
        Label1.Text = "Song number: "
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.FlatStyle = FlatStyle.System
        Label2.Font = New Font("Segoe UI", 7.8F)
        Label2.Location = New Point(16, 56)
        Label2.Name = "Label2"
        Label2.Size = New Size(138, 13)
        Label2.TabIndex = 4
        Label2.Text = "(number 0 means default)"
        ' 
        ' Button2
        ' 
        Button2.FlatStyle = FlatStyle.System
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
        Button3.Enabled = False
        Button3.FlatStyle = FlatStyle.System
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
        CheckBox2.FlatStyle = FlatStyle.System
        CheckBox2.Location = New Point(6, 18)
        CheckBox2.Margin = New Padding(3, 2, 3, 2)
        CheckBox2.Name = "CheckBox2"
        CheckBox2.Size = New Size(114, 20)
        CheckBox2.TabIndex = 8
        CheckBox2.Text = "Mute channel 1"
        CheckBox2.UseVisualStyleBackColor = True
        ' 
        ' CheckBox3
        ' 
        CheckBox3.AutoSize = True
        CheckBox3.FlatStyle = FlatStyle.System
        CheckBox3.Location = New Point(6, 41)
        CheckBox3.Margin = New Padding(3, 2, 3, 2)
        CheckBox3.Name = "CheckBox3"
        CheckBox3.Size = New Size(114, 20)
        CheckBox3.TabIndex = 9
        CheckBox3.Text = "Mute channel 2"
        CheckBox3.UseVisualStyleBackColor = True
        ' 
        ' CheckBox4
        ' 
        CheckBox4.AutoSize = True
        CheckBox4.FlatStyle = FlatStyle.System
        CheckBox4.Location = New Point(6, 63)
        CheckBox4.Margin = New Padding(3, 2, 3, 2)
        CheckBox4.Name = "CheckBox4"
        CheckBox4.Size = New Size(114, 20)
        CheckBox4.TabIndex = 10
        CheckBox4.Text = "Mute channel 3"
        CheckBox4.UseVisualStyleBackColor = True
        ' 
        ' CheckBox5
        ' 
        CheckBox5.AutoSize = True
        CheckBox5.FlatStyle = FlatStyle.System
        CheckBox5.Location = New Point(157, 213)
        CheckBox5.Margin = New Padding(3, 2, 3, 2)
        CheckBox5.Name = "CheckBox5"
        CheckBox5.Size = New Size(96, 20)
        CheckBox5.TabIndex = 11
        CheckBox5.Text = "NTSC mode"
        CheckBox5.UseVisualStyleBackColor = True
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Controls.Add(Button7)
        GroupBox1.Controls.Add(RadioButton5)
        GroupBox1.Controls.Add(RadioButton4)
        GroupBox1.Controls.Add(RadioButton3)
        GroupBox1.Controls.Add(Label6)
        GroupBox1.Controls.Add(NumericUpDown2)
        GroupBox1.Controls.Add(CheckBox7)
        GroupBox1.Controls.Add(CheckBox6)
        GroupBox1.Controls.Add(Label5)
        GroupBox1.Controls.Add(NumericUpDown4)
        GroupBox1.Controls.Add(Label4)
        GroupBox1.Controls.Add(NumericUpDown3)
        GroupBox1.FlatStyle = FlatStyle.System
        GroupBox1.Location = New Point(277, 9)
        GroupBox1.Margin = New Padding(3, 2, 3, 2)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Padding = New Padding(3, 2, 3, 2)
        GroupBox1.Size = New Size(238, 177)
        GroupBox1.TabIndex = 12
        GroupBox1.TabStop = False
        GroupBox1.Text = "Filter controls"
        ' 
        ' Button7
        ' 
        Button7.FlatStyle = FlatStyle.System
        Button7.Location = New Point(156, 148)
        Button7.Name = "Button7"
        Button7.Size = New Size(75, 23)
        Button7.TabIndex = 13
        Button7.Text = "Reset filter"
        Button7.UseVisualStyleBackColor = True
        ' 
        ' RadioButton5
        ' 
        RadioButton5.AutoSize = True
        RadioButton5.Enabled = False
        RadioButton5.FlatStyle = FlatStyle.System
        RadioButton5.Location = New Point(5, 152)
        RadioButton5.Name = "RadioButton5"
        RadioButton5.Size = New Size(122, 20)
        RadioButton5.TabIndex = 12
        RadioButton5.Text = "Bright filter curve"
        RadioButton5.UseVisualStyleBackColor = True
        ' 
        ' RadioButton4
        ' 
        RadioButton4.AutoSize = True
        RadioButton4.Checked = True
        RadioButton4.Enabled = False
        RadioButton4.FlatStyle = FlatStyle.System
        RadioButton4.Location = New Point(5, 133)
        RadioButton4.Name = "RadioButton4"
        RadioButton4.Size = New Size(133, 20)
        RadioButton4.TabIndex = 11
        RadioButton4.TabStop = True
        RadioButton4.Text = "Average filter curve"
        RadioButton4.UseVisualStyleBackColor = True
        ' 
        ' RadioButton3
        ' 
        RadioButton3.AutoSize = True
        RadioButton3.Enabled = False
        RadioButton3.FlatStyle = FlatStyle.System
        RadioButton3.Location = New Point(5, 114)
        RadioButton3.Name = "RadioButton3"
        RadioButton3.Size = New Size(114, 20)
        RadioButton3.TabIndex = 10
        RadioButton3.Text = "Dark filter curve"
        RadioButton3.UseVisualStyleBackColor = True
        ' 
        ' Label6
        ' 
        Label6.AutoSize = True
        Label6.FlatStyle = FlatStyle.System
        Label6.Location = New Point(9, 20)
        Label6.Name = "Label6"
        Label6.Size = New Size(95, 15)
        Label6.TabIndex = 9
        Label6.Text = "Cutoff multiplier"
        ' 
        ' NumericUpDown2
        ' 
        NumericUpDown2.DecimalPlaces = 3
        NumericUpDown2.Increment = New Decimal(New Integer() {1, 0, 0, 65536})
        NumericUpDown2.Location = New Point(136, 18)
        NumericUpDown2.Margin = New Padding(3, 2, 3, 2)
        NumericUpDown2.Maximum = New Decimal(New Integer() {32767, 0, 0, 0})
        NumericUpDown2.Name = "NumericUpDown2"
        NumericUpDown2.Size = New Size(74, 23)
        NumericUpDown2.TabIndex = 8
        NumericUpDown2.Value = New Decimal(New Integer() {1, 0, 0, 0})
        ' 
        ' CheckBox7
        ' 
        CheckBox7.AutoSize = True
        CheckBox7.FlatStyle = FlatStyle.System
        CheckBox7.Location = New Point(122, 90)
        CheckBox7.Name = "CheckBox7"
        CheckBox7.Size = New Size(97, 20)
        CheckBox7.TabIndex = 7
        CheckBox7.Text = "Disable filter"
        CheckBox7.UseVisualStyleBackColor = True
        ' 
        ' CheckBox6
        ' 
        CheckBox6.AutoSize = True
        CheckBox6.Checked = True
        CheckBox6.CheckState = CheckState.Checked
        CheckBox6.FlatStyle = FlatStyle.System
        CheckBox6.Location = New Point(5, 90)
        CheckBox6.Margin = New Padding(3, 2, 3, 2)
        CheckBox6.Name = "CheckBox6"
        CheckBox6.Size = New Size(117, 20)
        CheckBox6.TabIndex = 6
        CheckBox6.Text = "6581 filter mode"
        CheckBox6.UseVisualStyleBackColor = True
        ' 
        ' Label5
        ' 
        Label5.AutoSize = True
        Label5.FlatStyle = FlatStyle.System
        Label5.Location = New Point(9, 63)
        Label5.Name = "Label5"
        Label5.Size = New Size(103, 15)
        Label5.TabIndex = 5
        Label5.Text = "Resonance divider"
        ' 
        ' NumericUpDown4
        ' 
        NumericUpDown4.DecimalPlaces = 2
        NumericUpDown4.Increment = New Decimal(New Integer() {5, 0, 0, 131072})
        NumericUpDown4.Location = New Point(136, 62)
        NumericUpDown4.Margin = New Padding(3, 2, 3, 2)
        NumericUpDown4.Maximum = New Decimal(New Integer() {32768, 0, 0, 0})
        NumericUpDown4.Name = "NumericUpDown4"
        NumericUpDown4.Size = New Size(74, 23)
        NumericUpDown4.TabIndex = 4
        NumericUpDown4.Value = New Decimal(New Integer() {2, 0, 0, 0})
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.FlatStyle = FlatStyle.System
        Label4.Location = New Point(9, 42)
        Label4.Name = "Label4"
        Label4.Size = New Size(65, 15)
        Label4.TabIndex = 3
        Label4.Text = "Cutoff bias"
        ' 
        ' NumericUpDown3
        ' 
        NumericUpDown3.Increment = New Decimal(New Integer() {50, 0, 0, 0})
        NumericUpDown3.Location = New Point(136, 40)
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
        GroupBox2.FlatStyle = FlatStyle.System
        GroupBox2.Location = New Point(159, 98)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(112, 64)
        GroupBox2.TabIndex = 13
        GroupBox2.TabStop = False
        GroupBox2.Text = "Master volume"
        ' 
        ' RadioButton2
        ' 
        RadioButton2.AutoSize = True
        RadioButton2.FlatStyle = FlatStyle.System
        RadioButton2.Location = New Point(6, 39)
        RadioButton2.Name = "RadioButton2"
        RadioButton2.Size = New Size(105, 20)
        RadioButton2.TabIndex = 1
        RadioButton2.Text = "Volume mode"
        RadioButton2.UseVisualStyleBackColor = True
        ' 
        ' RadioButton1
        ' 
        RadioButton1.AutoSize = True
        RadioButton1.Checked = True
        RadioButton1.FlatStyle = FlatStyle.System
        RadioButton1.Location = New Point(6, 17)
        RadioButton1.Name = "RadioButton1"
        RadioButton1.Size = New Size(104, 20)
        RadioButton1.TabIndex = 0
        RadioButton1.TabStop = True
        RadioButton1.Text = "Sample mode"
        RadioButton1.UseVisualStyleBackColor = True
        ' 
        ' Button4
        ' 
        Button4.Enabled = False
        Button4.FlatStyle = FlatStyle.System
        Button4.Location = New Point(370, 248)
        Button4.Name = "Button4"
        Button4.Size = New Size(51, 23)
        Button4.TabIndex = 14
        Button4.Text = "Pause"
        Button4.UseVisualStyleBackColor = True
        ' 
        ' Button5
        ' 
        Button5.FlatStyle = FlatStyle.System
        Button5.Location = New Point(424, 248)
        Button5.Name = "Button5"
        Button5.Size = New Size(91, 23)
        Button5.TabIndex = 17
        Button5.Text = "Audio settings"
        Button5.UseVisualStyleBackColor = True
        ' 
        ' GroupBox3
        ' 
        GroupBox3.Controls.Add(NumericUpDown5)
        GroupBox3.Controls.Add(Label3)
        GroupBox3.Controls.Add(Button6)
        GroupBox3.FlatStyle = FlatStyle.System
        GroupBox3.Location = New Point(277, 188)
        GroupBox3.Name = "GroupBox3"
        GroupBox3.Size = New Size(238, 54)
        GroupBox3.TabIndex = 18
        GroupBox3.TabStop = False
        GroupBox3.Text = "Export controls"
        ' 
        ' NumericUpDown5
        ' 
        NumericUpDown5.DecimalPlaces = 1
        NumericUpDown5.Location = New Point(162, 24)
        NumericUpDown5.Maximum = New Decimal(New Integer() {32768, 0, 0, 0})
        NumericUpDown5.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        NumericUpDown5.Name = "NumericUpDown5"
        NumericUpDown5.Size = New Size(70, 23)
        NumericUpDown5.TabIndex = 1
        NumericUpDown5.Value = New Decimal(New Integer() {60, 0, 0, 0})
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.FlatStyle = FlatStyle.System
        Label3.Location = New Point(86, 26)
        Label3.Name = "Label3"
        Label3.Size = New Size(77, 15)
        Label3.TabIndex = 2
        Label3.Text = "Export length"
        ' 
        ' Button6
        ' 
        Button6.FlatStyle = FlatStyle.System
        Button6.Location = New Point(6, 22)
        Button6.Name = "Button6"
        Button6.Size = New Size(75, 23)
        Button6.TabIndex = 0
        Button6.Text = "Run export"
        Button6.UseVisualStyleBackColor = True
        ' 
        ' CheckBox8
        ' 
        CheckBox8.AutoSize = True
        CheckBox8.FlatStyle = FlatStyle.System
        CheckBox8.Location = New Point(6, 86)
        CheckBox8.Margin = New Padding(3, 2, 3, 2)
        CheckBox8.Name = "CheckBox8"
        CheckBox8.Size = New Size(106, 20)
        CheckBox8.TabIndex = 19
        CheckBox8.Text = "Mute samples"
        CheckBox8.UseVisualStyleBackColor = True
        ' 
        ' GroupBox4
        ' 
        GroupBox4.Controls.Add(CheckBox3)
        GroupBox4.Controls.Add(CheckBox8)
        GroupBox4.Controls.Add(CheckBox2)
        GroupBox4.Controls.Add(CheckBox4)
        GroupBox4.FlatStyle = FlatStyle.System
        GroupBox4.Location = New Point(6, 98)
        GroupBox4.Name = "GroupBox4"
        GroupBox4.Size = New Size(144, 111)
        GroupBox4.TabIndex = 20
        GroupBox4.TabStop = False
        GroupBox4.Text = "Channels"
        ' 
        ' NumericUpDown6
        ' 
        NumericUpDown6.Location = New Point(157, 183)
        NumericUpDown6.Maximum = New Decimal(New Integer() {960, 0, 0, 0})
        NumericUpDown6.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        NumericUpDown6.Name = "NumericUpDown6"
        NumericUpDown6.Size = New Size(94, 23)
        NumericUpDown6.TabIndex = 13
        NumericUpDown6.Value = New Decimal(New Integer() {50, 0, 0, 0})
        ' 
        ' Label7
        ' 
        Label7.AutoSize = True
        Label7.FlatStyle = FlatStyle.System
        Label7.Location = New Point(160, 165)
        Label7.Name = "Label7"
        Label7.Size = New Size(55, 15)
        Label7.TabIndex = 21
        Label7.Text = "Tick rate:"
        ' 
        ' Label8
        ' 
        Label8.AutoSize = True
        Label8.Location = New Point(253, 187)
        Label8.Name = "Label8"
        Label8.Size = New Size(21, 15)
        Label8.TabIndex = 22
        Label8.Text = "Hz"
        ' 
        ' SaveFileDialog1
        ' 
        SaveFileDialog1.Filter = "Wave files|*.wav|All files|*"
        ' 
        ' CheckBox9
        ' 
        CheckBox9.AutoSize = True
        CheckBox9.FlatStyle = FlatStyle.System
        CheckBox9.Location = New Point(6, 18)
        CheckBox9.Name = "CheckBox9"
        CheckBox9.Size = New Size(106, 20)
        CheckBox9.TabIndex = 13
        CheckBox9.Text = "Volume graph"
        CheckBox9.UseVisualStyleBackColor = True
        ' 
        ' GroupBox5
        ' 
        GroupBox5.Controls.Add(CheckBox9)
        GroupBox5.FlatStyle = FlatStyle.System
        GroupBox5.Location = New Point(6, 212)
        GroupBox5.Name = "GroupBox5"
        GroupBox5.Size = New Size(144, 46)
        GroupBox5.TabIndex = 23
        GroupBox5.TabStop = False
        GroupBox5.Text = "PSG View"
        ' 
        ' TrackBar1
        ' 
        TrackBar1.LargeChange = 20
        TrackBar1.Location = New Point(260, 244)
        TrackBar1.Maximum = 100
        TrackBar1.Name = "TrackBar1"
        TrackBar1.Size = New Size(104, 45)
        TrackBar1.SmallChange = 5
        TrackBar1.TabIndex = 24
        TrackBar1.TickFrequency = 10
        TrackBar1.Value = 100
        ' 
        ' Label9
        ' 
        Label9.AutoSize = True
        Label9.FlatStyle = FlatStyle.System
        Label9.Location = New Point(166, 250)
        Label9.Name = "Label9"
        Label9.Size = New Size(91, 15)
        Label9.TabIndex = 25
        Label9.Text = "Output volume:"
        ' 
        ' CheckBox10
        ' 
        CheckBox10.AutoSize = True
        CheckBox10.FlatStyle = FlatStyle.System
        CheckBox10.Location = New Point(6, 277)
        CheckBox10.Name = "CheckBox10"
        CheckBox10.Size = New Size(427, 20)
        CheckBox10.TabIndex = 26
        CheckBox10.Text = "enable variable delay linear pcm capture algorithm filesystem writer stream"
        CheckBox10.UseVisualStyleBackColor = True
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(520, 301)
        Controls.Add(CheckBox10)
        Controls.Add(Label9)
        Controls.Add(TrackBar1)
        Controls.Add(GroupBox5)
        Controls.Add(Label8)
        Controls.Add(Label7)
        Controls.Add(NumericUpDown6)
        Controls.Add(GroupBox4)
        Controls.Add(GroupBox3)
        Controls.Add(Button5)
        Controls.Add(Button4)
        Controls.Add(GroupBox2)
        Controls.Add(GroupBox1)
        Controls.Add(CheckBox5)
        Controls.Add(Button3)
        Controls.Add(Button2)
        Controls.Add(NumericUpDown1)
        Controls.Add(Label2)
        Controls.Add(Label1)
        Controls.Add(CheckBox1)
        Controls.Add(Button1)
        FormBorderStyle = FormBorderStyle.Fixed3D
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        Name = "Form1"
        Text = "ShitSID"
        CType(NumericUpDown1, ComponentModel.ISupportInitialize).EndInit()
        GroupBox1.ResumeLayout(False)
        GroupBox1.PerformLayout()
        CType(NumericUpDown2, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown4, ComponentModel.ISupportInitialize).EndInit()
        CType(NumericUpDown3, ComponentModel.ISupportInitialize).EndInit()
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        GroupBox3.ResumeLayout(False)
        GroupBox3.PerformLayout()
        CType(NumericUpDown5, ComponentModel.ISupportInitialize).EndInit()
        GroupBox4.ResumeLayout(False)
        GroupBox4.PerformLayout()
        CType(NumericUpDown6, ComponentModel.ISupportInitialize).EndInit()
        GroupBox5.ResumeLayout(False)
        GroupBox5.PerformLayout()
        CType(TrackBar1, ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents Label6 As Label
    Friend WithEvents NumericUpDown2 As NumericUpDown
    Friend WithEvents Button5 As Button
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents Button6 As Button
    Friend WithEvents NumericUpDown5 As NumericUpDown
    Friend WithEvents Label3 As Label
    Friend WithEvents CheckBox8 As CheckBox
    Friend WithEvents GroupBox4 As GroupBox
    Friend WithEvents RadioButton5 As RadioButton
    Friend WithEvents RadioButton4 As RadioButton
    Friend WithEvents RadioButton3 As RadioButton
    Friend WithEvents NumericUpDown6 As NumericUpDown
    Friend WithEvents Label7 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents SaveFileDialog1 As SaveFileDialog
    Friend WithEvents CheckBox9 As CheckBox
    Friend WithEvents GroupBox5 As GroupBox
    Friend WithEvents TrackBar1 As TrackBar
    Friend WithEvents Label9 As Label
    Friend WithEvents CheckBox10 As CheckBox
    Friend WithEvents Button7 As Button

End Class
