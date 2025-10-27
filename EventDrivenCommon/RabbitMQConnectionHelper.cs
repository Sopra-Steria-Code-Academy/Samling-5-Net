using RabbitMQ.Client;

namespace EventDrivenCommon
{
    public static class RabbitMQConnectionHelper
    {
        public static ConnectionFactory GetConnectionFactory()
        {
            return new ConnectionFactory()
            {
                HostName = RabbitMQConst.RABBITMQ_HOST_URL,
                Port = RabbitMQConst.RABBITMQ_PORT,
                UserName = RabbitMQConst.RABBITMQ_USERNAME,
                Password = RabbitMQConst.RABBITMQ_PASSWORD,
                VirtualHost = RabbitMQConst.RABBITMQ_VIRTUAL_HOST,
                ContinuationTimeout = new TimeSpan(10, 0, 0, 0)
            };

        }
    }
}
