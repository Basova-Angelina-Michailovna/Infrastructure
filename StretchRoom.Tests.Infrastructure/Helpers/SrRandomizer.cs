using JetBrains.Annotations;
using Org.BouncyCastle.Utilities.Encoders;

namespace StretchRoom.Tests.Infrastructure;

/// <summary>
///     The <see cref="SrRandomizer" /> class.
/// </summary>
/// <param name="seed">The seed.</param>
[PublicAPI]
public class SrRandomizer(int seed = 0)
{
    private const int MaxLengthForStringGeneration = 256;

    private readonly Random _random = new(seed != 0 ? seed : Random.Shared.Next());

    /// <summary>
    ///     The chars for string generation.
    /// </summary>
    public string Chars { get; set; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower();

    private string HexChars { get; } = "ABCDEF1234567890";

    /// <summary>
    ///     Gets the random string with specified length.
    /// </summary>
    /// <param name="length">The string length. If 0 the random length will be.</param>
    /// <returns>The new instance of <see cref="string" />.</returns>
    public string String(uint length = 0)
    {
        return String(length, Chars);
    }

    private string String(uint length, string chars)
    {
        length = length == 0 ? UInt(0U, byte.MaxValue) : length;
        return new string(
            Enumerable.Repeat(chars, (int)length)
                .Select(s => s[Int(0, s.Length)]).ToArray()
        );
    }

    /// <summary>
    ///     Gets the random hex string with specified length.
    /// </summary>
    /// <param name="length">The string length. If 0 the random length will be.</param>
    /// <returns>The new instance of hex <see cref="string" />.</returns>
    public string HexString(uint length = 0) //TODO: test it 
    {
        return String(length, HexChars);
    }

    /// <summary>
    ///     Gets the random int value.
    /// </summary>
    /// <returns>The random int value.</returns>
    public int Int()
    {
        return Int(int.MinValue, int.MaxValue);
    }

    /// <summary>
    ///     Gets the random int value.
    /// </summary>
    /// <returns>The random int value.</returns>
    public int Int(int minValue)
    {
        return Int(minValue, int.MaxValue);
    }

    /// <summary>
    ///     Gets the random int value.
    /// </summary>
    /// <returns>The random int value.</returns>
    public int Int(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }

    /// <summary>
    ///     Gets the random int value.
    /// </summary>
    /// <returns>The random int value.</returns>
    public uint UInt()
    {
        return UInt(0, uint.MaxValue);
    }

    /// <summary>
    ///     Gets the random int value.
    /// </summary>
    /// <returns>The random int value.</returns>
    public uint UInt(uint minValue)
    {
        return UInt(minValue, uint.MaxValue);
    }

    /// <summary>
    ///     Gets the random int value.
    /// </summary>
    /// <returns>The random int value.</returns>
    public uint UInt(uint minValue, uint maxValue)
    {
        return (uint)
            (_random.Next((int)minValue, (int)maxValue / 2) +
             _random.Next((int)minValue, (int)maxValue / 2));
    }

    /// <summary>
    ///     Gets the random long value.
    /// </summary>
    /// <returns>The random long value.</returns>
    public long Long()
    {
        return _random.Next();
    }

    /// <summary>
    ///     Gets the random long value.
    /// </summary>
    /// <returns>The random long value.</returns>
    public long Long(long minValue)
    {
        return Long(minValue, long.MaxValue);
    }

    /// <summary>
    ///     Gets the random long value.
    /// </summary>
    /// <returns>The random long value.</returns>
    public long Long(long minValue, long maxValue)
    {
        return _random.NextInt64(minValue, maxValue);
    }
    
    /// <summary>
    ///     Gets the random byte value.
    /// </summary>
    /// <returns>The random byte value.</returns>
    public byte Byte() // TODO: test it
    {
        return Byte(byte.MinValue, byte.MaxValue);
    }

    /// <summary>
    ///     Gets the random byte value.
    /// </summary>
    /// <returns>The random byte value.</returns>
    public byte Byte(long minValue)
    {
        return Byte(minValue, byte.MaxValue);
    }

    /// <summary>
    ///     Gets the random byte value.
    /// </summary>
    /// <returns>The random byte value.</returns>
    public byte Byte(long minValue, long maxValue)
    {
        var bytes = new byte[1];
        _random.NextBytes(bytes);
        return bytes[0];
    }

    /// <summary>
    ///     Gets the random double value.
    /// </summary>
    /// <returns>The random double value.</returns>
    public double Double()
    {
        return Double(double.MinValue / 2, double.MaxValue / 2);
    }

    /// <summary>
    ///     Gets the random double value.
    /// </summary>
    /// <returns>The random double value.</returns>
    public double Double(double minValue)
    {
        return Double(minValue, double.MaxValue);
    }

    /// <summary>
    ///     Gets the random double value.
    /// </summary>
    /// <returns>The random double value.</returns>
    public double Double(double minValue, double maxValue)
    {
        return _random.NextDouble() * (maxValue - minValue) + minValue;
    }

    /// <summary>
    ///     Gets the random decimal value.
    /// </summary>
    /// <returns>The random decimal value.</returns>
    public decimal Decimal()
    {
        return Decimal(decimal.MinValue, decimal.MaxValue);
    }

    /// <summary>
    ///     Gets the random decimal value.
    /// </summary>
    /// <returns>The random decimal value.</returns>
    public decimal Decimal(decimal minValue)
    {
        return Decimal(minValue, decimal.MaxValue);
    }

    /// <summary>
    ///     Gets the random decimal value.
    /// </summary>
    /// <returns>The random decimal value.</returns>
    public decimal Decimal(decimal minValue, decimal maxValue)
    {
        return (decimal)_random.NextSingle() * (maxValue - minValue) + minValue;
    }

    /// <summary>
    ///     Gets the random array of specified type.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <param name="conf">
    ///     The func generates each element of array.<br />
    ///     The first arg is index of element in array.
    /// </param>
    /// <typeparam name="T"></typeparam>
    /// <returns>The new array of <typeparamref name="T" />.</returns>
    public T[] Array<T>(Func<int, SrRandomizer, T> conf, uint length = 0)
    {
        if (length == 0) length = UInt(0, 256);
        return Enumerable.Range(0, (int)length)
            .Select(ind => conf(ind, this)).ToArray();
    }

    /// <summary>
    ///     Gets the random array of strings.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <param name="stringMaxLength">The max length of string.</param>
    /// <returns>The new array of strings.</returns>
    public string[] StringArray(uint length = 0, uint stringMaxLength = 0)
    {
        return Array<string>(
            (_, rand) => rand.String(UInt(0, stringMaxLength)),
            length);
    }

    /// <summary>
    ///     Gets the random array of longs.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <returns>The new array of longs.</returns>
    public long[] LongArray(uint length = 0)
    {
        return Array((_, rand) => rand.Long(), length);
    }

    /// <summary>
    ///     Gets the random array of ints.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <returns>The new array of ints.</returns>
    public int[] IntArray(uint length = 0)
    {
        return Array((_, rand) => rand.Int(), length);
    }

    /// <summary>
    ///     Gets the random array of uints.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <returns>The new array of uints.</returns>
    public uint[] UIntArray(uint length = 0)
    {
        return Array((_, rand) => rand.UInt(), length);
    }

    /// <summary>
    ///     Gets the random array of doubles.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <returns>The new array of doubles.</returns>
    public double[] DoubleArray(uint length = 0)
    {
        return Array((_, rand) => rand.Double(), length);
    }
    
    /// <summary>
    ///     Gets the random array of bytes.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <returns>The new array of bytes.</returns>
    public byte[] ByteArray(uint length = 0)
    {
        return Array((_, rand) => rand.Byte(), length);
    }

    /// <summary>
    ///     Gets the random array of decimals.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <returns>The new array of decimals.</returns>
    public decimal[] DecimalArray(uint length = 0)
    {
        return Array((_, rand) => rand.Decimal(), length);
    }

    /// <summary>
    ///     Gets the random datetime.
    /// </summary>
    /// <returns>The random datetime.</returns>
    public DateTime DateTime()
    {
        return DateTime(System.DateTime.UnixEpoch);
    }

    /// <summary>
    ///     Gets the random datetime.
    /// </summary>
    /// <returns>The random datetime.</returns>
    public DateTime DateTime(DateTime minValue)
    {
        return DateTime(minValue, System.DateTime.MaxValue);
    }

    /// <summary>
    ///     Gets the random datetime.
    /// </summary>
    /// <returns>The random datetime.</returns>
    public DateTime DateTime(DateTime minValue, DateTime maxValue)
    {
        var year = Int(0, maxValue.Year + 1);
        var month = Int(0, maxValue.Month + 1);
        var day = Int(0, maxValue.Day + 1);
        var hour = Int(0, maxValue.Hour + 1);
        var minute = Int(0, maxValue.Minute);
        var second = Int(0, maxValue.Second);
        return minValue
            .AddYears(year)
            .AddMonths(month)
            .AddDays(day)
            .AddHours(hour)
            .AddMinutes(minute)
            .AddSeconds(second);
    }

    /// <summary>
    ///     Gets the random DateTimeOffset.
    /// </summary>
    /// <returns>The random DateTimeOffset.</returns>
    public DateTimeOffset DateTimeOffset()
    {
        return DateTimeOffset(System.DateTime.UnixEpoch);
    }

    /// <summary>
    ///     Gets the random DateTimeOffset.
    /// </summary>
    /// <returns>The random DateTimeOffset.</returns>
    public DateTimeOffset DateTimeOffset(DateTimeOffset minValue)
    {
        return DateTimeOffset(minValue, System.DateTime.MaxValue);
    }

    /// <summary>
    ///     Gets the random DateTimeOffset.
    /// </summary>
    /// <returns>The random DateTimeOffset.</returns>
    public DateTimeOffset DateTimeOffset(DateTimeOffset minValue, DateTimeOffset maxValue)
    {
        var year = Int(0, maxValue.Year + 1);
        var month = Int(0, maxValue.Month + 1);
        var day = Int(0, maxValue.Day + 1);
        var hour = Int(0, maxValue.Hour + 1);
        var minute = Int(0, maxValue.Minute);
        var second = Int(0, maxValue.Second);
        return minValue
            .AddYears(year)
            .AddMonths(month)
            .AddDays(day)
            .AddHours(hour)
            .AddMinutes(minute)
            .AddSeconds(second)
            .ToOffset(TimeSpan.FromHours(Int(-12, 13)));
    }

    /// <summary>
    ///     Gets the random element of collection.
    /// </summary>
    /// <param name="elements">The elements.</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T RandomElement<T>(IEnumerable<T> elements)
    {
        return RandomElements(elements, 1).First();
    }

    /// <summary>
    ///     Gets the random elements from collection.
    /// </summary>
    /// <param name="elements">The elements.</param>
    /// <param name="count">The result collection length.</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>The new instance of <typeparamref name="T" /> collection.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Throw if <paramref name="count" /> is greater then
    ///     <paramref name="elements" /> length.
    /// </exception>
    public IEnumerable<T> RandomElements<T>(IEnumerable<T> elements, uint count)
    {
        var array = elements.ToArray();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, (uint)array.Length);

        _random.Shuffle(array);
        return array.Take((int)count);
    }

    /// <summary>
    ///     Gets the random enum entity.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <returns>The <typeparamref name="TEnum" /> exemplar.</returns>
    public TEnum Enum<TEnum>()
        where TEnum : struct, Enum
    {
        var values = System.Enum.GetValues<TEnum>();
        return RandomElement(values);
    }

    /// <summary>
    ///     Gets the random enum entities.
    /// </summary>
    /// <param name="count">The num of random enums to get.</param>
    /// <typeparam name="TEnum"></typeparam>
    /// <returns>The <typeparamref name="TEnum" /> exemplar.</returns>
    public IEnumerable<TEnum> Enums<TEnum>(uint count)
        where TEnum : struct, Enum
    {
        var values = System.Enum.GetValues<TEnum>();
        return RandomElements(values, count);
    }
}