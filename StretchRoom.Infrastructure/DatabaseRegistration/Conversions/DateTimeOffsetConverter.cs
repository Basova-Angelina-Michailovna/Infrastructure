using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace StretchRoom.Infrastructure.DatabaseRegistration.Conversions;

/// <inheritdoc />
[PublicAPI]
public class DateTimeOffsetConverter()
    : ValueConverter<DateTimeOffset, DateTimeOffset>(d => d.ToUniversalTime(), d => d.ToUniversalTime());