namespace EPR.RegulatorService.Frontend.Core.Enums.ReprocessorExporter;

public abstract class StringEnumBase(string value)
{
    private readonly string _value = value;

    public override string ToString() => _value;

    public override bool Equals(object? obj) =>
        obj is StringEnumBase other && _value == other._value;

    public override int GetHashCode() => _value.GetHashCode();
}