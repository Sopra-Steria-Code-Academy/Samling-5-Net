using EventDrivenCommon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace PublishMessage
{
    public class PublishMessageFunction
    {
        private readonly ILogger<PublishMessageFunction> _logger;

        public PublishMessageFunction(ILogger<PublishMessageFunction> logger)
        {
            _logger = logger;
        }

        [Function("PublishMessageFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            

            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
