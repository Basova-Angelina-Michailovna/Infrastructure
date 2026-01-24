using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace StretchRoom.Infrastructure.Extensions;

/// <summary>
///     The <see cref="object" /> extensions.
/// </summary>
[PublicAPI]
public static class SrObjectExtensions
{
    private static JsonSerializerOptions DiagnosticJsonOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true
    };

    private static JsonSerializerOptions DefaultJsonOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    ///     Casts the <paramref name="value" /> to <see cref="uint" /> value.
    /// </summary>
    /// <param name="value">The value to cast.</param>
    /// <returns>The value of <see cref="uint" />.</returns>
    public static uint ToUInt(this int value)
    {
        return (uint)value;
    }

    /// <summary>
    ///     Casts the <paramref name="value" /> to <see cref="ulong" /> value.
    /// </summary>
    /// <param name="value">The value to cast.</param>
    /// <returns>The value of <see cref="ulong" />.</returns>
    public static ulong ToULong(this long value)
    {
        return (ulong)value;
    }

    /// <param name="obj">The object.</param>
    extension(object obj)
    {
        /// <summary>
        ///     Serializes the <paramref name="obj" /> to diagnostic json string.
        /// </summary>
        /// <returns>The serialized json string.</returns>
        public string ToDiagnosticJson()
        {
            var json = JsonSerializer.Serialize(obj, DiagnosticJsonOptions);
            return json;
        }

        /// <summary>
        ///     Serializes the <paramref name="obj" /> to default json string.
        /// </summary>
        /// <returns>The serialized json string.</returns>
        public string ToJson()
        {
            var json = JsonSerializer.Serialize(obj, DefaultJsonOptions);
            return json;
        }
    }

    extension(string? val)
    {
        /// <summary>
        ///     Indicates that <paramref name="val" /> is null or empty.
        /// </summary>
        /// <returns>true if string is null or empty.</returns>
        public bool IsNullOrEmpty()
        {
            return string.IsNullOrEmpty(val);
        }

        /// <summary>
        ///     Indicates that <paramref name="val" /> is null or white space.
        /// </summary>
        /// <returns>true if string is null or white space.</returns>
        public bool IsNullOrWhiteSpace()
        {
            return string.IsNullOrWhiteSpace(val);
        }
    }
}