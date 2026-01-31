# How to Develop

Developer guide for the Matrix Code Rain .NET global tool.

## Project Overview

.NET 10.0 console application recreating The Matrix "digital rain" effect. Distributed as `matrix.enter` global tool.

## Repository Structure

```
NRG.Matrix/
├── NRG.Matrix/              # Main app (60 FPS animation loop)
│   ├── Program.cs           # Entry point
│   ├── Matrix.cs            # Core game loop
│   ├── Displays/            # ANSI rendering
│   ├── Models/              # CharDynamic, Shot, ObjectPool
│   └── Styles/              # StyleGreenWhite (default)
├── NRG.Matrix.Build/        # ModularPipelines build automation
└── NRG.Matrix.Tests/        # TUnit tests
```

## Quick Start

```bash
# Build and run
dotnet restore NRG.Matrix/NRG.Matrix.sln
dotnet build NRG.Matrix/NRG.Matrix.sln -c Release
dotnet run --project NRG.Matrix/NRG.Matrix/NRG.Matrix.csproj

# Test as global tool
dotnet pack NRG.Matrix/NRG.Matrix/NRG.Matrix.csproj -c Release
dotnet tool uninstall -g NRG.Matrix
dotnet tool install -g NRG.Matrix --add-source ./NRG.Matrix/bin/Release
matrix.enter
```

**Interactive Controls:**
- `S` - Statistics (FPS, object counts)
- `C` - Controls help
- `SHIFT + Arrows` - Adjust fall speed
- `CTRL + Arrows` - Adjust generation speed

## Architecture

**Core Loop (60 FPS):**
1. Update objects (falling characters, shots)
2. Render if changed (ANSI escape codes)
3. Handle keyboard input
4. Sleep to maintain frame timing

**Performance:**
- `ObjectPool<T>` for `CharDynamic` and `Shot` to avoid GC pressure
- Frame skipping when no changes
- Direct console buffer manipulation

**Key Classes:**
- `Matrix.cs` - Frame timing controller
- `StyleGreenWhite.cs` - Default visual style, keyboard handling
- `CharDynamic.cs` - Animated falling characters
- `Shot.cs` - Vertical character streams

## Testing

Uses [TUnit](https://tunit.dev/) testing framework (Microsoft.Testing.Platform).

```bash
# Run tests
dotnet test NRG.Matrix/NRG.Matrix.Tests/NRG.Matrix.Tests.csproj

# With filter (tree-node syntax)
dotnet run --project NRG.Matrix.Tests -- --treenode-filter "/*/*/*/*[Category=#Unit]"
```

**Test conventions:**
- All tests async, returning `Task`
- All assertions awaited: `await Assert.That(result).IsEqualTo(expected)`
- Naming: `MethodName_Scenario_ExpectedBehavior`
- See `.github/instructions/tunit-tests.instructions.md` for details

**Focus areas:**
1. Pure functions and utilities (extensions, calculations)
2. Models (`ObjectPool`, `NumberWithRandomness`, `RGB`)
3. Performance (verify no unexpected allocations)

## Code Conventions

Follow `.editorconfig`:
- 4 spaces, CRLF
- File-scoped namespaces
- No `var`
- Private fields: `_camelCase`
- Public: `PascalCase`
- Interfaces: `IPascalCase`

**Performance-sensitive areas:**
- `Displays/` rendering code - avoid LINQ, per-frame allocations
- Use existing `ObjectPool<T>` patterns

## Common Tasks

**Add new style:**
1. Implement `IMatrixStyle`
2. Define `UpdateInternalObjects()`, `DisplayFrame()`, `HandleKeyInput()`
3. Test frame rate with `S` key

**Add characters:**
- Edit `Alpabeth.cs` character sets
- Update `StyleGreenWhite._greenWhiteChars`

**Adjust performance:**
- `StyleGreenWhite._fallDelay` - fall speed
- `StyleGreenWhite._generateNewTimeBase` - generation rate
- `Matrix.frameTimeTarget` - target FPS

## Build Automation

```bash
dotnet run --project NRG.Matrix.Build/NRG.Matrix.Build.csproj
```

Pipeline: GitVersion → Build → Test → Pack → Publish (NuGet) → Release (GitHub)

## Contributing

1. Fork and create feature branch
2. Follow `.editorconfig` and existing patterns
3. Test locally (verify animation, controls, FPS)
4. Profile if touching core loop or rendering
5. Update docs for user-visible changes

## Debugging

- Use `S` key for real-time stats
- Slow motion: `SHIFT + Down`
- Avoid breakpoints in main loop (use conditional or logging)
- Large console windows impact performance

## Resources

- [Repository](https://github.com/NRG-Drink/matrix-like-code-rain)
- [TUnit Docs](https://tunit.dev/)
- [.NET Docs](https://docs.microsoft.com/en-us/dotnet/)
