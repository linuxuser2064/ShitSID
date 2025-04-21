Imports NAudio.Wave

Public Class SidAudioProvider
    Implements ISampleProvider
    Public Property Volume As Single = 0.33333333333333331 ' <-- floating point precision everyone
    Public sid As ShitSID
    Private ReadOnly sampleRate As Integer
    Private ReadOnly vWaveFormat As WaveFormat
    Private sidPhase As Double = 0
    Private Const SID_CLOCK_RATE As Double = 985248.0 ' PAL clock in Hz
    Public DoubleSpeed As Boolean = False
    Public Sub New(sidEmu As ShitSID, Optional sampleRateHz As Integer = 44100)
        sid = sidEmu
        sampleRate = sampleRateHz
        vWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRateHz, 1)
    End Sub

    Public Function Read(buffer() As Single, offset As Integer, count As Integer) As Integer Implements ISampleProvider.Read
        For i = 0 To count - 1
            ' clock the sid
            sidPhase += SID_CLOCK_RATE / sampleRate

            While sidPhase >= 1
                sid.Clock()
                If DoubleSpeed Then sid.Clock()
                sidPhase -= 1
            End While

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
