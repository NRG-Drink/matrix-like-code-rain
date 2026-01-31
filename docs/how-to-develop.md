# How to Develop - Matrix Code Rain

This guide provides information for developers who want to understand and contribute to the Matrix Code Rain project.

## Project Overview

Matrix Code Rain is a .NET console application that recreates the iconic "digital rain" effect from The Matrix movies. It's distributed as a global .NET tool and provides an interactive, customizable matrix-style animation in the terminal.

## Solution Structure

The repository is organized into two main projects:

```
NRG.Matrix/
├── NRG.Matrix.sln              # Main solution file
├── NRG.Matrix/                 # Main application project
└── NRG.Matrix.Build/           # Build automation project
```

### NRG.Matrix (Main Application)

This is the core application that users install and run.

**Key Files:**
- `Program.cs` - Entry point, handles cancellation tokens and starts the matrix
- `Matrix.cs` - Main game loop controller with frame timing logic
- `Alpabeth.cs` - Character sets (Latin, Katakana, Numbers, Symbols)
- `ObjectPool.cs` - Generic object pooling for performance optimization

**Folder Structure:**
- `Displays/` - Rendering logic for console output
  - `IAnsiConsole.cs` - Display interface
  - `IAnsiConsoleChar.cs` - Character rendering interface
  - `AnsiConsolePrintAll.cs` - ANSI console renderer implementation
  
- `Models/` - Core data structures
  - `CharDynamic.cs` - Animated falling characters
  - `CharStatic.cs` - Static characters in the matrix
  - `Shot.cs` - Vertical "shot" or stream of falling characters
  - `KeyInputHandler.cs` - Keyboard input management
  - `NumberWithRandomness.cs` - Numbers with random variation
  - `RGB.cs` - Color representation
  - `ICanFall.cs` - Interface for falling objects
  
- `Styles/` - Visual style implementations
  - `IMatrixStyle.cs` - Style interface
  - `StyleGreenWhite.cs` - Default green/white matrix style with interactive controls
  
- `Settings/` - Configuration interfaces
  - `IStyleSettings.cs` - Style settings interface

**Project Configuration:**
- **Target Framework:** .NET 10.0
- **Output Type:** Executable
- **Package Type:** .NET Global Tool
- **Tool Command:** `matrix.enter`
- **Dependencies:**
  - `CommandLineParser` (2.9.1) - For command-line argument parsing
  - `Microsoft.CodeAnalysis.CSharp.Scripting` (4.9.2) - For scripting support

### NRG.Matrix.Build (Build Automation)

This project uses the ModularPipelines framework to automate the build, test, pack, and release process.

**Key Components:**
- `Program.cs` - Pipeline configuration and execution
- `RepoPaths.cs` - Repository path discovery and management
- `Utils.cs` - Utility functions

**Module Categories:**
- `Modules/Common/` - Common build tasks (FindRepoPaths, BuildSolution, CleanSolution, GitVersion)
- `Modules/Testing/` - Test execution modules
- `Modules/Packing/` - NuGet package creation
- `Modules/Publishing/` - Publishing to package feeds
- `Modules/Releasing/` - GitHub release creation

## Architecture

### Core Loop

The application follows a standard game loop pattern:

1. **Initialization** - Set up console, create style instance
2. **Frame Loop** - Runs at ~60 FPS (16ms per frame)
   - Update internal objects (falling characters, shots)
   - Render frame if changes occurred
   - Handle keyboard input
   - Calculate frame timing
   - Sleep to maintain target frame rate
3. **Cleanup** - Restore console state on exit

### Object Pooling

The application uses object pooling (`ObjectPool<T>`) to minimize garbage collection pressure:
- `CharDynamic` objects are pooled for falling characters
- `Shot` objects are pooled for character streams

This is critical for maintaining smooth 60 FPS performance.

### Style System

The style system allows different visual themes:
- Implements `IMatrixStyle` interface
- `StyleGreenWhite` is the current implementation
- Handles character selection, colors, falling speeds, and input controls

### Display System

Rendering is abstracted through interfaces:
- `IAnsiConsole` - Console display interface
- `IAnsiConsoleChar` - Individual character interface
- Uses ANSI escape codes for colors and positioning

## Development Workflow

### Prerequisites

- .NET 10.0 SDK or later
- Visual Studio 2022+ or Visual Studio Code with C# extension
- Git

### Getting Started

1. **Clone the Repository**
   ```bash
   git clone https://github.com/NRG-Drink/matrix-like-code-rain.git
   cd matrix-like-code-rain
   ```

2. **Open the Solution**
   ```bash
   cd NRG.Matrix
   # Open NRG.Matrix.sln in Visual Studio or VS Code
   ```

3. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the Project**
   ```bash
   dotnet build
   ```

5. **Run the Application**
   ```bash
   dotnet run --project NRG.Matrix/NRG.Matrix.csproj
   ```

### Local Testing

To test the application as a global tool locally:

1. **Pack the Tool**
   ```bash
   dotnet pack NRG.Matrix/NRG.Matrix.csproj -c Release
   ```

2. **Uninstall Previous Version** (if exists)
   ```bash
   dotnet tool uninstall -g NRG.Matrix
   ```

3. **Install Local Version**
   ```bash
   dotnet tool install -g NRG.Matrix --add-source ./NRG.Matrix/bin/Release
   ```

