Imports System.Buffers.Binary
Imports System.IO

Public Class SidFile
    Public Property LoadAddress As UShort
    Public Property InitAddress As UShort
    Public Property PlayAddress As UShort
    Public Property Songs As UShort
    Public Property StartSong As UShort
    Public Property Speed As UInteger
    Public Property Flags As UInteger
    Public Property StartPage As Byte
    Public Property PageLength As Byte
    Public Property SecondSIDAddress As Byte
    Public Property ThirdSIDAddress As Byte
    Public Property SongName As String
    Public Property SongArtist As String
    Public Property SongStudio As String
    Public Property Data As Byte()

    Public Shared Function Load(path As String) As SidFile
        Dim raw = File.ReadAllBytes(path)

        ' Check if it's a valid PSID file
        Dim id = System.Text.Encoding.ASCII.GetString(raw, 0, 4)
        If id <> "PSID" AndAlso id <> "RSID" Then
            'Throw New Exception("Not a PSID file")
        End If

        ' Read Version (we'll assume this is PSIDv2 for now)
        Dim version = BinaryPrimitives.ReadUInt16BigEndian(raw.AsSpan(4, 2)) ' Big endian: byte 5 is more significant

        ' PSIDv2 header parsing (all numbers are in big-endian)
        Dim dataOffset = raw(7) ' fucking hack
        Dim loadAddress = BinaryPrimitives.ReadUInt16BigEndian(raw.AsSpan(8, 2)) ' Big endian: bytes 8 and 9
        Dim initAddress = BinaryPrimitives.ReadUInt16BigEndian(raw.AsSpan(10, 2)) ' Big endian: bytes 10 and 11
        Dim playAddress = BinaryPrimitives.ReadUInt16BigEndian(raw.AsSpan(12, 2)) ' Big endian: bytes 12 and 13
        Dim songs = BinaryPrimitives.ReadUInt16BigEndian(raw.AsSpan(14, 2)) ' Big endian: bytes 14 and 15
        Dim startSong = BinaryPrimitives.ReadUInt16BigEndian(raw.AsSpan(16, 2)) ' Big endian: bytes 16 and 17
        Dim speed = BinaryPrimitives.ReadUInt32BigEndian(raw.AsSpan(18, 4)) ' Big endian: bytes 18 to 21
        Dim songName = Text.Encoding.ASCII.GetString(raw.AsSpan(22, 32)).Trim(vbNullChar)
        Dim artistName = Text.Encoding.ASCII.GetString(raw.AsSpan(54, 32)).Trim(vbNullChar)
        Dim studioName = Text.Encoding.ASCII.GetString(raw.AsSpan(86, 32)).Trim(vbNullChar)
        ' Additional fields based on PSIDv2 header
        Dim flags = BinaryPrimitives.ReadUInt32BigEndian(raw.AsSpan(76, 4)) ' Big endian: bytes 76 to 79
        Dim startPage = raw(78)
        Dim pageLength = raw(79)
        Dim secondSIDAddress = raw(80)
        Dim thirdSIDAddress = raw(81)

        ' Extract program data (starts from dataOffset)
        Dim programData = raw.Skip(dataOffset).ToArray()

        ' If LoadAddress = 0, take first 2 bytes of data as real load address
        If loadAddress = 0 Then
            loadAddress = BitConverter.ToUInt16(raw, dataOffset) ' sid file is weird
            programData = programData.Skip(2).ToArray()
        End If

        ' If initAddress = 0, fallback to load address
        If initAddress = 0 Then initAddress = loadAddress

        ' Create and return SidFile object
        Return New SidFile With {
            .LoadAddress = loadAddress,
            .InitAddress = initAddress,
            .PlayAddress = playAddress,
            .Songs = songs,
            .StartSong = startSong,
            .Speed = speed,
            .Flags = flags,
            .StartPage = startPage,
            .PageLength = pageLength,
            .SecondSIDAddress = secondSIDAddress,
            .ThirdSIDAddress = thirdSIDAddress,
            .Data = programData,
            .SongName = songName,
            .SongArtist = artistName,
            .SongStudio = studioName
        }
    End Function
End Class
