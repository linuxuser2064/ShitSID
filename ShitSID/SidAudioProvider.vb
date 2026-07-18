Imports System.Reflection.Emit
Imports System.Threading
Imports System.Windows.Forms.AxHost
Imports Highbyte.DotNet6502
Imports Highbyte.DotNet6502.Instructions
Imports Highbyte.DotNet6502.Systems.Commodore64.TimerAndPeripheral
Imports Highbyte.DotNet6502.Utils
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
    Dim CIA1 As Cia1
    Dim CIA2 As Cia2
    Dim volumebuf As New List(Of Byte)
    Dim volphase As Integer = 0

    Public EnablePSGView As Boolean = True
    Public PSGViewDivider As Integer = 8
    Dim psgPhase As Integer = 0

    Public EnablePCMCapture As Boolean = False
    Dim lastVol As Byte = 0
    Dim CycleCounter As UInt128 = 0

    Public ReadCSVDump As Boolean = False ' the big one
    Public reader As SidTraceReader
    Dim sampleCounter As Integer = 0
    Dim cycleDelay As Long = 50
    Dim data As SidTraceEntry = New SidTraceEntry With {.AbsoluteCycles = 0, .RelativeCycles = 50, .Address = &HD400, .Value = 0}
    Public ReadOnly Property voldivider As Integer
        Get
            Return CInt(sampleRate / 5512.5)
        End Get
    End Property
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
    Public Sub New(ByRef sidEmu As ShitSID, traceReader As SidTraceReader, ByRef PSGView As PSGView, Optional sampleRateHz As Integer = 44100)
        Me.sampleRate = sampleRateHz
        Me.sid = sidEmu
        Me.psgView = PSGView
        vWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRateHz, 1)
        For i = 0 To 255
            volumebuf.Add(15)
        Next
        reader = traceReader
        ReadCSVDump = True
    End Sub
    Public Sub DisAsmState(state As ExecState, Optional wait As Boolean = False)
        If cpu.PC = &H0 Then Exit Sub
        If wait Then Thread.Sleep(1)
        Dim size = cpu.GetInstructionSize(mem, state.PCBeforeLastOpCodeExecuted)
        Dim out = ""
        For i = 1 To size - 1
            Dim data = mem(state.PCBeforeLastOpCodeExecuted + i)
            out &= $"{data:X2} "
        Next
        If state.LastInstructionExecResult.OpCodeByte = OpCodeId.PHA Or
            state.LastInstructionExecResult.OpCodeByte = OpCodeId.PHP Then
            System.Console.WriteLine($"[SP {cpu.SP} PC {cpu.PC:X4}] Pushed {cpu.A:X2} to stack")
            Exit Sub
        End If
        If state.LastInstructionExecResult.OpCodeByte = OpCodeId.PLA Or
            state.LastInstructionExecResult.OpCodeByte = OpCodeId.PLP Then
            System.Console.WriteLine($"[SP {cpu.SP} PC {cpu.PC:X4}] Pulled {cpu.A:X2} from stack")
            Exit Sub
        End If
        If state.LastInstructionExecResult.OpCodeByte = OpCodeId.TXS Then
            System.Console.WriteLine($"[SP {cpu.SP} PC {cpu.PC:X4}] Transferred {cpu.X} to SP")
            Exit Sub
        End If
        Static LastSP = 0
        If LastSP <> cpu.SP Then
            System.Console.WriteLine($"[SP {cpu.SP} PC {cpu.PC:X4}]")
        End If
        System.Console.WriteLine($"PC: {cpu.PC:X4} A:{cpu.A:X2} X:{cpu.X:X2} Y:{cpu.Y:X2} {CType(state.LastInstructionExecResult.OpCodeByte, OpCodeId)} {out}")
        LastSP = cpu.SP
    End Sub
    Public Sub InitSIDFile()
        If ReadCSVDump Then Exit Sub
        'mem.MapRAM(sidfile.LoadAddress, sidfile.Data)
        For i = sidfile.LoadAddress To sidfile.LoadAddress + sidfile.Data.Length - 1
            mem(i) = sidfile.Data(i - sidfile.LoadAddress)
        Next
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
        If EnablePCMCapture Then
            mem.MapWriter(&HD418, Sub(addr, value)

                                  End Sub)
        End If
        mem.MapReader(54299, Function(addr As UShort)
                                 Return sid.Voices(2).GenerateNoEnvelope() * 127
                             End Function)
        mem.MapReader(54300, Function(addr As UShort)
                                 Return sid.Voices(2).Envelope.Output
                             End Function)
        Dim factory = LoggerFactory.Create(Function(builder)
                                               Return builder.SetMinimumLevel(LogLevel.Debug).AddConsole
                                           End Function)
        CIA2 = New Cia2(cpu, factory)
        CIA2.MapIOLocations(mem)
        'CIA1 = New Cia1(cpu, factory)
        'CIA1.MapIOLocations(mem)
        While True ' nuclear while
            Dim state = cpu.ExecuteOneInstruction(mem)
            CIA2.ProcessTimers(state.CyclesConsumed)
            'CIA1.ProcessTimers(state.CyclesConsumed)
            DisAsmState(state)
            'If state.LastInstructionExecResult.OpCodeByte = &H60 Then Exit While
            'If state.LastInstructionExecResult.OpCodeByte = &H40 Then Exit While
            If state.LastInstructionExecResult.OpCodeByte = &H0 Then Exit While
            'Thread.Sleep(1)
        End While
        'cpu.ExecuteUntilBRK(mem)

        Dim SidDataEnd As UShort = sidfile.LoadAddress + sidfile.Data.Length


        If sidfile.StartPage = 255 Then
            ' cooked
            SidDataEnd = &HFFF0
        ElseIf sidfile.StartPage > 0 Then
            ' between 1 and 254
            SidDataEnd = (sidfile.StartPage * 256) + (sidfile.PageLength * 256)
        End If
        System.Console.WriteLine($"SID data region end: {SidDataEnd:X4}")
        'Const driverAddress As UShort = &HFFF0
        Dim driverAddress = SidDataEnd
        mem.MapROM(driverAddress, {&H4C, driverAddress.Lowbyte, driverAddress.Highbyte})
        mem(driverAddress) = &H4C
        mem(driverAddress + 1) = driverAddress.Lowbyte
        mem(driverAddress + 2) = driverAddress.Highbyte
        If sidfile.PlayAddress = 0 Then ' RSID mode activate
            playAddr = BitConverter.ToUInt16({mem(788), mem(789)}) ' kernal IRQ vector (rarely used)
            If playAddr = 0 Then
                playAddr = BitConverter.ToUInt16({mem(65534), mem(65535)}) ' IRQ vector
            End If
            MsgBox($"RSID detected, play address set to {playAddr}")
        Else
            playAddr = sidfile.PlayAddress
            'mem(65534) = playAddr And &HFF
            'mem(65535) = (playAddr And &HFF00) >> 8
        End If
        mem(65534) = &H0 ' fucking hack but it works anyway lol (will be removed)
        mem(65535) = &H0
        System.Console.WriteLine($"Play address is {playAddr:X4}")
        System.Console.WriteLine($"IRQ is {BitConverter.ToUInt16({mem(65534), mem(65535)})}")
        System.Console.WriteLine($"NMI is {BitConverter.ToUInt16({mem(65530), mem(65531)})}")
        System.Console.WriteLine($"Timer A every {BitConverter.ToUInt16({mem(56580), mem(56581)})} cycles")
        cpuClockPhase = 0 ' sampleRate \ TickRate - 1
        cpu.PC = &H0
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

            If Not ReadCSVDump Then
                cpuClockPhase += 1

                If cpuClockPhase >= (sampleRate \ TickRate) - 1 AndAlso cpu.CPUInterrupts.ActiveNMISources.Count = 0 Then ' 1 frame
                    cpu.CPUInterrupts.SetIRQSourceActive("raster", True)
                    cpu.PC = playAddr
                    cpuClockPhase -= (sampleRate \ TickRate)
                    If EnablePSGView Then
                        If psgPhase = 0 Then RaiseEvent PSGViewFrame(psgView.Frame(volumebuf.ToArray, 2))
                        psgPhase += 1
                        psgPhase = psgPhase Mod PSGViewDivider
                    End If
                End If
            End If

            ' --- advance SID phase while handling CPU ---
            While sidPhase >= 1
                If ReadCSVDump Then
                    ' clock cycle
                    If data.Address = 0 Then
                        Return 0 ' end of stream
                    End If
                    If cycleDelay >= data.RelativeCycles Then
                        sid.WriteRegister(data.Address, data.Value)
                        data = reader.ReadNext
                        cycleDelay = 0
                    End If
                    sidPhase -= 1
                    cycleDelay += 1
                    sid.Clock()
                    CycleCounter += 1
                Else
                    If runCPU Then
                        Dim state = cpu.ExecuteOneInstruction(mem)
                        'DisAsmState(state, True)
                        sidPhase -= state.CyclesConsumed
                        CycleCounter += state.CyclesConsumed
                        ' Clock the SID for each CPU cycle
                        For cyc = CULng(1) To state.CyclesConsumed
                            sid.Clock()
                        Next
                        CIA2.ProcessTimers(state.CyclesConsumed)
                        'CIA1.ProcessTimers(state.CyclesConsumed)
                    Else
                        ' If CPU not running, just clock SID
                        sid.Clock()
                        sidPhase -= 1
                    End If
                End If
            End While

            If ReadCSVDump Then
                If sampleCounter = (sampleRate \ 50) Then
                    If EnablePSGView Then
                        If psgPhase = 0 Then RaiseEvent PSGViewFrame(psgView.Frame(volumebuf.ToArray, 2))
                        psgPhase += 1
                        psgPhase = psgPhase Mod PSGViewDivider
                    End If
                    sampleCounter = 0
                End If
                sampleCounter += 1
            End If

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