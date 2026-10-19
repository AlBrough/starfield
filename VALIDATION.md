# Validation record

Checked 19/10/2026 on Windows x64.

## Observed

- The .NET Framework compiler built Starfield.scr successfully.
- The settings form was instantiated and displayed through Windows Forms without an exception.
- The renderer produced 20 frames at 1280 x 720 with the default 10,000 stars in 336 ms in an off-screen smoke check. This is not a display frame-rate benchmark.
- The generated starfield image was visually inspected.

## Outstanding before a stable release

- Interactive testing of every control, save/reload, cancel and full screen preview.
- Windows Screen Saver Settings installation and embedded preview.
- Multiple monitors, negative desktop coordinates and mixed DPI.
- Keyboard, mouse and system resume behaviour, including Windows sign-in on resume.
- Long-duration performance and resource usage on target hardware.

This is an initial development version. Compilation and rendering checks do not establish completion of these interactive checks. No stable release has been published.
