# Starfield delivery checklist

Updated 02/11/2026.

## Current update

Version 0.1.5 adds a TNG-inspired Warp factor control from 1.00 - 9.99, with an artistic visual mapping and legacy speed migration. Density cycling remains 5 - 15 minutes, default 12 minutes, with a roughly 30% minimum. No cloud services or new dependencies.

## Recorded passes

- PASS: Windows compiler build.
- PASS: existing motion, input, natural-colour and settings-compatibility checks.
- PASS: layout checks at three sizes and simulated 100%, 125%, 150% and 200% scaling.
- PASS: accelerated whole-cycle checks, fade bounds and continuity, real-renderer thinning and return, density controls and settings compatibility.
- PASS: visual inspection of rendered dense and sparse phases.
- PASS: warp input limits, increasing finite speeds, legacy speed migration, settings round-trip and actual renderer motion at Warp 1, 9 and 9.99.
- PASS: visual inspection of the warp settings UI.

## Next action

- PENDING WARP-01 - Owner: Al. After installing 0.1.5, use existing personal Windows settings and compare Warp 1.00, 9.00 and 9.99 in the live preview. Expected: progressively faster motion, a steeper increase above 9 and no selectable Warp 10. Keep the existing FPS setting for this comparison. No account, network or timed wait is required. Report whether the upper range feels right.

## Other outstanding checks

- PENDING DENS-01 - Owner: Al. Using 0.1.5 with 10,000 stars, enable Vary star density and set Cycle length to five minutes. Leave full screen preview untouched for five minutes. Expected: roughly 30% density at two and a half minutes and a return by five minutes, without sudden groups or speed changes. This earlier manual check remains pending.

The previously recorded Windows installation/embedded preview, mixed-DPI/multi-monitor, resume/sign-in and long-duration hardware checks remain unverified. Preserve the automated passes above when recording manual results. Version 0.1.2 remains the stable release until a later release is explicitly promoted; 0.1.5 is a preview for this change.
