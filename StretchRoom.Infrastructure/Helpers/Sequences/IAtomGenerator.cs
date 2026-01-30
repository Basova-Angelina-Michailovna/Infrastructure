using System.Numerics;
using JetBrains.Annotations;

namespace StretchRoom.Infrastructure.Helpers.Sequences;

/// <summary>
///     The atomic generator for <see cref="long" />.
/// </summary>
/// <param name="seed">The base seed.</param>
[PublicAPI]
public class AtomicLongGenerator(int? seed = null) : IAtomGenerator<long>
{
    private long _currentId = new Random(seed ?? Environment.TickCount).NextInt64();

    /// <inheritdoc />
    public long Next()
    {
        return Interlocked.Increment(ref _currentId);
    }
}

/// <summary>
///     The atomic generator for <see cref="long" />.
/// </summary>
/// <param name="seed">The base seed.</param>
[PublicAPI]
public class AtomicIntGenerator(int? seed = null) : IAtomGenerator<int>
{
    private int _currentId = new Random(seed ?? Environment.TickCount).Next();

    /// <inheritdoc />
    public int Next()
    {
        return Interlocked.Increment(ref _currentId);
    }
}

/// <summary>
///     The atomic num generator.
/// </summary>
/// <typeparam name="T">The number.</typeparam>
[PublicAPI]
public interface IAtomGenerator<out T> where T : struct, INumber<T>
{
    /// <summary>
    ///     Gets the next value.
    /// </summary>
    /// <returns>The next value.</returns>
    T Next();
}