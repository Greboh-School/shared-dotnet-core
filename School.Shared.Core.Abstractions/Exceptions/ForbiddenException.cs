namespace School.Shared.Core.Abstractions.Exceptions;

public class ForbiddenException : ServiceException
{
    public ForbiddenException()
    {
    }

    public ForbiddenException(string? message) : base(message)
    {
    }

    public ForbiddenException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}