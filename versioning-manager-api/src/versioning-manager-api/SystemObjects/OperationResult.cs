namespace versioning_manager_api.SystemObjects;

public class OperationResult<T>(OperationResult result, T? o)
{
    public OperationResult Result { get; } = result;
    public T? Object { get; } = o;

    public bool IsSuccess()
    {
        return Result == OperationResult.Success;
    }

    public bool IsFailure()
    {
        return Result == OperationResult.Failure;
    }

    public bool IsConflict()
    {
        return Result == OperationResult.Conflict;
    }

    public bool IsNotFound()
    {
        return Result == OperationResult.NotFound;
    }

    public static OperationResult<T> SuccessResult(T? o)
    {
        return new OperationResult<T>(OperationResult.Success, o);
    }

    public static OperationResult<T> FailureResult(T? o)
    {
        return new OperationResult<T>(OperationResult.Failure, o);
    }

    public static OperationResult<T> NotFoundResult(T? o)
    {
        return new OperationResult<T>(OperationResult.NotFound, o);
    }

    public static OperationResult<T> ConflictResult(T? o)
    {
        return new OperationResult<T>(OperationResult.Conflict, o);
    }
}

public enum OperationResult
{
    Success,
    Failure,
    NotFound,
    Conflict
}