using EventDrivenCommon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;


namespace PublishMessage
{
    public class PublishMessageFunction
    {
        private readonly ILogger<PublishMessageFunction> _logger;
        private readonly IConfiguration _configuration;

        public PublishMessageFunction(ILogger<PublishMessageFunction> logger, IConfiguration Configuration)
        {
            _logger = logger;
            _configuration = Configuration;
        }

        [Function("PublishMessageFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, string message="Default Value")
        {
            using var connection = RabbitMQConnectionHelper.GetConnectionFactory(_configuration["RABBITMQ_USERNAME"], _configuration["RABBITMQ_PASSWORD"]).CreateConnection();
            using var channel = connection.CreateModel();
            
            channel.ExchangeDeclare(exchange: RabbitMQConst.RABBITMQ_EXCHANGE_NAME, type: ExchangeType.Fanout, durable: true);

            channel.QueueDeclare(queue: RabbitMQConst.RABBITMQ_QUEUE_NAME + "_RETRY",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.QueueBind(queue: RabbitMQConst.RABBITMQ_QUEUE_NAME + "_RETRY",
                              exchange: RabbitMQConst.RABBITMQ_EXCHANGE_NAME,
                              routingKey: RabbitMQConst.RABBITMQ_ROUTING_KEY);

            Console.WriteLine(" [*] Waiting for gueue.");

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: RabbitMQConst.RABBITMQ_EXCHANGE_NAME,
                                 routingKey: RabbitMQConst.RABBITMQ_ROUTING_KEY,
                                 basicProperties: properties,
                                 body: body);
            Console.WriteLine($" [x] Sent {message}");

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
