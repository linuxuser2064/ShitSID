# ShitSID
A crusty (?) SID emulator written in Visual Basic.

# Features
- Basic waveform emulation
- Partial mixed waveform support
- Duty cycle support on pulse wave
- Special 4-bit duty cycle mode for extra crustyness
- SID exponential ADSR (code conversion from reSIDfp)
- Half-ass filter emulationm with switchable 8580 and 6581 modes
- Half-ass filter distortion emulation
- "Pitchable" noise
- Channels are mutable
- Statistics window for debugging
- $D418 sample support

# Issues
- On some songs the filter does not work (destabilizes because of bad emulation).
- On some other songs there appears to be memory or stack corruption, and crash after a bit.

# Usage
To load and play a .SID file, press "Load SID file".
To view info about a .SID file, press "View info".

There's also a special statistics window where you can view statistics about the emulated SID.
To open it, press "Open statistics".

There's also a 4-bit duty cycle mode included. To toggle it press "4-bit duty cycle mode".
Tech details: The 4-bit duty mode makes the SID only use the upper 4 bits of the duty cycle, giving 16 levels.

### Filter controls
- Cutoff multiplier: The amount of cutoff variance that the filter has.
- Cutoff bias: the offset that the cutoff has, in Hz.
- Resonance divider: The amount by which the resonance is divided.
- 6581 filter mode: Enables the experimental SID 6581-like filter.
- Disable filter: Disables the SID filter entirely.

The rest should be pretty self-explainatory.

# Libraries used/credits
- NAudio for audio output
- Highbyte.DotNet6502 for emulating the 6502
- Credits to the reSIDfp team for the envelope generator code
