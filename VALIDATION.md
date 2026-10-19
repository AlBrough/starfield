# Validation record

Checked 19/10/2026 on Windows x64.

## Observed

- The .NET Framework compiler built Starfield.scr successfully.
- The settings form was instantiated and displayed through Windows Forms without an exception.
- The renderer produced 20 frames at 1280 x 720 with the default 10,000 stars in 336 ms in an off-screen smoke check. This is not a display frame-rate benchmark.
- The generated starfield image was visually inspected.
- Version 0.1.1: reproduced the settings sidebar collapsing to 121 pixels with the original layout. Replaced the splitter with a fixed-width settings column.
- The layout regression script passed at three window sizes and simulated 100%, 125%, 150% and 200% scaling. It checks sidebar width, control clipping, action visibility and slider/numeric synchronisation through the actual Windows Forms controls.
- The redesigned settings window was rendered and visually inspected. Simulated scaling is not physical mixed-DPI testing.

## Outstanding before a stable release

- Interactive testing of every control, save/reload, cancel and full screen preview.
- Windows Screen Saver Settings installation and embedded preview.
- Multiple monitors, negative desktop coordinates and mixed DPI.
- Keyboard, mouse and system resume behaviour, including Windows sign-in on resume.
- Long-duration performance and resource usage on target hardware.

This is an initial development version. Compilation and rendering checks do not establish completion of these interactive checks. No stable release has been published.
