using System.Text;
using EBCEYS.RabbitMQ.Configuration;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace StretchRoom.Infrastructure.Options;

/// <summary>
///     The <see cref="RabbitMqExtensions" /> class.
/// </summary>
[PublicAPI]
public static class RabbitMqExtensions
{
    internal const string ConnectionStringField = "ConnectionString";
    internal const string ExchangeConfField = "ExchangeConfiguration";
    internal const string QueueConfField = "QueueConfiguration";
    internal const string QoSConfField = "QoSConfiguration";
    internal const string CallbackConfField = "CallbackConfiguration";
    internal const string OnStartConfField = "OnStartConfiguration";
    internal const string CreateChannelConfField = "CreateChannelConfiguration";


    extension(IConfiguration configuration)
    {
        /// <summary>
        ///     Gets the <see cref="RabbitMQConfiguration" /> from <paramref name="section" /> of <paramref name="configuration" />
        ///     .
        /// </summary>
        /// <param name="section">The section name.</param>
        /// <returns>The new instance of <see cref="RabbitMQConfiguration" />.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public RabbitMQConfiguration GetRabbitMqConfiguration(string section)
        {
            var conf = configuration.GetSection(section);
            if (conf.GetValue<string>(nameof(SimpleRabbitMqConfiguration.ExName)) != null)
            {
                var config = conf.Get<SimpleRabbitMqConfiguration>();
                config = config.Validate();
                return config.ToConfig();
            }

            var connectionString = conf.GetValue<Uri>(ConnectionStringField)
                                   ?? throw new ArgumentNullException(ConnectionStringField);

            var exchangeConf = conf.GetExchangeConfiguration(ExchangeConfField)
                               ?? throw new ArgumentNullException(ExchangeConfField);

            var queueConf = conf.GetQueueConfiguration(QueueConfField)
                            ?? throw new ArgumentNullException(QueueConfField);

            var qosConf = conf.GetQoSConfiguration(QoSConfField);
            var callbackConf = conf.GetCallbackConfiguration(CallbackConfField);
            var onStartConf = conf.GetOnStartConfigs(OnStartConfField);
            var channelConf = conf.GetCreateChannelOptions(CreateChannelConfField);


            var builder = new RabbitMQConfigurationBuilder()
                .AddConnectionFactory(new ConnectionFactory
                {
                    Uri = connectionString
                }).AddEncoding(Encoding.UTF8)
                .AddExchangeConfiguration(exchangeConf)
                .AddQueueConfiguration(queueConf);
            if (qosConf is not null)
            {
                builder.AddQoSConfiguration(qosConf);
            }

            if (callbackConf is not null)
            {
                builder.AddCallbackConfiguration(callbackConf);
            }

            if (onStartConf is not null)
            {
                builder.AddOnStartConfiguration(onStartConf);
            }

            if (channelConf is not null)
            {
                builder.AddCreateChannelOptions(channelConf);
            }

            return builder.Build();
        }

        /// <summary>
        ///     Gets the <see cref="ExchangeConfiguration" /> from <paramref name="configuration" /> by
        ///     <paramref name="fieldName" />.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <returns></returns>
        public ExchangeConfiguration? GetExchangeConfiguration(string fieldName)
        {
            return configuration.GetSection(fieldName).Get<ExchangeConfiguration>();
        }

        /// <summary>
        ///     Gets the <see cref="QueueConfiguration" />.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <returns></returns>
        public QueueConfiguration? GetQueueConfiguration(string fieldName)
        {
            return configuration.GetSection(fieldName).Get<QueueConfiguration>();
        }

        /// <summary>
        ///     Gets the <see cref="QoSConfiguration" />.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public QoSConfiguration? GetQoSConfiguration(string fieldName)
        {
            return configuration.GetSection(fieldName).Get<QoSConfiguration>();
        }

        /// <summary>
        ///     Gets the callback conf.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public CallbackRabbitMQConfiguration? GetCallbackConfiguration(string fieldName)
        {
            return configuration.GetSection(fieldName).Get<CallbackRabbitMQConfiguration>();
        }

        /// <summary>
        ///     Gets on start configs.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public RabbitMQOnStartConfigs? GetOnStartConfigs(string fieldName)
        {
            return configuration.GetSection(fieldName).Get<RabbitMQOnStartConfigs>();
        }

        /// <summary>
        ///     Gets the create channel options.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public CreateChannelOptions? GetCreateChannelOptions(string fieldName)
        {
            return configuration.GetSection(fieldName).Get<CreateChannelOptions>();
        }
    }
}