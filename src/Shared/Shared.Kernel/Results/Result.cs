using Shared.Kernel.Responses;

namespace Shared.Kernel.Results;

/// <summary>
/// Representa o resultado de uma operação (sucesso ou falha)
/// </summary>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ResponseCodeDTO? Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = null;
    }

    private Result(ResponseCodeDTO error)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(ResponseCodeDTO error) => new(error);

    // Implicit conversions
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(ResponseCodeDTO error) => Failure(error);

    // Match pattern
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<ResponseCodeDTO, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Error!);
    }
}

/// <summary>
/// Result sem valor (apenas sucesso/falha)
/// </summary>
public sealed class Result
{
    public bool IsSuccess { get; }
    public ResponseCodeDTO? Error { get; }

    private Result(bool isSuccess, ResponseCodeDTO? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true);
    public static Result Failure(ResponseCodeDTO error) => new(false, error);

    public static implicit operator Result(ResponseCodeDTO error) => Failure(error);
}

