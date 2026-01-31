---
applyTo: "**/NRG.Matrix.Tests/**/*.cs"
---

# TUnit Test Instructions

Write tests using [TUnit](https://tunit.dev/), a modern .NET testing framework with native async support.

## Categories

Add #Unit to all unit tests, #Internal to all tests that uses local sources and #External to all tests that uses sources on the internet.

#Unit - All sources in memory
#Internal - When local resources are accessed (e.g. files, Docker container, etc.)
#External - When external resources are used (e.g. http requests, etc.)

## Test Class Structure

```csharp
[Category("#Unit")]
[Category("FeatureName")]
public class ClassNameTests
{
    [Test]
    public async Task MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        var input = CreateTestInput();
        
        // Act
        var result = MethodUnderTest(input);
        
        // Assert
        await Assert.That(result).IsEqualTo(expected);
    }
}
```

**Docs:** [Categories](https://tunit.dev/docs/tutorials/categories) | [Tests](https://tunit.dev/docs/tutorial-basics/writing-tests)

## Key Rules

- **All test methods MUST be async** returning `Task`
- **All assertions MUST be awaited**
- Use `MethodName_Scenario_ExpectedBehavior` naming
- One class per production class being tested
- Add `[Category]` attributes for test filtering

## Assertions (Fluent API)

```csharp
// Equality
await Assert.That(actual).IsEqualTo(expected);
await Assert.That(actual).IsNotEqualTo(unwanted);

// Null checks
await Assert.That(value).IsNull();
await Assert.That(value).IsNotNull();

// Comparisons
await Assert.That(number).IsGreaterThan(5);
await Assert.That(number).IsLessThanOrEqualTo(10);

// Strings
await Assert.That(text).Contains("substring");
await Assert.That(text).StartsWith("prefix");

// Collections
await Assert.That(collection).IsEmpty();
await Assert.That(list).HasCount().EqualTo(5);
```

**Docs:** [Assertions](https://tunit.dev/docs/tutorial-basics/assertions)

## Data-Driven Tests

Use `MethodDataSource` for parameterized tests:

```csharp
public static IEnumerable<Func<(int Input, int Expected)>> GetTestCases()
{
    yield return () => (1, 2);
    yield return () => (5, 10);
    yield return () => (0, 0);
}

[Test]
[MethodDataSource(nameof(GetTestCases))]
public async Task Method_VariousInputs_ReturnsExpected(int input, int expected)
{
    var result = Process(input);
    await Assert.That(result).IsEqualTo(expected);
}
```

**Docs:** [MethodDataSource](https://tunit.dev/docs/tutorial-parameterised-tests/method-data-source-generation)

## Test Coverage Priorities

1. Pure functions (extension methods, utilities, calculations)
2. Models and data structures (validation, state management)
3. Performance-critical code (verify no unexpected allocations)

## Testing Best Practices

- **Test edge cases:** null/empty, boundary values, invalid inputs
- **Keep tests focused:** one behavior per test
- **Use descriptive names:** test names document expected behavior
- **Group related tests:** use data-driven tests for similar scenarios
- **Performance tests:** use `[Category("Performance")]` for perf-sensitive code

## Running Tests

```bash
# Run all tests
dotnet test

# Run with filter (use --treenode-filter)
dotnet run -- --treenode-filter "/*/*/*/*[Category=#Unit]"

# Filter by class
dotnet run -- --treenode-filter "/*/*/ClassNameTests/*"

# Filter by method
dotnet run -- --treenode-filter "/*/*/ClassName/MethodName"

# Multiple categories (OR)
dotnet run -- --treenode-filter "/*/*/*/*[Category=#Unit]|/*/*/*/*[Category=Performance]"

# Exclude category
dotnet run -- --treenode-filter "/*/*/*/*[Category!=#Unit]"
```

**Note:** Use `--treenode-filter` with pattern `/<Assembly>/<Namespace>/<Class>/<Method>[Property=Value]`. The `--` separator passes arguments to the test app.

Use filters as often as you can to speed up test runs during development.