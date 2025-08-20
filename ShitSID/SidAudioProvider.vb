Imports Highbyte.DotNet6502
Imports NAudio.Wave

Public Class SidAudioProvider
    Implements ISampleProvider
    Public Property Volume As Single = 0.33333333333333331 ' <-- floating point precision everyone
    Public sid As ShitSID
    Public sidfile As SidFile
    Public cpu As New CPU
    Public mem As New Memory(65536)
    Private ReadOnly sampleRate As Integer
    Private ReadOnly vWaveFormat As WaveFormat
    Private sidPhase As Double = 0
    Private Const SID_CLOCK_RATE As Double = 985248.0 ' PAL clock in Hz
    Dim cpuClockPhase = 881
    Dim playAddr As UShort
    Dim TimerLoByte As Byte = 0
    Dim TimerHiByte As Byte = 0
    Dim TimerACycle As UShort = 0
    Dim TimerActive As Boolean = False
    Dim TimerTrigger As Boolean = False
    Dim TimerPhase As UShort = 0
    Public UseNTSC As Boolean = False
    Dim NMIVec As UShort = 0
    Public runCPU As Boolean = False
    Public Sub New(sidEmu As ShitSID, Optional sampleRateHz As Integer = 44100)
        sid = sidEmu
        sampleRate = sampleRateHz
        vWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRateHz, 1)
    End Sub
    Public Sub InitSIDFile()
        mem.MapRAM(sidfile.LoadAddress, sidfile.Data)
        cpu.PC = sidfile.InitAddress
        If Form1.NumericUpDown1.Value > 0 Then
            cpu.A = Form1.NumericUpDown1.Value - 1
        Else
            cpu.A = 0
        End If
        cpu.ExecuteUntilBRK(mem)
        For i = &HD400 To &HD41F ' 54272 to 54303
            mem.MapWriter(i, Sub(addr As UShort, value As Byte)
                                 sid.WriteRegister(addr, value)
                             End Sub)
        Next
        mem.MapWriter(56580, Sub(addr As UShort, value As Byte)
                                 TimerLoByte = value
                                 Console.WriteLine($"Timer A every {BitConverter.ToUInt16({TimerLoByte, TimerHiByte})} cycles")
                                 TimerACycle = BitConverter.ToUInt16({TimerLoByte, TimerHiByte})
                             End Sub)
        mem.MapWriter(56581, Sub(addr As UShort, value As Byte)
                                 TimerHiByte = value
                                 Console.WriteLine($"Timer A every {BitConverter.ToUInt16({TimerLoByte, TimerHiByte})} cycles")
                                 TimerACycle = BitConverter.ToUInt16({TimerLoByte, TimerHiByte})
                             End Sub)
        mem.MapWriter(56589, Sub(addr As UShort, value As Byte)
                                 Dim bits As New BitArray({value})
                                 TimerTrigger = bits(0)
                                 Console.WriteLine($"Timer trigger: {TimerTrigger}")
                             End Sub)
        mem.MapWriter(56590, Sub(addr As UShort, value As Byte)
                                 Dim bits As New BitArray({value})
                                 TimerActive = bits(0)
                                 Console.WriteLine($"Timer active: {TimerActive}")
                             End Sub)
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
        Console.WriteLine($"IRQ is {BitConverter.ToUInt16({mem(65534), mem(65535)})}")
        Console.WriteLine($"NMI is {BitConverter.ToUInt16({mem(65530), mem(65531)})}")
        NMIVec = BitConverter.ToUInt16({mem(65530), mem(65531)})
        Console.WriteLine($"Timer A every {BitConverter.ToUInt16({mem(56580), mem(56581)})} cycles")
        cpuClockPhase = If(UseNTSC, 734, 881)

    End Sub
    Public Function Read(buffer() As Single, offset As Integer, count As Integer) As Integer Implements ISampleProvider.Read
        For i = 0 To count - 1
            ' --- clock the SID phase ---
            sidPhase += (SID_CLOCK_RATE / sampleRate)
            cpuClockPhase += 1

            ' --- optional: old frame-based playAddr jump ---
            If cpuClockPhase >= If(UseNTSC, 735, 882) AndAlso cpu.CPUInterrupts.ActiveNMISources.Count = 0 Then ' 1 frame (PAL)
                ' fake interrupt
                cpu.PushWordToStack(cpu.PC, mem)
                cpu.PushByteToStack(0, mem) ' fuck da flags this is purely for stack alignment
                cpu.PC = playAddr
                cpuClockPhase = 0
            End If

            ' --- advance SID phase while handling CPU ---
            While sidPhase >= 1
                If runCPU Then
                    Dim state = cpu.ExecuteOneInstruction(mem)

                    ' --- decrement Timer A if active ---
                    If TimerActive Then
                        TimerPhase += state.CyclesConsumed
                        While TimerPhase >= TimerACycle
                            ' Trigger NMI
                            NMIVec = BitConverter.ToUInt16({mem(65530), mem(65531)})
                            cpu.CPUInterrupts.SetNMISourceActive("CIA2") ' the proper way
                            TimerPhase -= TimerACycle
                        End While
                    End If

                    sidPhase -= state.CyclesConsumed

                    ' Clock the SID for each CPU cycle
                    For cyc = CULng(1) To state.CyclesConsumed
                        sid.Clock()
                    Next

                Else
                    ' If CPU not running, just clock SID
                    sid.Clock()
                    sidPhase -= 1
                End If
            End While

            ' --- write sample to buffer ---
            buffer(offset + i) = CSng(sid.GetSample() * Volume)
        Next

        Return count
    End Function
    Public ReadOnly Property WaveFormat As WaveFormat Implements ISampleProvider.WaveFormat
        Get
            Return vWaveFormat
        End Get
    End Property
End Class