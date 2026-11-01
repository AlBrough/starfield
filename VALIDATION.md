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
- Version 0.1.2: reproduced a twofold difference in old trail lengths using 12 ms and 24 ms frame intervals at the same speed and stretch.
- Motion checks pass at 30, 50, 60, 90, 120 and 144 FPS, including uneven intervals and a consistent trail exposure.
- Modern input checks pass for direct typing, invalid-text recovery, keyboard increments, slider synchronisation and selecting 50 FPS.
- Natural-colour toggling, palette diversity, XML round-trip and old settings without the new preference pass.
- The updated settings window and natural-colour preview were rendered and visually inspected. Layout checks pass at all previously recorded simulated scales.
- An off-screen rendering smoke check with antialiasing produced 20 frames at 1280 x 720 in 497 ms with 10,000 stars. This is not a live frame-pacing measurement. A perfectly smooth 50 or 60 FPS is not established.
- User reported 30 FPS removed the visible jitter in version 0.1.1 on their display. The new timer pacing and antialiasing still require user confirmation.

## Outstanding interactive checks

- Interactive testing of every control, save/reload, cancel and full screen preview.
- Windows Screen Saver Settings installation and embedded preview.
- Multiple monitors, negative desktop coordinates and mixed DPI.
- Keyboard, mouse and system resume behaviour, including Windows sign-in on resume.
- Long-duration performance and resource usage on target hardware.

Version 0.1.2 was promoted to a stable GitHub release at the owner's explicit request on 19/10/2026. That promotion did not establish completion of these interactive checks.

## Density-cycle update - 02/11/2026

- Full 10-, 12- and 15-minute cycles were checked using a synthetic clock, including endpoints, every whole second, bounds, continuity and repeated periods.
- Per-star opacity remains bounded and follows the requested density with a gradual fade band. Disabling the cycle returns constant full density.
- The real renderer was exercised at dense, sparse and returned phases. Image-energy checks confirm thinning and recovery without replacing the star array. Dense and sparse images were visually inspected.
- Cycle toggle and period controls, XML round-trip and older settings without the new fields pass.
- Existing behaviour and simulated layout regression checks pass.
- A real-time 12-minute cycle on the user's display remains pending as DENS-01 in CHECKLIST.md. Automated clock checks do not establish perceived smoothness or long-run reliability on that display.
