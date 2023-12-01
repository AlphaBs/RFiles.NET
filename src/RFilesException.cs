namespace RFiles.NET;

public class RFilesException : Exception
{
    private static string CreateExceptionMessage(int statusCode, string? errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return $"StatusCode={statusCode}";
        else
            return $"{errorMessage}, StatusCode={statusCode}";
    }

    public RFilesException(int statusCode, string? errorMessage) : 
        base(CreateExceptionMessage(statusCode, errorMessage))
    {
        StatusCode = statusCode;
        Error = errorMessage;
    }

    public int StatusCode { get; }
    public string? Error { get; }
}