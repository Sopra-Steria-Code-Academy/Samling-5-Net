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

        public PublishMessageFunction(ILogger<PublishMessageFunction> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Function("PublishMessageFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, string message="Default Value Code Academy")
        {
            

            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
