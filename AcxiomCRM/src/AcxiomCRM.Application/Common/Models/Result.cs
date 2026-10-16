namespace AcxiomCRM.Application.Common.Models;

public class Result<T>
{
    public bool Succeeded { get; }
    public T? Data { get; }
    public string? Error { get; }
    public IDictionary<string, string[]>? ValidationErrors { get; }

    protected Result(bool succeeded, T? data, string? error, IDictionary<string, string[]>? validationErrors = null)
    {
        Succeeded = succeeded;
        Data = data;
        Error = error;
        ValidationErrors = validationErrors;
    }

    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(string error) => new(false, default, error);
    public static Result<T> ValidationFailure(IDictionary<string, string[]> errors) => new(false, default, "One or more validation failures occurred.", errors);
}

public class Result
{
    public bool Succeeded { get; }
    public string? Error { get; }
    public IDictionary<string, string[]>? ValidationErrors { get; }

    protected Result(bool succeeded, string? error, IDictionary<string, string[]>? validationErrors = null)
    {
        Succeeded = succeeded;
        Error = error;
        ValidationErrors = validationErrors;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result ValidationFailure(IDictionary<string, string[]> errors) => new(false, "One or more validation failures occurred.", errors);
}
