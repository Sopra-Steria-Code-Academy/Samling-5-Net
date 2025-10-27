using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenCommon
{
    public static class RabbitMQConst
    {

        public static readonly string RABBITMQ_USERNAME = "";
        public static readonly string RABBITMQ_PASSWORD = "";
        public static readonly string RABBITMQ_VIRTUAL_HOST = "/";
        public static readonly string RABBITMQ_HOST_URL = "20.251.144.199";
        public static readonly int RABBITMQ_PORT = 5672;

        public static readonly string RABBITMQ_QUEUE_NAME = "HelloWorldDLC";
        public static readonly string RABBITMQ_EXCHANGE_NAME = "CodeAcademy";
        public static readonly string RABBITMQ_ROUTING_KEY = "codeacademy-routingkey";
    }
}
