Imports System.CodeDom

Public Class Filter6581
    Inherits Filter
    Public Property Mode6581 As Boolean = True
    Public Sub New()
        MyBase.New()
        ReDim type3_w0s(1023)

        For i As Integer = 0 To 1023
            ReDim type3_w0s(i)(255)
        Next
        setCurveAndDistortionDefaults()
    End Sub
    Private attenuation, nonlinearity As Single
    Private baseresistance, offset, steepness, minimumfetresistance, voiceNonlinearity As Single
    Private Shared SIDCAPS_6581 As Double = 0.00000000047
    Private Shared OSC_TO_FC As Single = 1 / 512.0F

    '/*
    '* 1024 because real chip has disconnected line #0. 128 seems to suffice for the
    '* size of approximation: the bound Is exceeded only during most extreme
    '* distortion.
    '*/
    Private type3_w0s()() As Single
    Private type3_w0() As Single
    '/*
    '* succeeding values differ by about 1 % at this resolution, so that feels
    '* suitable level of precision. Previously I worked with fastexp() (linear
    '* approximation of exp()) that had up to 3 % error.
    '*/
    Private Shared TYPE3_W0S_RESOLUTION As Single = 1 / 5000.0F

    Private Function Ftype3_w0(dist As Single)
        If dist < 0 Then
            Return type3_w0(0)
        End If
        Dim index As Integer = dist * TYPE3_W0S_RESOLUTION
        Return type3_w0(If(index < 256, index, 255))
    End Function

    Private Function waveshaper1(value As Single)
        Dim newvalue As Single = value ' to be safe
        If value > nonlinearity Then
            newvalue -= (value - nonlinearity) * 0.5F
        End If
        Return newvalue
    End Function

    Public Function estimateCurrentDistortion() As Single
        Return Ftype3_w0(Vhp) + Ftype3_w0(Vbp) - 2.0F * Ftype3_w0(0)
    End Function
    Public Overrides Function Clock(v1 As Single, v2 As Single, v3 As Single, vE As Single) As Single
        Dim Vi As Single = 0
        Dim Vf As Single = 0

        If filt1 Then
            Vi += v1
        Else
            Vf += v1
        End If
        If filt2 Then
            Vi += v2
        Else
            Vf += v2
        End If
        ' NB! Voice 3 is not silenced by voice3off if it is routed through
        ' the filter.
        If filt3 Then
            Vi += v3
        ElseIf voice3off Then
            Vf += v3
        End If
        If filtE Then
            Vi += vE
        Else
            Vf += vE
        End If

        Vlp -= Vbp * Ftype3_w0(Vbp)
        Vbp -= Vhp * Ftype3_w0(Vhp)
        Vhp = (Vbp * _1_div_Q - Vlp - Vi) * attenuation

        If lp Then
            Vf += Vlp
        End If
        If bp Then
            Vf += Vbp
        End If
        If hp Then
            Vf += Vhp
        End If

        Return waveshaper1(Vf)
    End Function

    Public Sub setNonLinearity(nl As Single)
        voiceNonlinearity = nl
        recalculate()
        updatedCenterFrequency()
    End Sub

    Public Overrides Sub setCurveAndDistortionDefaults()
        setDistortionProperties(0.64F, 3300000.0F, 1.0F)
        setCurveProperties(1147036.5F, 2.742288E+8F, 1.00666344F, 16125.1553F)
        setNonLinearity(0.961316049F)
    End Sub
    Public Overrides Sub setClockFrequency(clock As Double)
        MyBase.setClockFrequency(clock)
        recalculate()
        updatedCenterFrequency()
    End Sub
    Public Overrides Function getDistortionProperties() As Single()
        Return {attenuation, nonlinearity, resonanceFactor}
    End Function
    Public Overrides Sub setDistortionProperties(a As Single, b As Single, c As Single)
        attenuation = a
        nonlinearity = b
        resonanceFactor = c
        updatedResonance()
    End Sub
    Public Overrides Function getCurveProperties() As Single()
        Return {baseresistance, offset, steepness, minimumfetresistance}
    End Function

    Public Overrides Sub setCurveProperties(br As Single, o As Single, s As Single, mfr As Single)
        baseresistance = br
        offset = o
        steepness = s
        minimumfetresistance = mfr
        recalculate()
        updatedCenterFrequency()
    End Sub
    Public Overrides Sub updatedCenterFrequency()
        type3_w0 = type3_w0s(fc >> 1)
    End Sub
    Private Sub recalculate() ' very scary
        Dim fcBase(1023) As Single
        For j = 0 To fcBase.Length - 1
            Dim type3_fc_kink As Single = kinkedDac(j << 1, voiceNonlinearity, 11)
            fcBase(j) = offset / MathF.Pow(steepness, type3_fc_kink) ' thank .net for MathF
        Next

        Dim distBase(255) As Single
        For i = 0 To distBase.Length - 1
            Dim dist As Single = If(i > 0, (i + 0.5F) / TYPE3_W0S_RESOLUTION, 0)
            distBase(i) = 1.0F / MathF.Pow(steepness, dist * OSC_TO_FC)
        Next

        Dim _1_div_caps_freq As Single = 1 / (SIDCAPS_6581 * clockFrequency)

        For j = 0 To fcBase.Length - 1
            Dim fcBaseValue As Single = fcBase(j)
            For i = 0 To distBase.Length - 1 ' holy recursive loop
                Dim fetresistance As Single = fcBaseValue * distBase(i)
                Dim dynamic_resistance As Single = minimumfetresistance + fetresistance

                ' /* 2 parallel resistors */
                Dim _1_div_resistance As Single = (baseresistance + dynamic_resistance) /
                    (baseresistance * dynamic_resistance)
                type3_w0s(j)(i) = _1_div_caps_freq * _1_div_resistance
            Next
        Next
    End Sub
    Public Overrides Sub updatedResonance()
        _1_div_Q = 1.0F / (0.5F + resonanceFactor * res / 18.0F)
    End Sub

    ' this is from SID class
    Public Shared Function kinkedDac(input As Integer, nonLinearity As Single, maxBit As Integer) As Single
        Dim value As Single = 0F
        Dim currentBit As Integer = 1
        Dim weight As Single = 1.0F
        Dim dir As Single = 2.0F * nonLinearity
        For i = 0 To maxBit - 1
            If (input And currentBit) > 0 Then
                value += weight
            End If
            currentBit <<= 1
            weight *= dir
        Next
        Return value / (weight / nonLinearity / nonLinearity) * (1 << maxBit)
    End Function

    ' partial port from FilterModelConfig class
    Public Function estimateFrequency() As Double
        Dim ik As Double = kinkedDac(fc, voiceNonlinearity, 11)
        Dim dynamic As Double = minimumfetresistance + offset / Math.Pow(steepness, ik)
        Dim R As Double = baseresistance * dynamic / (baseresistance + dynamic)
        Return 1 / (2 * Math.PI * SIDCAPS_6581 * R)
    End Function
End Class
