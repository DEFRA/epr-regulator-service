namespace EPR.Common.Functions.Exceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class BadRequestException : Exception
{
    public BadRequestException(string message)
        : base(message)
    {
    }
}
