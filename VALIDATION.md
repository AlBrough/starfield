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

## Expanded period range - 02/11/2026

- Version 0.1.4 extends the selectable full-cycle range to 5 - 15 minutes. Default remains 12 minutes and minimum density remains roughly 30%.
- Synthetic-clock checks pass at 5, 10, 12 and 15 minutes. The continuity bound accounts for the faster five-minute cycle.
- UI binding and XML round-trip now explicitly verify five minutes. Existing behaviour and layout checks pass.
- DENS-01 remains pending and now uses the five-minute setting in CHECKLIST.md to verify the expanded range. No previous automated passes have been removed.

## Warp control - 02/11/2026

- Version 0.1.5 replaces the speed UI with a TNG-inspired factor from 1.00 - 9.99 and an explicit visual-scaling note. No official physical velocity is claimed.
- Warp checks pass for finite, increasing motion across every 0.01 step, representative legacy speeds from 1 - 200, old XML migration, the 9.99 input limit and settings round-trip.
- Real-renderer star-depth changes match the selected warp factor at 1, 9 and 9.99 with the legacy Speed field unchanged.
- Existing behaviour, density and layout checks pass. The settings window was rendered and visually inspected.
- WARP-01 remains a manual acceptance check in CHECKLIST.md. Smoothness at high warp on the user's display is not established by these automated results.

## Optional flight effects - 02/11/2026

- Version 0.1.6 adds six independent visual effects, a separate opt-in alert chime and a 20-second preview of selected events. All new options default off.
- Automated checks pass for each effect's sample timing, bounds over ten minutes of synthetic time and unchanged flight state when disabled.
- Actual-renderer checks exercise each visual option independently and all together. Combined, nebula-only and red-alert images were visually inspected.
- Checkbox bindings, preview-button wiring, XML round-trip and old settings with all new effects disabled pass.
- The original chime's WAV structure and SoundPlayer loading pass without playing audio. Audible output, volume and device behaviour remain unverified.
- The previous warp, density, behaviour and simulated layout checks pass.
- Real-time perception, combined-effect performance at target resolution, preview-to-normal transitions and audible chime acceptance remain manual checks. FLIGHT-01 in CHECKLIST.md is the next user action.

## Stationary bridge panel - 02/11/2026

- Version 0.1.7 anchors the bridge readout at the bottom left, reserves fixed dimensions for alert text and replaces HDG with HEADING (simulated direction of travel).
- Actual rendered pixel bounds remain unchanged at 0, 60, 300 and 900 seconds, including alert text, at 960 x 600 and 1920 x 1080. The check passes in PowerShell 7 and Windows PowerShell 5.1.
- The stationary panel image was visually inspected. Existing effects, warp, density, behaviour and simulated layout checks pass.
- BRIDGE-01 is the next manual display check; previous passes and pending tests remain recorded in CHECKLIST.md.

## Navigation and calendar - 02/11/2026

- Version 0.1.8 randomises the launch heading once per application process, shared across monitor windows, and wraps course corrections into the compass range.
- SIM STARDATE now uses the computer's local clock and the documented +338-year approximation: 2364 starts at 41000, with 1000 units per local year. It is independent of preview/session elapsed time.
- Navigation checks pass for random heading range and diversity, stable session heading, compass wrap and rounding, date anchors, visible ticks over 40 seconds, year rollover and even leap-day progression.
- The bridge pixel-bounds check passes with the additional stardate digit. Existing effects, warp, density, behaviour and simulated layout checks pass.
- NAV-01 in CHECKLIST.md is the next manual test. No exact canonical stardate conversion is claimed.

## Optional remembrance - 02/11/2026

- Version 0.1.9 adds Remember Sir Patrick Stewart as an opt-in bridge dedication with 28/10/2026 and simulated stardate 41821.918. The latter uses the start of the date, not a claimed time of death.
- The death date was checked against contemporary reporting, with the AP obituary and official StarTrek.com tribute linked in README.md. No photographs, recordings or copied tribute prose are included.
- Tests pass for date conversion, bridge-display dependency, checkbox binding, XML persistence, older settings opting out and actual rendering. The rendered dedication was visually inspected.
- Existing navigation, bridge bounds, effects, warp, density, behaviour and simulated layout checks pass.
- MEM-01 in CHECKLIST.md is the next manual acceptance check. Earlier recorded passes and pending checks remain preserved.

## Stardate cleanup and nebula performance - 02/11/2026

- Version 0.1.10 removes the memorial text, setting and UI control at the user's request. The obsolete MEM-01 test is withdrawn. Older XML with PatrickTribute still loads the other settings correctly.
- The live readout now says STARDATE, retaining the existing calculated calendar and stationary panel.
- An isolated benchmark reproduced expensive full-screen GDI+ colour-matrix/transparency work. A small opaque cache with native Windows stretching reduces that work. Cloud compositing is capped at 12 Hz; stars retain their configured render timing.

| Isolated background, median ms/frame | Before (0.1.9) | After (0.1.10) |
| --- | --- | --- |
| 1920 x 1080 | 39.818 | 7.585 |
| 3840 x 2160 | 159.215 | 41.560 |

These are same-machine off-screen measurements, including a canvas clear, from three warm 30-frame passes after warm-up. They isolate the background and do not establish whole-app FPS or the result on Al's device. measure-nebula.ps1 is the reproducible harness. Initial cache generation and hardware-specific behaviour are not established by these medians.

- Cache size/reuse, periodic refresh, viewport resizing, custom background colour, preview time reset, disable state and removed-setting compatibility checks pass.
- Navigation, bridge bounds, independent effects, warp, density, existing behaviour and simulated layout checks pass.
- Before/after cloud images and the combined scene were visually inspected; the readout no longer includes SIM or memorial text.
- PERF-01 in CHECKLIST.md remains the next device acceptance check.
