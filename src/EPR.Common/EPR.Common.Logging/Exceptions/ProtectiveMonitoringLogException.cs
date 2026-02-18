namespace EPR.Common.Logging.Exceptions;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

[ExcludeFromCodeCoverage]
[Serializable]
public class ProtectiveMonitoringLogException : Exception
{
    public ProtectiveMonitoringLogException()
    {
    }
    public ProtectiveMonitoringLogException(string message)
        : base(message)
    {
    }
    public ProtectiveMonitoringLogException(string message, Exception inner)
        : base(message, inner)
    {
    }
    protected ProtectiveMonitoringLogException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}