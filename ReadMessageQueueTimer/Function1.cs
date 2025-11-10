using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EventDrivenCommon;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Configuration;


namespace ReadMessageQueueTimer
{
    public class Function1
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public Function1(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
            _configuration = configuration;
        }

        [Function("ReadMessageQueueTimer")]
        public void Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer)
        {
            var rabbitmq_username = _configuration.GetSection("RABBITMQ_USERNAME").Value;
            var rabbitmq_password = _configuration.GetSection("RABBITMQ_PASSWORD").Value;

            using var connection = RabbitMQConnectionHelper.GetConnectionFactory(rabbitmq_username, rabbitmq_password).CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: RabbitMQConst.RABBITMQ_EXCHANGE_NAME, type: ExchangeType.Fanout, durable: true);

            channel.QueueDeclare(queue: RabbitMQConst.RABBITMQ_QUEUE_NAME + "_MAIN",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.QueueBind(queue: RabbitMQConst.RABBITMQ_QUEUE_NAME+ "_MAIN",
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
            channel.BasicConsume(queue: RabbitMQConst.RABBITMQ_QUEUE_NAME+ "_MAIN",
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
