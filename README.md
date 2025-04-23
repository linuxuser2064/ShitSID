# ShitSID
A crusty (?) SID emulator written in Visual Basic.

# Features
- Basic waveform emulation
- Duty cycle support on pulse wave
- Special 4-bit duty cycle mode for extra crustyness
- SID-like exponential ADSR
- Filter emulation with broken highpass
- "Pitchable" noise
- Channels are mutable
- Statistics window for debugging

# Issues
- On older Rob Hubbard songs, the ADSR triggers 1-2 frames too late.
- On some songs, the pitch gets set 1 frame too late.

# Usage
To load and play a .SID file, press "Load SID file".
To view info about a .SID file, press "View info".

There's also a special statistics window where you can view statistics about the emulated SID.
To open it, press "Open statistics".

There's also a 4-bit duty cycle mode included. To toggle it press "4-bit duty cycle mode".
Tech details: The 4-bit duty mode makes the SID only use the upper 4 bits of the duty cycle, giving 16 levels.

### Filter controls
- Cutoff multiplier: The amount of cutoff variance that the filter has.
- Cutoff bias: the offset that the cutoff has, in hz.
- Resonance divider: The amount by which the resonance is divided.
- 6581 filter mode: Enables the experimental SID 6581-like filter

The rest should be pretty self-explainatory.

# Libraries used
- NAudio for audio output
- Highbyte.DotNet6502 for emulating the 6502
