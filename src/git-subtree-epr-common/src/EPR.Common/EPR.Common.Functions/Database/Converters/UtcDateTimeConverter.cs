namespace EPR.Common.Functions.Database.Converters;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

internal static class UtcDateTimeConverter
{
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter = new (v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    // Make sure this is called after configuring all your entities.
    internal static void ApplyUtcDateTimeConverter(this ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(UtcConverter);
                }
            }
        }
    }
}