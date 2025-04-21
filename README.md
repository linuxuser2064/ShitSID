# ShitSID
A crusty (?) SID emulator written in Visual Basic.

# Features
- Basic waveform emulation
- Duty cycle support on pulse wave
- Special 4-bit duty cycle mode for extra crustyness
- Linear ADSR, adjusted to be SID-like
- ADSR attack always starts at 0 volume
- No SID filter
- "Pitchable" noise
- Channels are mutable
- Statistics window for debugging

# Issues
- Most MoN songs, even PSID's, don't work properly.
- On some Rob Hubbard songs, the ADSR triggers 1 tick too late

# Usage
To load and play a .SID file, press "Load SID file".
To view info about a .SID file, press "View info".

There's also a special statistics window where you can view statistics about the emulated SID.
To open it, press "Open statistics".

There's also a 4-bit duty cycle mode included. To toggle it press "4-bit duty cycle mode".
Tech details: The 4-bit duty mode makes the SID only use the upper 4 bits of the duty cycle, giving 16 levels.

The rest should be pretty self-explainatory.

# Libraries used
- NAudio for audio output
- Highbyte.DotNet6502 for emulating the 6502
