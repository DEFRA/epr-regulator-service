namespace EPR.Common.Functions.Exceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class SeedLockException : Exception
{
    public SeedLockException()
    {
    }

    public SeedLockException(string? message)
        : base(message)
    {
    }

    public SeedLockException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
