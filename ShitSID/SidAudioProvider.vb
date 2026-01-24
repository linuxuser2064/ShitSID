Imports Highbyte.DotNet6502
Imports Highbyte.DotNet6502.Systems.Commodore64.TimerAndPeripheral
Imports Microsoft.Extensions.Logging
Imports NAudio.Wave

Public Class SidAudioProvider
    Implements ISampleProvider
    Public Property Volume As Single = 0.5
    Public sid As ShitSID
    Public sidfile As SidFile
    Public cpu As CPU
    Public mem As Memory
    Private ReadOnly sampleRate As Integer
    Private ReadOnly vWaveFormat As WaveFormat
    Private Const SID_CLOCK_RATE As Double = 985248 ' PAL clock in Hz
    Public TickRate As Integer = 50
    Public runCPU As Boolean = False

    Dim IsRSID As Boolean = False
    Dim playAddr As UShort

    Private sidPhase As Double = 0.0
    Private Const cpuCyclesPerSecond As Double = 985248.0

    Dim CIA1 As Cia1
    Dim CIA2 As Cia2
    Dim VICII As FakeVICII

    Public Const LOG_INSTRUCTIONS As Boolean = False
    Public Sub PrintLn(str As String)
        System.Console.WriteLine(str)
    End Sub
    Public Sub New(ByRef sidEmu As ShitSID, ByRef proc As CPU, ByRef memory As Memory, Optional sampleRateHz As Integer = 88200)
        sid = sidEmu
        cpu = proc
        mem = memory
        sampleRate = sampleRateHz
        vWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRateHz, 1)
        PrintLn($"[AUDIO] Sample rate: {sampleRateHz}")
    End Sub
    Private Sub InstallRasterPlayerIRQ()

        Dim irqStub As UShort = &HFF80

        ' IRQ vector
        mem(&HFFFE) = irqStub And &HFF
        mem(&HFFFF) = irqStub >> 8

        Dim data As New List(Of Byte)

        ' --- prologue ---
        data.Add(&H48)             ' PHA ff80
        data.Add(&H8A)             ' TXA ff81
        data.Add(&H48)             ' PHA ff82
        data.Add(&H98)             ' TYA ff83
        data.Add(&H48)             ' PHA ff84
        data.Add(&H8)             ' PHP  ff85 

        ' --- acknowledge VIC raster IRQ ---
        data.Add(&HA9)             ' LDA #$01 ff86
        data.Add(&H1)                       ' ff87
        data.Add(&H8D)            ' STA $D019 ff88
        data.Add(&H19)           '            ff89
        data.Add(&HD0)           '            ff8a

        ' --- acknowledge CIA IRQs ---
        data.Add(&HAD) : data.Add(&HD) : data.Add(&HDC) ' LDA $DC0D ' ff8d
        data.Add(&HAD) : data.Add(&HD) : data.Add(&HDD) ' LDA $DD0D ' ff90

        ' --- call play routine ---
        data.Add(&H20)             ' JSR playAddr       'ff91
        data.Add(playAddr And &HFF) '                    ff92
        data.Add(playAddr >> 8)     '                    ff93

        ' --- epilogue ---
        data.Add(&H28)             ' PLP ff94
        data.Add(&H68)             ' PLA ff95
        data.Add(&HA8)             ' TAY ff96
        data.Add(&H68)             ' PLA ff97
        data.Add(&HAA)             ' TAX ff98
        data.Add(&H68)             ' PLA ff99
        data.Add(&H40)             ' RTI ff9a

        data.Add(&H40)             ' rogue RTI ff9b

        mem.MapROM(irqStub, data.ToArray())

        ' --- enable raster IRQ ---
        mem(&HD011) = &H1B          ' screen on, raster MSB = 0
        mem(&HD012) = &H0          ' trigger on line 0
        mem(&HD01A) = &H1          ' enable raster IRQ
        mem(&HD019) = &H1          ' clear pending raster IRQ
    End Sub
    Public Sub InitSIDFile()
        Dim factory = LoggerFactory.Create(
    Sub(builder)
        builder.SetMinimumLevel(LogLevel.Debug)
        builder.AddConsole
    End Sub
)
        CIA1 = New Cia1(cpu, factory)
        CIA2 = New Cia2(cpu, factory)
        CIA1.MapIOLocations(mem)
        CIA2.MapIOLocations(mem)
        VICII = New FakeVICII(cpu, mem)
        mem.MapRAM(sidfile.LoadAddress, sidfile.Data)
        cpu.PC = sidfile.InitAddress
        If Form1.NumericUpDown1.Value > 0 Then
            cpu.A = Form1.NumericUpDown1.Value - 1
        Else
            cpu.A = 0
        End If
        For i = &HD400 To &HD41F ' 54272 to 54303
            mem.MapWriter(i, Sub(addr As UShort, value As Byte) sid.WriteRegister(addr, value))
            mem.MapReader(i, Function(addr As UShort) sid.ReadRegister(addr))
        Next
        If sidfile.PlayAddress = 0 Then ' RSID mode activate
            MsgBox($"RSID detected")
            IsRSID = True
        Else
            playAddr = sidfile.PlayAddress
            InstallRasterPlayerIRQ()
        End If
        PrintLn($"[Loader] IRQ is {BitConverter.ToUInt16({mem(65534), mem(65535)})}")
        PrintLn($"[Loader] NMI is {BitConverter.ToUInt16({mem(65530), mem(65531)})}")
        PrintLn($"[Loader] Timer A every {BitConverter.ToUInt16({mem(56580), mem(56581)})} cycles")
        'cyclesPerSample = SID_CLOCK_RATE / sampleRate
        PrintLn("[CPU] Init RUN")
        While True ' run init routine
            Dim state = cpu.ExecuteOneInstruction(mem)
            PrintLn($"[CPU] {GetInstructionString(state.LastInstructionExecResult)}")
            CIA1.ProcessTimers(state.CyclesConsumed)
            CIA2.ProcessTimers(state.CyclesConsumed)
            For i = CULng(1) To state.CyclesConsumed
                VICII.Clock()
                sid.Clock()
            Next
            If state.LastInstructionExecResult.OpCodeByte = &H20 Or state.LastInstructionExecResult.OpCodeByte = &H0 Then Exit While
        End While
        PrintLn("[CPU] Init END")
        cpu.ProcessorStatus.Decimal = False
        cpu.ProcessorStatus.InterruptDisable = False
        ' insert tiny program to keep the cpu busy instead of disintegrating on the spot
        Dim mainLoopAddr As UShort = &HFF00
        mem.MapROM(&H336, {&H95, &HFF}) ' little endian moment
        mem.MapROM(&H314, {&H95, &HFF})
        mem.MapROM(mainLoopAddr, {
    &HEA,        ' NOP
    &H4C, &H0, &HFF  ' JMP $FF00
})
        cpu.PC = mainLoopAddr
        'RunC64Cycles(10, False) ' run the mainloop a bit
    End Sub
    Public Function Read(buffer() As Single, offset As Integer, count As Integer) As Integer _
    Implements ISampleProvider.Read
        For i = 0 To count - 1

            ' accumulate SID/CPU time
            sidPhase += cpuCyclesPerSecond / sampleRate

            While sidPhase >= 1.0
                ' run exactly ONE CPU cycle worth of time
                Dim state = cpu.ExecuteOneInstruction(mem)

                If LOG_INSTRUCTIONS Then
                    PrintLn($"[CPU] {GetInstructionString(state.LastInstructionExecResult)}")
                End If

                If cpu.PC = &HFF80 Then PrintLn("[CPU] IRQ Stub RUN")
                If cpu.PC = &HFF92 Then PrintLn("[CPU] IRQ Stub RETURN")
                If cpu.PC = playAddr Then PrintLn("[CPU] Play routine RUN")
                If state.LastInstructionExecResult.OpCodeByte = &H0 Then PrintLn($"[CPU] BRK instruction hit at {state.PCBeforeLastOpCodeExecuted}")

                CIA1.ProcessTimers(state.CyclesConsumed)
                CIA2.ProcessTimers(state.CyclesConsumed)

                For g = CULng(1) To state.CyclesConsumed
                    sid.Clock()
                    VICII.Clock()
                Next

                sidPhase -= state.CyclesConsumed
            End While

            buffer(offset + i) = CSng(sid.GetSample * Volume)
        Next
        Return count
    End Function
    Public Function GetInstructionString(result As InstructionExecResult) As String
        Dim opCode As OpCode
        Try
            opCode = cpu.InstructionList.GetOpCode(result.OpCodeByte)
        Catch ex As Exception
            Return "INVALID OPCODE"
        End Try
        Dim incName = opCode.Code.ToString '.Substring(0, 3)
        Dim afterBytes As String = ""
        For i = 1 To opCode.Size - 1
            afterBytes += $"{mem(result.AtPC + i).ToString("X2")} "
        Next
        afterBytes = afterBytes.Trim
        Return $"${result.AtPC.ToString("X4")}: {incName} {afterBytes}"
    End Function
    Public ReadOnly Property WaveFormat As WaveFormat Implements ISampleProvider.WaveFormat
        Get
            Return vWaveFormat
        End Get
    End Property
End Class