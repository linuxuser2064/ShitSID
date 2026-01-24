Imports Highbyte.DotNet6502

Public Class FakeVICII
    Private cpu As CPU
    Private mem As Memory

    ' --- timing ---
    Public CpuHz As Double = 985248.0
    Public RasterLines As Integer = 312
    Public CyclesPerLine As Integer = 63

    ' --- internal counters ---
    Private cycleInLine As Integer = 0
    Private rasterLine As Integer = 0

    ' --- registers ---
    Private d011 As Byte = 0
    Private d012 As Byte = 0
    Private d019 As Byte = 0
    Private d01a As Byte = 0

    Private rasterIRQFired As Boolean = False

    Public Sub New(ByRef cpu As CPU, ByRef mem As Memory)
        Me.cpu = cpu
        Me.mem = mem

        ' register hooks
        mem.MapReader(&HD011, Function(a)
                                  Console.WriteLine($"[VIC] $D011 READ {d011.ToString("X2")}")
                                  Return d011
                              End Function)
        mem.MapWriter(&HD011, Sub(a, v)
                                  Console.WriteLine($"[VIC] $D011 WRITE {v.ToString("X2")}")
                                  d011 = v
                              End Sub)

        mem.MapReader(&HD012, Function(a)
                                  Console.WriteLine($"[VIC] $D012 READ {CByte(rasterLine And &HFF).ToString("X2")}")
                                  Return CByte(rasterLine And &HFF)
                              End Function)
        mem.MapWriter(&HD012, Sub(a, v)
                                  Console.WriteLine($"[VIC] $D012 WRITE {v.ToString("X2")}")
                                  d012 = v
                              End Sub)

        mem.MapReader(&HD019, Function(a)
                                  Console.WriteLine($"[VIC] $D019 READ {d019.ToString("X2")}")
                                  Return d019
                              End Function)
        mem.MapWriter(&HD019, Sub(a, v)
                                  Console.WriteLine($"[VIC] $D019 WRITE {v.ToString("X2")}")
                                  ' write-1-to-clear
                                  d019 = d019 And Not v
                                  UpdateIRQLine()
                              End Sub)

        mem.MapReader(&HD01A, Function(a)
                                  Console.WriteLine($"[VIC] $D01A READ {d01a.ToString("X2")}")
                                  Return d01a
                              End Function)
        mem.MapWriter(&HD01A, Sub(a, v)
                                  Console.WriteLine($"[VIC] $D01A WRITE {v.ToString("X2")}")
                                  d01a = v
                                  UpdateIRQLine()
                              End Sub)
    End Sub

    Private totalCycles As Integer = 0

    Public Sub Clock()
        totalCycles += 1

        cycleInLine += 1
        If cycleInLine = 63 Then
            cycleInLine = 0
            rasterLine += 1
            If rasterLine = 312 Then rasterLine = 0
            CheckRasterIRQ()
        End If

        'If totalCycles = 19656 Then
        '    Console.WriteLine("[VIC] Triggering IRQ")
        '    cpu.CPUInterrupts.SetIRQSourceActive("raster", True)
        '    totalCycles = 0
        'End If
    End Sub

    Private Sub CheckRasterIRQ()
        Dim targetRaster As Integer =
        (If((d011 And &H80) <> 0, 256, 0)) Or d012

        ' Only set flag if not already set
        If rasterLine = targetRaster AndAlso (d019 And &H1) = 0 Then
            d019 = d019 Or &H1
            UpdateIRQLine()
        End If
    End Sub


    Private Sub UpdateIRQLine()
        ' IRQ is asserted ONLY if:
        '  - raster IRQ flag is set
        '  - raster IRQ is enabled
        Dim irqActive As Boolean =
        ((d019 And &H1) <> 0) AndAlso ((d01a And &H1) <> 0)

        If irqActive Then
            'Console.WriteLine("[VIC] Triggering interrupt")
            cpu.CPUInterrupts.SetIRQSourceActive("raster", False)
        Else
            'Console.WriteLine("[VIC] NOT triggering interrupt")
            cpu.CPUInterrupts.SetIRQSourceInactive("raster")
        End If
    End Sub
End Class
