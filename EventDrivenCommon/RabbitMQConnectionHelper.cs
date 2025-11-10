using RabbitMQ.Client;

namespace EventDrivenCommon
{
    public static class RabbitMQConnectionHelper
    {
        public static ConnectionFactory GetConnectionFactory(string rabbitmq_username, string rabbitmq_password)
        {
            return new ConnectionFactory()
            {
                HostName = RabbitMQConst.RABBITMQ_HOST_URL,
                Port = RabbitMQConst.RABBITMQ_PORT,
                UserName = rabbitmq_username,
                Password = rabbitmq_password,
                VirtualHost = RabbitMQConst.RABBITMQ_VIRTUAL_HOST,
                ContinuationTimeout = new TimeSpan(10, 0, 0, 0)
            };

        }
    }
}
