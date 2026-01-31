---
applyTo: "**/*.cs,**/*.csproj,**/*.sln"
---

# C# / .NET instructions

These instructions apply when editing C# and .NET project files in this repository.

## Style + formatting (source of truth: `.editorconfig`)

- Use **4 spaces** for indentation (`indent_size = 4`, `tab_width = 4`).
- Use **CRLF** line endings (`end_of_line = crlf`).
- Always use braces for control blocks (`csharp_prefer_braces = true`).
- Keep `using` directives **outside** the namespace (`csharp_using_directive_placement = outside_namespace`).
- Use **file-scoped namespaces** (`csharp_style_namespace_declarations = file_scoped`).
- Do **not** use top-level statements (`csharp_style_prefer_top_level_statements = false`).
- Avoid `var` (the repo’s style disables `var` in all common cases).
- Keep spaces around binary operators (`csharp_space_around_binary_operators = before_and_after`).

## Naming

- Interfaces start with `I`.
- Types and members: `PascalCase`.
- Private fields: follow existing code: `_camelCase`.

## Code patterns (match existing code)

- Prefer `Random.Shared`.
- Prefer `Task.CompletedTask` / `Task.FromResult(...)` for trivially async methods.
- Prefer object/collection initializers when they improve clarity.
- Prefer `is null` / `is not null` pattern checks.

## Performance-sensitive guidance

- Rendering code in `NRG.Matrix/NRG.Matrix/Displays/` is hot-path.
  - Avoid LINQ in tight loops.
  - Avoid per-frame allocations and large intermediate strings.
  - Use pooling patterns already in the repo (`ObjectPool<T>`) when adding frequently created objects.

## Build / validate (expected)

Run from repo root:

- `dotnet restore NRG.Matrix/NRG.Matrix.sln`
- `dotnet build NRG.Matrix/NRG.Matrix.sln -c Release`

If you touch frame timing or rendering, manually run:

- `dotnet run --project NRG.Matrix/NRG.Matrix/NRG.Matrix.csproj`

…and verify smooth animation + keyboard controls.
