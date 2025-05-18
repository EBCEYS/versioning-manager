using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
// ReSharper disable ClassNeverInstantiated.Global

namespace versioning_manager_api.DevDatabase;

/// <inheritdoc />
public class DateTimeOffsetConverter()
    : ValueConverter<DateTimeOffset, DateTimeOffset>(d => d.ToUniversalTime(), d => d.ToUniversalTime());