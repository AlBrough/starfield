# Starfield delivery checklist

Updated 02/11/2026.

## Current update

Version 0.1.3 adds optional star-density cycling with a 12-minute default and a 10 - 15 minute control. The selected star count is the upper limit; the sparse phase is roughly 30%. No cloud services or new dependencies.

## Recorded passes

- PASS: Windows compiler build.
- PASS: existing motion, input, natural-colour and settings-compatibility checks.
- PASS: layout checks at three sizes and simulated 100%, 125%, 150% and 200% scaling.
- PASS: accelerated whole-cycle checks, fade bounds and continuity, real-renderer thinning and return, density controls and settings compatibility.
- PASS: visual inspection of rendered dense and sparse phases.

## Next action

- PENDING DENS-01 - Owner: Al. After installing 0.1.3, use the existing personal Windows settings with 10,000 stars, Vary star density enabled and Cycle length set to 12 minutes. Run the full screen preview without interacting for 12 minutes. Expected: a gradual decrease to roughly 30% density around six minutes, then a gradual return by 12 minutes, without abrupt groups appearing or changes to speed. Report whether the variation and transition feel right. No account or network connection is required.

## Other outstanding checks

The previously recorded Windows installation/embedded preview, mixed-DPI/multi-monitor, resume/sign-in and long-duration hardware checks remain unverified. Preserve the automated passes above when recording manual results. Version 0.1.2 remains the stable release until a later release is explicitly promoted; 0.1.3 is a preview for this change.
