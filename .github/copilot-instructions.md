# Repository instructions for GitHub Copilot

Use these repository-wide instructions for any work in this repo.

## What this repo is

- A .NET console "Matrix code rain" animation distributed as a .NET global tool.
- Main app lives under `NRG.Matrix/NRG.Matrix/` and is shipped as the `matrix.enter` tool command.
- There is a separate build automation project under `NRG.Matrix/NRG.Matrix.Build/` (pipeline-style build/release tooling).

## How to navigate the codebase

- Entry point: `NRG.Matrix/NRG.Matrix/Program.cs`.
- Main loop / frame timing: `NRG.Matrix/NRG.Matrix/Matrix.cs`.
- Visual behavior and input handling: `NRG.Matrix/NRG.Matrix/Styles/` (default style: `StyleGreenWhite`).
- Rendering/ANSI output: `NRG.Matrix/NRG.Matrix/Displays/`.
- Core data models: `NRG.Matrix/NRG.Matrix/Models/`.
- Character sets: `NRG.Matrix/NRG.Matrix/Alpabeth.cs`.

If you’re unsure where a change belongs, start at the style (`Styles/`) and follow calls into models/display.

For C#-specific formatting and conventions, also follow the path-specific rules in `.github/instructions/csharp.instructions.md`.

## Build / run / validate (Windows-friendly)

Prefer `dotnet` CLI commands from the repo root.

- Restore: `dotnet restore NRG.Matrix/NRG.Matrix.sln`
- Build: `dotnet build NRG.Matrix/NRG.Matrix.sln -c Release`
- Run (dev): `dotnet run --project NRG.Matrix/NRG.Matrix/NRG.Matrix.csproj`
- Tests (if present): `dotnet test NRG.Matrix/NRG.Matrix.sln -c Release`

If you change anything performance-sensitive (main loop, rendering, pooling), do a quick manual run and verify:
- Smooth animation
- Keyboard controls still work
- No obvious regressions (e.g., flicker, high CPU, runaway allocations)

## Formatting and conventions

- Follow `.editorconfig` for formatting and C# style.
- Do not reformat unrelated files.
- Prefer the conventions already present in nearby files.

Prefer to rely on Copilot instruction files (this file + `.github/instructions/*.instructions.md`) rather than inventing new conventions.

## Performance-sensitive areas

- Rendering in `NRG.Matrix/NRG.Matrix/Displays/` is hot-path code. Avoid:
  - LINQ in tight loops
  - per-frame allocations
  - excessive string concatenations
- Prefer existing patterns like `ObjectPool<T>` for frequently created objects.

## Change discipline

- Keep PRs focused: implement only what’s requested.
- Preserve public APIs and behavior unless the request is explicitly about changing them.
- Update docs when you introduce user-visible changes:
  - `README.md` for user-facing behavior
  - `docs/` for developer workflow/architecture

## When you’re blocked

- Don’t guess paths, commands, or behavior.
- Search within the repo for the relevant symbol/feature and align to existing patterns.
- If build/test commands fail, report the exact command and error output and stop before making unrelated sweeping changes.
