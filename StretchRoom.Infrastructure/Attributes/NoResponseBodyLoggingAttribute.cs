using JetBrains.Annotations;

namespace StretchRoom.Infrastructure.Attributes;

/// <summary>
///     The <see cref="NoResponseBodyLoggingAttribute" /> class.
/// </summary>
/// <remarks>
///     Use it to hide response logging.
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Method)]
public class NoResponseBodyLoggingAttribute : Attribute
{
}