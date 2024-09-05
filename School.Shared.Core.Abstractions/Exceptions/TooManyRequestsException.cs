namespace School.Shared.Core.Abstractions.Exceptions;

public class TooManyRequestsException : ServiceException
{
    public TooManyRequestsException()
    {
    }

    public TooManyRequestsException(string? message) : base(message)
    {
    }

    public TooManyRequestsException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}