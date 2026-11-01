# Starfield delivery checklist

Updated 02/11/2026.

## Current update

Version 0.1.11 adds installed version metadata, a manual GitHub update checker and an Include previews preference. Newer-release links open in the browser for manual download. No background checks or automatic installation. Existing visual and performance changes remain included. No new third-party dependencies.

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
- PASS: compiled Framework update-check fixtures covering numeric/channel selection, safe URLs, pagination, HTTP/JSON/size failures, a real stalled-body timeout, installed metadata and Settings lifecycle/duplicate-click behaviour.
- PASS: update status links remain visible at independently simulated 100%, 125%, 150% and 200% scales; Settings header visually inspected.
- PASS: compiled checker reached the unauthenticated live repository API before publication and found 0.1.10 (preview).

## Next action

- PENDING UPD-01 - Owner: Al. After manually installing 0.1.11 once, open Settings with the existing personal preferences. Confirm Version 0.1.11 (preview), leave Include previews selected and click Check for updates. Expected within 15 seconds: No newer release found while 0.1.11 is the newest release, or a link if a later one exists. Settings should stay responsive. Requires internet access, no GitHub login or timed wait beyond the request.
- WITHDRAWN MEM-01 - Memorial removed by user request on 02/11/2026. No further test is required for that feature.

## Other outstanding checks

- PENDING PERF-01 - Owner: Al. Using 0.1.11, keep the star count, warp and FPS settings that showed slowdown. Enable Nebula passages and Bridge display, leave sound off and run Full screen preview for two minutes. Expected: less slowdown when clouds appear after 90 seconds. This earlier device check remains pending.

- PENDING NAV-01 - Owner: Al. Using 0.1.9 with Bridge display enabled and Course corrections off, note the heading and date, close the application fully and relaunch. Expected: a random heading, a clock-driven 41xxx stardate during 2026 and a visible date increase after 40 seconds. Random headings may repeat by chance. This earlier manual check remains pending.

- PENDING BRIDGE-01 - Owner: Al. Using 0.1.8, enable Bridge display and watch for 30 seconds. Expected: a fixed bottom-left panel while stars move. This earlier manual check remains pending.

- PENDING FLIGHT-01 - Owner: Al. Using 0.1.7, enable the six visual options, leave Alert sound off and click Preview selected effects. Watch for 20 seconds. Expected: warp engagement, subtle turn and nebula, the stationary bridge panel and an eight-second red border alert, without sound. No account or network is required. This earlier manual check remains pending.

- PENDING WARP-01 - Owner: Al. Using 0.1.6 with flight effects off, compare Warp 1.00, 9.00 and 9.99 at the same FPS. Expected: progressively faster motion with no selectable Warp 10. This earlier manual check remains pending.
- PENDING DENS-01 - Owner: Al. Using 0.1.6 with 10,000 stars, enable Vary star density and set Cycle length to five minutes. Leave full screen preview untouched for five minutes. Expected: roughly 30% density at two and a half minutes and a return by five minutes, without sudden groups or speed changes. This earlier manual check remains pending.
- PENDING: audible chime acceptance if Al chooses to enable sound, live combined-effect performance and preview-to-normal transitions on the target display.

The previously recorded Windows installation/embedded preview, mixed-DPI/multi-monitor, resume/sign-in and long-duration hardware checks remain unverified. Preserve the automated passes above when recording manual results. Version 0.1.2 remains the stable release until a later release is explicitly promoted; 0.1.11 is a preview for this change.
