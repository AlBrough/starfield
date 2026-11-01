# Starfield delivery checklist

Updated 02/11/2026.

## Current update

Version 0.1.9 adds an optional remembrance of Sir Patrick Stewart beneath the stationary bridge readout, including 28/10/2026 and its fixed simulated stardate 41821.918. The live clock, random launch heading, flight effects, optional chime, warp and density controls remain available. No cloud services or new dependencies.

## Recorded passes

- PASS: Windows compiler build.
- PASS: existing motion, input, natural-colour and settings-compatibility checks.
- PASS: layout checks at three sizes and simulated 100%, 125%, 150% and 200% scaling.
- PASS: accelerated whole-cycle checks, fade bounds and continuity, real-renderer thinning and return, density controls and settings compatibility.
- PASS: visual inspection of rendered dense and sparse phases.
- PASS: warp input limits, increasing finite speeds, legacy speed migration, settings round-trip and actual renderer motion at Warp 1, 9 and 9.99.
- PASS: visual inspection of the warp settings UI.
- PASS: effect timing and bounds, disabled-state equivalence, actual rendering of each effect and the combination, UI bindings, preview action and XML compatibility.
- PASS: combined, nebula-only and red-alert render inspection.
- PASS: original chime WAV format and SoundPlayer loading, without playback.
- PASS: actual bridge-panel pixel bounds remain fixed across elapsed times and alert text at preview and Full HD sizes, in PowerShell 7 and Windows PowerShell 5.1; stationary image visually inspected.
- PASS: random heading bounds and diversity, compass wrap, stable session heading, TNG-era date anchors, visible ticks, year rollover and even leap-day progression.
- PASS: remembrance date conversion, bridge dependency, opt-in UI binding, XML persistence, old-settings compatibility and actual rendered dedication; visual inspection passed.

## Next action

- PENDING MEM-01 - Owner: Al. After installing 0.1.9, use existing personal Windows settings and enable Bridge display and Remember Sir Patrick Stewart. Expected immediately: a stationary bottom-left dedication with his name, 28/10/2026 and fixed SIM STARDATE 41821.918 beneath the separate live date. No timed wait, account or network is required. Save settings to retain the option.

## Other outstanding checks

- PENDING NAV-01 - Owner: Al. Using 0.1.9 with Bridge display enabled and Course corrections off, note the heading and date, close the application fully and relaunch. Expected: a random heading, a clock-driven 41xxx stardate during 2026 and a visible date increase after 40 seconds. Random headings may repeat by chance. This earlier manual check remains pending.

- PENDING BRIDGE-01 - Owner: Al. Using 0.1.8, enable Bridge display and watch for 30 seconds. Expected: a fixed bottom-left panel while stars move. This earlier manual check remains pending.

- PENDING FLIGHT-01 - Owner: Al. Using 0.1.7, enable the six visual options, leave Alert sound off and click Preview selected effects. Watch for 20 seconds. Expected: warp engagement, subtle turn and nebula, the stationary bridge panel and an eight-second red border alert, without sound. No account or network is required. This earlier manual check remains pending.

- PENDING WARP-01 - Owner: Al. Using 0.1.6 with flight effects off, compare Warp 1.00, 9.00 and 9.99 at the same FPS. Expected: progressively faster motion with no selectable Warp 10. This earlier manual check remains pending.
- PENDING DENS-01 - Owner: Al. Using 0.1.6 with 10,000 stars, enable Vary star density and set Cycle length to five minutes. Leave full screen preview untouched for five minutes. Expected: roughly 30% density at two and a half minutes and a return by five minutes, without sudden groups or speed changes. This earlier manual check remains pending.
- PENDING: audible chime acceptance if Al chooses to enable sound, live combined-effect performance and preview-to-normal transitions on the target display.

The previously recorded Windows installation/embedded preview, mixed-DPI/multi-monitor, resume/sign-in and long-duration hardware checks remain unverified. Preserve the automated passes above when recording manual results. Version 0.1.2 remains the stable release until a later release is explicitly promoted; 0.1.9 is a preview for this change.