4. **Run the Tool**
   ```bash
   matrix.enter
   ```

### Interactive Controls During Development

When running the application:
- Press `S` - Toggle statistics panel (FPS, object counts)
- Press `C` - Toggle controls help panel
- `SHIFT + Arrows` - Adjust object falling speed
- `CTRL + Arrows` - Adjust object generation speed
- `CTRL + C` - Exit

These controls are essential for testing performance and visual effects.

## Code Conventions

- **Nullable Reference Types:** Enabled - use `?` for nullable types
- **Implicit Usings:** Enabled - common namespaces are automatically imported
- **Naming:** 
  - Private fields: `_camelCase` with underscore prefix
  - Public properties: `PascalCase`
  - Methods: `PascalCase`
  - Interfaces: `IPascalCase` with 'I' prefix
- **Async/Await:** Use async methods where I/O or delays occur

## Key Concepts

### Frame Timing

The `Matrix.cs` class maintains a target of 60 FPS:
- Target frame time: ~16.67ms (1000ms / 60 FPS)
- Measures actual frame time with `Stopwatch`
- Sleeps remaining time to avoid busy-waiting
- Only renders when changes occur (optimization)

### Character Animation

Characters change over time using the `CharDynamic` class:
- Each character has a `ChangeTimeMilliseconds` property
- Characters randomly pick new symbols from the alphabet when expired
- Uses `Stopwatch` to track elapsed time

### Shot System

A "Shot" represents a vertical stream of falling characters:
- Has an X position and Z-depth
- Contains multiple `CharDynamic` characters at different Y positions
- Moves downward over time based on fall delay settings

## Testing Strategy

Currently, the project focuses on manual testing:
1. Run the application
2. Verify visual appearance
3. Test interactive controls
4. Monitor performance with statistics panel (press `S`)
5. Check for memory leaks with long-running sessions

**Future Testing Opportunities:**
- Unit tests for `ObjectPool<T>`
- Unit tests for `NumberWithRandomness`
- Unit tests for RGB color calculations
- Integration tests for keyboard input handling

## Build Automation

The `NRG.Matrix.Build` project provides automated CI/CD:

**Local Build Execution:**
```bash
dotnet run --project NRG.Matrix.Build/NRG.Matrix.Build.csproj
```

**Build Pipeline Steps:**
1. Find repository paths and projects
2. Get version from Git (GitVersion)
3. Build solution
4. Run tests (if any)
5. Create NuGet package
6. Publish to NuGet (on release)
7. Create GitHub release
8. Upload release assets

## Performance Considerations

1. **Object Pooling** - Reuse objects to avoid GC pressure
2. **Frame Skipping** - Only render when changes occur
3. **Efficient Rendering** - Use ANSI escape codes for direct cursor positioning
4. **Throttling** - Configurable delays for falling and generation speeds
5. **Buffer Management** - Minimize console buffer operations

## Contributing Guidelines

When contributing:

1. **Fork & Branch** - Create a feature branch from main
2. **Code Style** - Follow existing conventions
3. **Test Locally** - Run and verify changes before submitting
4. **Performance** - Profile changes if modifying core loop
5. **Documentation** - Update docs for new features
6. **Pull Request** - Provide clear description of changes

## Debugging Tips

1. **Statistics Panel** - Press `S` to see FPS, object counts, frame times
2. **Slow Down** - Use `SHIFT + Down Arrow` to slow falling speed for observation
3. **Reduce Generation** - Use `CTRL + Down Arrow` to reduce new object generation
4. **Console Buffer** - Be aware that large console windows impact performance
5. **Breakpoints** - Avoid breakpoints in the main loop; use conditional breakpoints or logging

## Common Development Tasks

### Adding a New Style

1. Create a class implementing `IMatrixStyle`
2. Implement required methods: `UpdateInternalObjects()`, `DisplayFrame()`, `HandleKeyInput()`
3. Define color schemes using `RGB` class
4. Test frame rate performance

### Adding New Characters

1. Modify `Alpabeth.cs` to add new character sets
2. Update `StyleGreenWhite.cs` to include new alphabet in the `_greenWhiteChars` array

### Adjusting Performance

1. Modify `_fallDelay` in `StyleGreenWhite.cs` for falling speed
2. Adjust `_generateNewTimeBase` for generation frequency
3. Change `frameTimeTarget` in `Matrix.cs` for target FPS

### Adding Command-Line Options

1. Create a class with properties decorated with `CommandLineParser` attributes
2. Parse arguments in `Program.cs` using `CommandLineParser`
3. Pass options to `Matrix` or style constructors

## Release Process

1. Update version in `NRG.Matrix.csproj`
2. Update `README.md` if needed
3. Commit changes
4. Run build automation: `dotnet run --project NRG.Matrix.Build`
5. Build automation creates GitHub release and publishes to NuGet

## Resources

- **Repository:** https://github.com/NRG-Drink/matrix-like-code-rain
- **License:** MIT
- **.NET Documentation:** https://docs.microsoft.com/en-us/dotnet/
- **ANSI Escape Codes:** For console colors and cursor positioning

## Questions?

For questions or issues:
1. Check existing GitHub issues
2. Create a new issue with detailed description
3. Include reproduction steps and environment details
