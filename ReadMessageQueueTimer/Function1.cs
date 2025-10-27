using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EventDrivenCommon;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ReadMessageQueueTimer
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
        {
            
        }
    }
}
