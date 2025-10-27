using System;
using EventDrivenCommon;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ReadMessageQueue
{
    public class ReadMessageQueue
    {
        private readonly ILogger _logger;

        public ReadMessageQueue(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ReadMessageQueue>();
        }

        [Function("Function1")]
        public void Run([RabbitMQTrigger("HelloWorldDLC", ConnectionStringSetting = "RabbitMQConnectionString")] string myQueueItem)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}