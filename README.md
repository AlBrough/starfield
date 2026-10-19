# Starfield

A standalone Windows starfield screensaver with a live settings preview, adjustable star count, speed, size, motion stretch, colours, chromatic shift and frame rate. Supports a single desktop, individual monitors or the current mouse position as the centre.

## Build

Run `powershell -ExecutionPolicy Bypass -File .\build.ps1` on Windows with .NET Framework 4.8. The built file is `dist\Starfield.scr`. No packages or administrator access are needed to build.

## Use

Right-click the built `.scr` and select **Configure** to adjust settings or **Test** to run it. Select **Install** to open Windows Screen Saver Settings and choose it. Keep the file in a permanent local folder before installing. Set the wait period and enable the Windows sign-in requirement in Windows Screen Saver Settings if wanted.

Launching without arguments opens settings. Windows may pass `/s` when double-clicking a `.scr` file. Run with `/c` to configure, `/s` to start or `/p HWND` for the embedded Windows preview. Full screen mode exits on keyboard input, a mouse click or mouse movement after a short grace period. Settings are stored per user in `%LOCALAPPDATA%\Brough\Starfield\settings.xml`.

The FPS setting is a target ceiling. Actual rendering speed depends on the computer and star count. This version uses Windows GDI+ software rendering. It does not provide a security lock; Windows manages sign-in on resume.

## Validation status

Build validation is recorded separately from interactive testing. Windows preview embedding, multi-monitor layout, resume behaviour and performance require testing on the target computer before release.

See [VALIDATION.md](VALIDATION.md) for observed checks and remaining release gates.

Run `powershell -ExecutionPolicy Bypass -File .\verify-layout.ps1` after building to check settings layout and slider binding. The script opens temporary settings windows and writes a preview image into `dist`.

Run `powershell -ExecutionPolicy Bypass -File .\verify-behaviour.ps1` to check motion calculations, typing, keyboard increments, 50 FPS selection, natural colours and compatibility with older settings.

## Appearance and motion

Number fields support direct typing, minus/plus clicks and Up/Down keys. Sliders support dragging and arrow keys. Dropdowns support keyboard selection. Frame-rate options include 30, 50, 60, 90, 120 and 144 FPS; these are target ceilings, not a guarantee of performance or display synchronisation.

Natural star colours use a restrained mix of near-white, blue-white, cream, amber and orange tones. Each star retains its assigned colour until it is recycled. The palette is inspired by [NASA's overview of stellar colour classes](https://science.nasa.gov/exoplanets/stars/), with artistic proportions rather than a scientific population model. Disable Natural star colours for a single chosen colour; selecting a star colour does this automatically. Chromatic shift overrides both while enabled.

Motion trails use a fixed exposure duration instead of the previous frame's duration, so uneven frame times do not make streak lengths pulse. Lines are antialiased and frame requests use a finer timer while the preview or screensaver is open. The renderer is still CPU-based and does not provide VSync. Actual smoothness depends on resolution, star count and the computer. Lower FPS or star counts may help on slower machines.

## Licence

Copyright (c) 2026 Al Brough. Licensed under the [MIT licence](LICENSE), allowing personal and commercial use, modification and redistribution with the licence notice preserved. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) for dependency and reference-image boundaries. Include both files when sharing the built application.
