using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenCommon
{
    public static class RabbitMQConst
    {
        public static readonly string RABBITMQ_VIRTUAL_HOST = "/";
        public static readonly string RABBITMQ_HOST_URL = "localhost";//"20.251.16.156";
        public static readonly int RABBITMQ_PORT = 5672;

        public static readonly string RABBITMQ_QUEUE_NAME = "HelloWorldDLC";
        public static readonly string RABBITMQ_EXCHANGE_NAME = "CodeAcademy";
        public static readonly string RABBITMQ_ROUTING_KEY = "codeacademy-routingkey";
    }
}
