Imports System.IO
Imports System.Text

Public Enum SidFormat
    PSID
    RSID
End Enum

Public Enum SidVideoStandard As UInt16
    Unknown = 0
    PAL = 1   ' 50Hz
    NTSC = 2  ' 60Hz
    Both = 3
End Enum

Public Enum SidModel As UInt16
    Unknown = 0
    MOS6581 = 1
    MOS8580 = 2
    Both = 3
End Enum

Public Class SidWriter
    ' --- Core Properties ---
    Public Property Format As SidFormat = SidFormat.PSID

    ''' <summary>
    ''' 0 means the load address is read from the first 2 bytes of the Data array.
    ''' For RSID, this MUST be 0 (enforced automatically on save).
    ''' </summary>
    Public Property LoadAddress As UInt16 = 0
    Public Property InitAddress As UInt16 = 0

    ''' <summary>
    ''' Forced to 0 when generating an RSID file.
    ''' </summary>
    Public Property PlayAddress As UInt16 = 0

    Public Property Songs As UInt16 = 1
    Public Property StartSong As UInt16 = 1

    ''' <summary>
    ''' False = VBI (Vertical Blank, 50Hz PAL/60Hz NTSC). 
    ''' True = CIA 1 Timer (60Hz default). Forced to False for RSID.
    ''' </summary>
    Public Property UseCiaTimer As Boolean = False

    Public Property VideoStandard As SidVideoStandard = SidVideoStandard.PAL
    Public Property Model As SidModel = SidModel.MOS8580

    ' --- Metadata ---
    Public Property Name As String = ""
    Public Property Author As String = ""
    Public Property Released As String = ""

    ' --- C64 Binary Data ---
    ''' <summary>
    ''' The actual C64 binary data. If LoadAddress is 0, the first 2 bytes 
    ''' MUST be the little-endian load address.
    ''' </summary>
    Public Property Data As Byte()

    Public Function GetBytes() As Byte()
        If Data Is Nothing OrElse Data.Length = 0 Then
            Throw New InvalidOperationException("SID Data cannot be empty.")
        End If

        Using ms As New MemoryStream()
            Using writer As New BinaryWriter(ms)
                ' +00 magicID
                If Format = SidFormat.RSID Then
                    writer.Write(Encoding.ASCII.GetBytes("RSID"))
                Else
                    writer.Write(Encoding.ASCII.GetBytes("PSID"))
                End If

                ' +04 version (always 0002 for this spec level)
                WriteBE(writer, CUShort(2))

                ' +06 dataOffset (always 0x7C for version 2)
                WriteBE(writer, CUShort(&H7C))

                ' +08 loadAddress (RSID forces 0)
                If Format = SidFormat.RSID Then
                    WriteBE(writer, CUShort(0))
                Else
                    WriteBE(writer, LoadAddress)
                End If

                ' +0A initAddress
                WriteBE(writer, InitAddress)

                ' +0C playAddress (RSID forces 0)
                If Format = SidFormat.RSID Then
                    WriteBE(writer, CUShort(0))
                Else
                    WriteBE(writer, PlayAddress)
                End If

                ' +0E songs
                WriteBE(writer, Songs)

                ' +10 startSong
                WriteBE(writer, StartSong)

                ' +12 speed (32-bit). RSID forces 0. 
                ' If UseCiaTimer is true, set all 32 bits to 1 (applies to all 32 max songs).
                If Format = SidFormat.RSID OrElse Not UseCiaTimer Then
                    WriteBE(writer, CUInt(0))
                Else
                    WriteBE(writer, CUInt(&HFFFFFFFFUI))
                End If

                ' +16 name (32 bytes)
                WriteString32(writer, Name)
                ' +36 author (32 bytes)
                WriteString32(writer, Author)
                ' +56 released (32 bytes)
                WriteString32(writer, Released)

                ' +76 flags
                Dim flags As UInt16 = 0
                flags = flags Or (CUShort(VideoStandard) << 2) ' Bits 2-3
                flags = flags Or (CUShort(Model) << 4)         ' Bits 4-5
                WriteBE(writer, flags)

                ' +78 startPage (clean = 0)
                writer.Write(CByte(0))
                ' +79 pageLength (clean = 0)
                writer.Write(CByte(0))
                ' +7A secondSIDAddress (v3 feature, set to 0 for v2)
                writer.Write(CByte(0))
                ' +7B thirdSIDAddress (v4 feature, set to 0 for v2)
                writer.Write(CByte(0))

                ' +7C <data>
                writer.Write(Data)
            End Using
            Return ms.ToArray()
        End Using
    End Function

    Public Sub Save(filePath As String)
        File.WriteAllBytes(filePath, GetBytes())
    End Sub

    ' --- Private Helpers ---

    ''' <summary>
    ''' Writes a 16-bit unsigned integer in Big-Endian format.
    ''' </summary>
    Private Sub WriteBE(writer As BinaryWriter, value As UInt16)
        Dim bytes() As Byte = BitConverter.GetBytes(value)
        If BitConverter.IsLittleEndian Then Array.Reverse(bytes)
        writer.Write(bytes)
    End Sub

    ''' <summary>
    ''' Writes a 32-bit unsigned integer in Big-Endian format.
    ''' </summary>
    Private Sub WriteBE(writer As BinaryWriter, value As UInt32)
        Dim bytes() As Byte = BitConverter.GetBytes(value)
        If BitConverter.IsLittleEndian Then Array.Reverse(bytes)
        writer.Write(bytes)
    End Sub

    ''' <summary>
    ''' Writes exactly 32 bytes for a string, truncating or padding with 0x00.
    ''' </summary>
    Private Sub WriteString32(writer As BinaryWriter, text As String)
        Dim bytes() As Byte = New Byte(31) {} ' Initialize with 32 zeros
        If Not String.IsNullOrEmpty(text) Then
            Dim textBytes() As Byte = Encoding.ASCII.GetBytes(text)
            Dim lengthToCopy As Integer = Math.Min(textBytes.Length, 32)
            Array.Copy(textBytes, bytes, lengthToCopy)
        End If
        writer.Write(bytes)
    End Sub
End Class