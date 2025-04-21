<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class StatsViewer
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Label1 = New Label()
        Timer1 = New Timer(components)
        TableLayoutPanel1 = New TableLayoutPanel()
        GroupBox4 = New GroupBox()
        Label4 = New Label()
        GroupBox3 = New GroupBox()
        Label3 = New Label()
        GroupBox2 = New GroupBox()
        Label2 = New Label()
        GroupBox1 = New GroupBox()
        TableLayoutPanel1.SuspendLayout()
        GroupBox4.SuspendLayout()
        GroupBox3.SuspendLayout()
        GroupBox2.SuspendLayout()
        GroupBox1.SuspendLayout()
        SuspendLayout()
        ' 
        ' Label1
        ' 
        Label1.Dock = DockStyle.Fill
        Label1.Font = New Font("Consolas", 9F)
        Label1.Location = New Point(3, 23)
        Label1.Name = "Label1"
        Label1.Size = New Size(372, 214)
        Label1.TabIndex = 0
        Label1.Text = "Label1aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
        ' 
        ' Timer1
        ' 
        Timer1.Enabled = True
        Timer1.Interval = 16
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.ColumnCount = 2
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50F))
        TableLayoutPanel1.Controls.Add(GroupBox4, 1, 1)
        TableLayoutPanel1.Controls.Add(GroupBox3, 0, 1)
        TableLayoutPanel1.Controls.Add(GroupBox2, 1, 0)
        TableLayoutPanel1.Controls.Add(GroupBox1, 0, 0)
        TableLayoutPanel1.Dock = DockStyle.Fill
        TableLayoutPanel1.Location = New Point(0, 0)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 2
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 50F))
        TableLayoutPanel1.Size = New Size(769, 493)
        TableLayoutPanel1.TabIndex = 1
        ' 
        ' GroupBox4
        ' 
        GroupBox4.Controls.Add(Label4)
        GroupBox4.Dock = DockStyle.Fill
        GroupBox4.Location = New Point(387, 249)
        GroupBox4.Name = "GroupBox4"
        GroupBox4.Size = New Size(379, 241)
        GroupBox4.TabIndex = 5
        GroupBox4.TabStop = False
        GroupBox4.Text = "Voice 3"
        ' 
        ' Label4
        ' 
        Label4.Dock = DockStyle.Fill
        Label4.Font = New Font("Consolas", 9F)
        Label4.Location = New Point(3, 23)
        Label4.Name = "Label4"
        Label4.Size = New Size(373, 215)
        Label4.TabIndex = 0
        Label4.Text = "Label4"
        ' 
        ' GroupBox3
        ' 
        GroupBox3.Controls.Add(Label3)
        GroupBox3.Dock = DockStyle.Fill
        GroupBox3.Location = New Point(3, 249)
        GroupBox3.Name = "GroupBox3"
        GroupBox3.Size = New Size(378, 241)
        GroupBox3.TabIndex = 4
        GroupBox3.TabStop = False
        GroupBox3.Text = "Voice 2"
        ' 
        ' Label3
        ' 
        Label3.Dock = DockStyle.Fill
        Label3.Font = New Font("Consolas", 9F)
        Label3.Location = New Point(3, 23)
        Label3.Name = "Label3"
        Label3.Size = New Size(372, 215)
        Label3.TabIndex = 0
        Label3.Text = "Label3"
        ' 
        ' GroupBox2
        ' 
        GroupBox2.Controls.Add(Label2)
        GroupBox2.Dock = DockStyle.Fill
        GroupBox2.Location = New Point(387, 3)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(379, 240)
        GroupBox2.TabIndex = 3
        GroupBox2.TabStop = False
        GroupBox2.Text = "Voice 1"
        ' 
        ' Label2
        ' 
        Label2.Dock = DockStyle.Fill
        Label2.Font = New Font("Consolas", 9F)
        Label2.Location = New Point(3, 23)
        Label2.Name = "Label2"
        Label2.Size = New Size(373, 214)
        Label2.TabIndex = 0
        Label2.Text = "Label2"
        ' 
        ' GroupBox1
        ' 
        GroupBox1.Controls.Add(Label1)
        GroupBox1.Dock = DockStyle.Fill
        GroupBox1.Location = New Point(3, 3)
        GroupBox1.Name = "GroupBox1"
        GroupBox1.Size = New Size(378, 240)
        GroupBox1.TabIndex = 2
        GroupBox1.TabStop = False
        GroupBox1.Text = "Global"
        ' 
        ' StatsViewer
        ' 
        AutoScaleDimensions = New SizeF(8F, 20F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.White
        ClientSize = New Size(769, 493)
        Controls.Add(TableLayoutPanel1)
        Name = "StatsViewer"
        Text = "Statistics"
        TableLayoutPanel1.ResumeLayout(False)
        GroupBox4.ResumeLayout(False)
        GroupBox3.ResumeLayout(False)
        GroupBox2.ResumeLayout(False)
        GroupBox1.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Timer1 As Timer
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents GroupBox4 As GroupBox
    Friend WithEvents Label4 As Label
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents Label3 As Label
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents Label2 As Label
    Friend WithEvents GroupBox1 As GroupBox
End Class
