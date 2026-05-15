Public MustInherit Class Filter

	'/**
	' * Filter enabled.
	' */
	Private enabled As Boolean = True

	'/**
	' * Filter cutoff frequency.
	' */
	Protected fc As Integer

	'/**
	' * Filter resonance.
	' */
	Protected res As Integer

	'/**
	' * Selects which inputs to route through filter.
	' */
	Private filt As Byte
	Protected filt1, filt2, filt3, filtE As Boolean

	'/**
	' * Switch voice 3 off.
	' */
	Protected voice3off As Boolean

	'/**
	' * Highpass, bandpass, And lowpass filter modes.
	' */
	Protected hp, bp, lp As Boolean

	Protected vol As Single '/* To avoid Integer-To-float conversion at output */

	Protected clockFrequency As Double

	Protected Vhp, Vbp, Vlp As Single

	'/* Resonance/Distortion/Type3/Type4 helpers. */
	Protected _1_div_Q As Single

	Protected resonanceFactor As Single

	Public MustOverride Function Clock(v1 As Single, v2 As Single, v3 As Single, vE As Single) As Single

	Protected Sub zeroDenormals()
		If Vbp > -1.0E-12F And Vbp < 1.0E-12F Then
			Vbp = 0
		End If
		If Vlp > -1.0E-12F And Vlp < 1.0E-12F Then
			Vlp = 0
		End If
	End Sub
	Public MustOverride Sub setCurveAndDistortionDefaults()
	Public Sub enable(enable As Boolean)
		If (Not enabled) And enable Then
			enabled = enable
			writeRES_FILT(filt)
		ElseIf enabled And (Not enable) Then
			enabled = enable
			filt1 = filt2 = filt3 = filtE = False
		End If
	End Sub
	Public Overridable Sub setClockFrequency(clock As Double)
		clockFrequency = clock
		updatedCenterFrequency()
	End Sub
	Public MustOverride Function getCurveProperties() As Single()
	Public MustOverride Sub setCurveProperties(a As Single, b As Single, c As Single, d As Single)
	Public MustOverride Function getDistortionProperties() As Single()
	Public MustOverride Sub setDistortionProperties(a As Single, b As Single, c As Single)
	Public Sub reset()
		Vhp = Vbp = Vlp = 0
		writeFC_LO(CByte(0))
		writeFC_HI(CByte(0))
		writeMODE_VOL(CByte(0))
		writeRES_FILT(CByte(0))
	End Sub
	Public Sub writeFC_LO(fc_lo As Byte)
		fc = (fc And &H7F8) Or (fc_lo And &H7)
		updatedCenterFrequency()
	End Sub
	Public Sub writeFC_HI(fc_hi As Byte)
		fc = ((CInt(fc_hi) << 3) And &H7F8) Or (fc And &H7)
		updatedCenterFrequency()
	End Sub
	Public Sub writeRES_FILT(res_filt As Byte)
		filt = res_filt

		res = res_filt >> 4 And &HF
		updatedResonance()

		If enabled Then
			filt1 = (filt And 1) > 0
			filt2 = (filt And 2) > 0
			filt3 = (filt And 4) > 0
			filtE = (filt And 8) > 0
		End If
	End Sub
	Public Sub writeMODE_VOL(mode_vol As Byte)
		vol = (mode_vol And &HF) / 15.0F
		lp = (mode_vol And &H10) > 0
		bp = (mode_vol And &H20) > 0
		hp = (mode_vol And &H40) > 0
		voice3off = (mode_vol And &H80) = 0
	End Sub
	Public MustOverride Sub updatedResonance()
	Public MustOverride Sub updatedCenterFrequency()

End Class
