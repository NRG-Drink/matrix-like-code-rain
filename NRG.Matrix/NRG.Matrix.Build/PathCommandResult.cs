namespace NRG.Matrix.Build;

public record PathCommandResult<T>(T? Path, ModularPipelines.Models.CommandResult CommandResult);
