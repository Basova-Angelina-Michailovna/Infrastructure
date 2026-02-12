using AwesomeAssertions;
using EBCEYS.RabbitMQ.Configuration;
using FluentValidation;
using StretchRoom.Infrastructure.Options;
using StretchRoom.Tests.Infrastructure.Helpers;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace StretchRoom.Infrastructure.UnitTests.Validators;

public class SimpleRabbitMqConfigurationValidatorTests
{
    private static readonly SrRandomizer Randomizer = SrRandomizer.Create();

    [TestCaseSource(nameof(ValidConfiguration))]
    public void When_Validate_With_SpecifiedValidCases_Result_NoException(SimpleRabbitMqConfiguration? configuration)
    {
        var act = configuration.Validate;
        act.Should().NotThrow();
    }

    [TestCaseSource(nameof(InvalidConfiguration))]
    public void When_Validate_With_SpecifiedInvalidCases_Result_ValidationException(
        SimpleRabbitMqConfiguration? configuration)
    {
        var act = configuration.Validate;
        act.Should().Throw<ValidationException>();
    }

    private static IEnumerable<TestCaseData> ValidConfiguration()
    {
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16)
        }).SetArgDisplayNames("With_AllValidWithoutCallback");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_AllValidWithoutCallbackAndRoutingKey");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>()
        }).SetArgDisplayNames("With_AllValidWithCallback");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_AllValidWithCallbackAndRoutingKey");
    }

    private static IEnumerable<TestCaseData> InvalidConfiguration()
    {
        yield return new TestCaseData((SimpleRabbitMqConfiguration?)null).SetArgDisplayNames("With_Null");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = null,
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_NULLConnectionString");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = Randomizer.String(64),
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_NotUriConnectionString");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = null,
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_NULLExName");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = string.Empty,
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_EmptyExName");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = (ExchangeTypes)54,
            QueueName = Randomizer.String(16),
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_InvalidExType");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = null,
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_NULLQueueName");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = string.Empty,
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_EmptyQueueName");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = null,
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_TimeoutSetButQueueNameCallbackIsNull");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = string.Empty,
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_TimeoutSetButQueueNameCallbackIsEmpty");

        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = null,
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_TimeoutSetButExNameCallbackIsNull");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = string.Empty,
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_TimeoutSetButExNameCallbackIsEmpty");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            TimeoutCallback = Randomizer.TimeSpan(TimeSpan.FromMinutes(5)),
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = (ExchangeTypes)54,
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_TimeoutSetButExTypeCallbackIsNotInEnum");

        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            TimeoutCallback = TimeSpan.Zero,
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_AllValidWithTimeoutZero");
        yield return new TestCaseData(new SimpleRabbitMqConfiguration
        {
            ConnectionString = "amqp://guest:guest@localhost:5672",
            ExName = Randomizer.String(16),
            ExType = Randomizer.Enum<ExchangeTypes>(),
            QueueName = Randomizer.String(16),
            TimeoutCallback = TimeSpan.FromSeconds(-1),
            QueueNameCallback = Randomizer.String(16),
            ExNameCallback = Randomizer.String(16),
            ExTypeCallback = Randomizer.Enum<ExchangeTypes>(),
            RoutingKey = Randomizer.String(16)
        }).SetArgDisplayNames("With_AllValidWithTimeoutLessThanZero");
    }
}