using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EventDrivenCommon;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ReadMessageQueueTimer2;

public class ReadFromRabbitMQ
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public ReadFromRabbitMQ(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<ReadFromRabbitMQ>();
        _configuration = configuration;
    }

    [Function("ReadFromRabbitMQ")]
    public void Run([TimerTrigger("*/10 * * * * *")] TimerInfo myTimer)
    {
            var rabbitmq_username = _configuration.GetSection("RABBITMQ_USERNAME").Value;
            var rabbitmq_password = _configuration.GetSection("RABBITMQ_PASSWORD").Value;

            using var connection = RabbitMQConnectionHelper.GetConnectionFactory(rabbitmq_username, rabbitmq_password).CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueBind(queue: RabbitMQConst.RABBITMQ_QUEUE_NAME+ "_RETRY",
                              exchange: RabbitMQConst.RABBITMQ_EXCHANGE_NAME,
                              routingKey: RabbitMQConst.RABBITMQ_ROUTING_KEY);

            Console.WriteLine(" [*] Waiting for logs.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] {message}");
            };
            channel.BasicConsume(queue: RabbitMQConst.RABBITMQ_QUEUE_NAME+ "_RETRY",
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
    }
}