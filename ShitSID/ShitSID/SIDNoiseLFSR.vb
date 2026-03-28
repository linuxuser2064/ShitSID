Public Class SIDNoiseLFSR
    ' 23-bit shift register (SID initial state, all ones except LSBs)
    Private noiseSR As Integer = &H7FFFFC

    ' Track previous accumulator to detect rising edge
    Private prevAcc As Integer = 0

    ' Bit mask for rising edge detection (SID uses bit 19)
    Private Const BIT19 As Integer = &H80000

    ' Call once per sample, passing the oscillator accumulator (24-bit integer)
    Public Function Read(acc As Integer) As Integer
        ' Detect rising edge of accumulator bit 19
        'If ((prevAcc And BIT19) = 0) AndAlso ((acc And BIT19) <> 0) Then
        Shift()
        'End If

        prevAcc = acc

        ' Convert shift register state to SID-style 8-bit noise
        Dim noise8 As Integer =
    ((noiseSR >> 22) And 1) << 7 Or
    ((noiseSR >> 20) And 1) << 6 Or
    ((noiseSR >> 16) And 1) << 5 Or
    ((noiseSR >> 13) And 1) << 4 Or
    ((noiseSR >> 11) And 1) << 3 Or
    ((noiseSR >> 7) And 1) << 2 Or
    ((noiseSR >> 4) And 1) << 1 Or
    ((noiseSR >> 2) And 1)

        ' Convert to 12-bit output (0–4095)
        Return (noise8 << 4) ' multiply by 16
    End Function

    ' Single SID noise shift step
    Private Sub Shift()
        ' New bit = XOR taps at bit22 and bit17
        Dim newBit As Integer = ((noiseSR >> 22) Xor (noiseSR >> 17)) And 1
        noiseSR = ((noiseSR << 1) And &H7FFFFF) Or newBit ' Keep 23 bits
    End Sub
End Class