using JetBrains.Annotations;

namespace StretchRoom.Infrastructure.Helpers;

/// <summary>
/// The <see cref="SrRandom"/> class.
/// </summary>
/// <param name="seed">The seed.</param>
[PublicAPI]
public class SrRandom(int seed = 0)
{
    private readonly Random _random = new(seed != 0 ? seed : Random.Shared.Next());

    /// <summary>
    /// Gets the random string with specified length.
    /// </summary>
    /// <param name="length">The string length. If 0 the random length will be.</param>
    /// <returns>The new instance of <see cref="string"/>.</returns>
    public string String(uint length = 0) // TODO: test it
    {
        var bytes = new byte[length == 0 ? Random.Shared.Next(0) : length];
        _random.NextBytes(bytes);
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Gets the random int value.
    /// </summary>
    /// <returns>The random int value.</returns>
    public int Int()
    {
        return _random.Next();
    }

    /// <summary>
    /// Gets the random int value.
    /// </summary>
    /// <returns>The random int value.</returns>
    public int Int(int minValue)
    {
        return Int(minValue, int.MaxValue);
    }

    /// <summary>
    /// Gets the random int value.
    /// </summary>
    /// <returns>The random int value.</returns>
    public int Int(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }
}