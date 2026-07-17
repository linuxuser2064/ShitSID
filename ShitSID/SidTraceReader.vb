Imports System.IO

Public Structure SidTraceEntry
    Public AbsoluteCycles As Integer
    Public RelativeCycles As Integer
    Public Address As UShort
    Public Value As Byte
End Structure

Public Class SidTraceReader
    Implements IDisposable

    Private SR As StreamReader

    Public Sub New(file As String)
        SR = New StreamReader(file)

        'skip header
        SR.ReadLine()
    End Sub

    Public Function ReadNext() As SidTraceEntry
        If SR.EndOfStream Then Return New SidTraceEntry With {.Address = 0}

        Dim line As String = SR.ReadLine()
        If String.IsNullOrWhiteSpace(line) Then
            Return New SidTraceEntry With {.Address = 0}
        End If

        Dim len As Integer = line.Length
        Dim fields(4) As String

        Dim fieldIndex As Integer = 0
        Dim start As Integer = 0
        Dim inQuotes As Boolean = False

        For i As Integer = 0 To len - 1
            Dim c As Char = line(i)

            If c = """"c Then
                inQuotes = Not inQuotes

            ElseIf c = ","c AndAlso Not inQuotes Then
                fields(fieldIndex) = line.Substring(start, i - start).Trim(" "c, """"c)
                fieldIndex += 1
                start = i + 1

                If fieldIndex = 4 Then Exit For
            End If
        Next

        ' last field (description + rest of line)
        fields(4) = line.Substring(start).Trim(" "c, """"c)

        Dim x As New SidTraceEntry

        x.AbsoluteCycles = Integer.Parse(fields(0))
        x.RelativeCycles = Integer.Parse(fields(1))

        x.Address = Convert.ToUInt16(fields(2).Substring(1), 16)
        x.Value = Convert.ToByte(fields(3).Substring(1), 16)

        Return x
    End Function
    Private Function ParseCsv(line As String) As List(Of String)

        Dim result As New List(Of String)
        Dim current As New System.Text.StringBuilder
        Dim inQuotes As Boolean = False

        For Each c In line

            If c = """"c Then
                inQuotes = Not inQuotes

            ElseIf c = ","c AndAlso Not inQuotes Then

                result.Add(current.ToString)
                current.Clear()

            Else
                current.Append(c)
            End If
        Next

        result.Add(current.ToString)

        Return result
    End Function

    Public Sub Dispose() Implements IDisposable.Dispose
        SR?.Dispose()
    End Sub
End Class