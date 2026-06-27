namespace Pharmaceutical.Core;

public enum DrugOperationError
{
    None,
    ValidationFailed,
    DuplicateKey,
    NotFound,
    DatabaseError
}

public class DrugOperationResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public DrugOperationError Error { get; init; } = DrugOperationError.None;

    public static DrugOperationResult Ok(string? message = null) =>
        new() { Success = true, Message = message, Error = DrugOperationError.None };

    public static DrugOperationResult Fail(string message, DrugOperationError error) =>
        new() { Success = false, Message = message, Error = error };
}
