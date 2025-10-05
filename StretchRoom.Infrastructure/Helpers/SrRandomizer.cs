using JetBrains.Annotations;

namespace StretchRoom.Infrastructure.Helpers;

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

    /// <summary>
    ///     Gets the random string with specified length.
    /// </summary>
    /// <param name="length">The string length. If 0 the random length will be.</param>
    /// <returns>The new instance of <see cref="string" />.</returns>
    public string String(uint length = 0)
    {
        return new string(
            Enumerable.Repeat(Chars, (int)length)
                .Select(s => s[Int(0, s.Length)]).ToArray()
        );
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
    ///     Gets the random array of decimals.
    /// </summary>
    /// <param name="length">The array length.</param>
    /// <returns>The new array of decimals.</returns>
    public decimal[] DecimalArray(uint length = 0)
    {
        return Array((_, rand) => rand.Decimal(), length);
    }
}