Imports Highbyte.DotNet6502
Imports Highbyte.DotNet6502.Instructions
Imports Highbyte.DotNet6502.Systems.Commodore64.TimerAndPeripheral
Imports Microsoft.Extensions.Logging
Imports NAudio.Wave

Public Class SidAudioProvider
    Implements ISampleProvider
    Public Property Volume As Single = 0.5
    Public sid As ShitSID
    Public sidfile As SidFile
    Public cpu As New CPU
    Public mem As New Memory(65536)
    Public psgView As PSGView
    Public Event PSGViewFrame(frame As Bitmap)
    Private ReadOnly sampleRate As Integer
    Private ReadOnly vWaveFormat As WaveFormat
    Private sidPhase As Long = 0
    Private sidPhaseFrac As Double = 0.0
    Private Const SID_CLOCK_RATE_NTSC As Long = 1022727 ' PAL clock in Hz
    Private Const SID_CLOCK_RATE_PAL As Long = 985248 ' PAL clock in Hz
    Private SIDClockRate As Long = SID_CLOCK_RATE_PAL
    Dim cpuClockPhase As Long = 1
    Dim playAddr As UShort
    Dim CIA2 As Cia2
    Dim volumebuf As New List(Of Byte)
    Dim volphase As Integer = 0
    Const voldivider As Integer = 16
    Private NTSCOn As Boolean = False
    Public Property UseNTSC() As Boolean
        Get
            Return NTSCOn
        End Get
        Set(ByVal value As Boolean)
            NTSCOn = value
            If value Then SIDClockRate = SID_CLOCK_RATE_NTSC Else SIDClockRate = SID_CLOCK_RATE_PAL
        End Set
    End Property
    Dim NMIVec As UShort = 0
    Public runCPU As Boolean = False
    Public TickRate As Integer
    Public Sub New(ByRef sidEmu As ShitSID, ByRef cpuV As CPU, ByRef memV As Memory, ByRef PSGView As PSGView, Optional sampleRateHz As Integer = 44100)
        sid = sidEmu
        cpu = cpuV
        mem = memV
        sampleRate = sampleRateHz
        Me.psgView = PSGView
        vWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRateHz, 1)
        For i = 0 To 255
            volumebuf.Add(15)
        Next
    End Sub
    Public Sub InitSIDFile()
        mem.MapRAM(sidfile.LoadAddress, sidfile.Data)
        cpu.PC = sidfile.InitAddress
        If Form1.NumericUpDown1.Value > 0 Then
            cpu.A = Form1.NumericUpDown1.Value - 1
        Else
            cpu.A = 0
        End If
        For i = &HD400 To &HD41F ' 54272 to 54303
            mem.MapWriter(i, Sub(addr As UShort, value As Byte)
                                 sid.WriteRegister(addr, value)
                             End Sub)
        Next

        mem.MapReader(54299, Function(addr As UShort)
                                 Return sid.Voices(2).GenerateNoEnvelope(sid.CurrentTime) * 127
                             End Function)
        mem.MapReader(54300, Function(addr As UShort)
                                 Return sid.Voices(2).Envelope.Output
                             End Function)
        Dim factory = LoggerFactory.Create(Function(builder)
                                               Return builder.SetMinimumLevel(LogLevel.Debug).AddConsole
                                           End Function)
        CIA2 = New Cia2(cpu, factory)
        CIA2.MapIOLocations(mem)
        cpu.ExecuteUntilBRK(mem)
        mem(0) = &H4C
        mem(1) = &H0
        mem(2) = &H0
        If sidfile.PlayAddress = 0 Then ' RSID mode activate
            playAddr = BitConverter.ToUInt16({mem(788), mem(789)}) ' kernal IRQ vector (rarely used)
            If playAddr = 0 Then
                playAddr = BitConverter.ToUInt16({mem(65534), mem(65535)}) ' IRQ vector
            End If
            MsgBox($"RSID detected, play address set to {playAddr}")
            mem(65534) = 0 ' fucking hack but it works anyway lol (will be removed)
            mem(65535) = 0
        Else
            playAddr = sidfile.PlayAddress
        End If
        System.Console.WriteLine($"IRQ is {BitConverter.ToUInt16({mem(65534), mem(65535)})}")
        System.Console.WriteLine($"NMI is {BitConverter.ToUInt16({mem(65530), mem(65531)})}")
        NMIVec = BitConverter.ToUInt16({mem(65530), mem(65531)})
        System.Console.WriteLine($"Timer A every {BitConverter.ToUInt16({mem(56580), mem(56581)})} cycles")
        cpuClockPhase = sampleRate \ TickRate - 1
    End Sub
    Public Function Read(buffer() As Single, offset As Integer, count As Integer) As Integer Implements ISampleProvider.Read
        Dim phaseIncrement As Double = SIDClockRate / sampleRate
        'Console.WriteLine($"sidPhase: {sidPhase} - sidPhaseFrac: {sidPhaseFrac}")
        For i = 0 To count - 1
            ' --- clock the SID phase ---
            sidPhaseFrac += phaseIncrement
            Dim wholeCycles As Long = CLng(Math.Floor(sidPhaseFrac))
            sidPhase += wholeCycles
            sidPhaseFrac -= wholeCycles
            cpuClockPhase += 1

            If cpuClockPhase >= (sampleRate \ TickRate) - 1 AndAlso cpu.CPUInterrupts.ActiveNMISources.Count = 0 Then ' 1 frame (PAL)
                cpu.CPUInterrupts.SetIRQSourceActive("raster", True)
                cpu.PC = playAddr
                cpuClockPhase -= (sampleRate \ TickRate)
                RaiseEvent PSGViewFrame(psgView.Frame(volumebuf.ToArray, 2))
            End If

            ' --- advance SID phase while handling CPU ---
            While sidPhase >= 1
                If runCPU Then
                    Dim state = cpu.ExecuteOneInstruction(mem)

                    sidPhase -= state.CyclesConsumed

                    ' Clock the SID for each CPU cycle
                    For cyc = CULng(1) To state.CyclesConsumed
                        sid.Clock()
                    Next
                    CIA2.ProcessTimers(state.CyclesConsumed)
                Else
                    ' If CPU not running, just clock SID
                    sid.Clock()
                    sidPhase -= 1
                End If
            End While

            volphase += 1
            If volphase >= voldivider \ 2 Then
                volumebuf.Add(sid.VolumeRegister)
                If volumebuf.Count > 256 Then
                    volumebuf.RemoveAt(0)
                End If
                volphase = 0
            End If

            ' --- write sample to buffer ---
            buffer(offset + i) = CSng(sid.GetSample * Volume) ' test
        Next

        Return count
    End Function
    Public ReadOnly Property WaveFormat As WaveFormat Implements ISampleProvider.WaveFormat
        Get
            Return vWaveFormat
        End Get
    End Property
End Class