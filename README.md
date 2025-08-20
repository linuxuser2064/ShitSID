# ShitSID
A crusty (?) SID emulator written in Visual Basic.

# Features
- Basic waveform emulation
- Partial mixed waveform support
- Duty cycle support on pulse wave
- Special 4-bit duty cycle mode for extra crustiness
- SID pseudo-exponential ADSR (code conversion from reSIDfp)
- Half-ass filter emulation with switchable 8580 and 6581 modes
- Half-ass filter distortion emulation
- Half-ass RSID support
- "Pitchable" noise
- Channels are mute-able
- Statistics window for debugging
- $D418 sample support
- PAL/NTSC timing support

# Issues
- Chris Hülsbeck's songs that use samples do not play.
- Martin Galway's songs that use sampels do not play.

# Usage
To load and play a .SID file, press "Load SID file".
To view info about a .SID file, press "View info".

There's also a special statistics window where you can view statistics about the emulated SID.
To open it, press "Open statistics".

There's also a 4-bit duty cycle mode included. To toggle it press "4-bit duty cycle mode".
Tech details: The 4-bit duty mode makes the SID only use the upper 4 bits of the duty cycle, giving 16 levels.

### Filter controls
- Cutoff bias: the offset that the cutoff has, in Hz.
- Resonance divider: The amount by which the resonance is divided.
- 6581 filter mode: Enables the experimental SID 6581-like filter.
- Disable filter: Disables the SID filter entirely.

### Master volume register controls
- Sample mode: Makes the master volume register output as a seperate channel. (good for sampled songs)
- Volume mode: Makes the master volume register affect the volume also. (good for songs that use the master volume)

The rest should be pretty self-explainatory.

# Libraries used/credits
- NAudio for audio output
- Highbyte.DotNet6502 for emulating the 6502
- Credits to the reSIDfp team for the envelope generator code
