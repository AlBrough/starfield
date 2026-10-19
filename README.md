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

## Licence

Copyright (c) 2026 Al Brough. Licensed under the [MIT licence](LICENSE), allowing personal and commercial use, modification and redistribution with the licence notice preserved. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) for dependency and reference-image boundaries. Include both files when sharing the built application.
