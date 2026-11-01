# Starfield delivery checklist

Updated 02/11/2026.

## Current update

Version 0.1.10 removes the memorial and SIM prefix, retaining only the advancing calculated STARDATE. Nebula clouds use a small opaque cache and native Windows drawing to reduce full-screen transparency cost. The random heading, flight effects, chime, warp and density controls remain available. No cloud services or new dependencies.

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
- Historical PASS (0.1.9, feature removed in 0.1.10): remembrance date conversion, bridge dependency, opt-in UI binding, XML persistence, old-settings compatibility and rendered dedication.
- PASS: nebula cache bounds/reuse, refresh, resizing, colour changes, preview time reset, disable state and saved settings containing the removed memorial preference.
- PASS: same-machine isolated nebula benchmark improved from 39.818 to 7.585 ms/frame at Full HD and 159.215 to 41.560 ms/frame at 4K; not a whole-app or target-device FPS claim.
- PASS: visual comparison of cloud rendering and inspection of the cleaned STARDATE readout.

## Next action

- PENDING PERF-01 - Owner: Al. After installing 0.1.10, use the same personal Windows star count, warp and FPS settings that showed the slowdown. Enable Nebula passages and Bridge display, leave sound off and run Full screen preview for two minutes. Expected: STARDATE without SIM or a memorial, and less slowdown when the nebula fades in after 90 seconds. Report whether the cloud passage is smooth enough on the device. No account or network is required.
- WITHDRAWN MEM-01 - Memorial removed by user request on 02/11/2026. No further test is required for that feature.

## Other outstanding checks

- PENDING NAV-01 - Owner: Al. Using 0.1.9 with Bridge display enabled and Course corrections off, note the heading and date, close the application fully and relaunch. Expected: a random heading, a clock-driven 41xxx stardate during 2026 and a visible date increase after 40 seconds. Random headings may repeat by chance. This earlier manual check remains pending.

- PENDING BRIDGE-01 - Owner: Al. Using 0.1.8, enable Bridge display and watch for 30 seconds. Expected: a fixed bottom-left panel while stars move. This earlier manual check remains pending.

- PENDING FLIGHT-01 - Owner: Al. Using 0.1.7, enable the six visual options, leave Alert sound off and click Preview selected effects. Watch for 20 seconds. Expected: warp engagement, subtle turn and nebula, the stationary bridge panel and an eight-second red border alert, without sound. No account or network is required. This earlier manual check remains pending.

- PENDING WARP-01 - Owner: Al. Using 0.1.6 with flight effects off, compare Warp 1.00, 9.00 and 9.99 at the same FPS. Expected: progressively faster motion with no selectable Warp 10. This earlier manual check remains pending.
- PENDING DENS-01 - Owner: Al. Using 0.1.6 with 10,000 stars, enable Vary star density and set Cycle length to five minutes. Leave full screen preview untouched for five minutes. Expected: roughly 30% density at two and a half minutes and a return by five minutes, without sudden groups or speed changes. This earlier manual check remains pending.
- PENDING: audible chime acceptance if Al chooses to enable sound, live combined-effect performance and preview-to-normal transitions on the target display.

The previously recorded Windows installation/embedded preview, mixed-DPI/multi-monitor, resume/sign-in and long-duration hardware checks remain unverified. Preserve the automated passes above when recording manual results. Version 0.1.2 remains the stable release until a later release is explicitly promoted; 0.1.10 is a preview for this change.
