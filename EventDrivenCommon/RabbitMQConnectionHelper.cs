using RabbitMQ.Client;

namespace EventDrivenCommon
{
    public static class RabbitMQConnectionHelper
    {
        public static ConnectionFactory GetConnectionFactory(string rabbitmqUsername, string rabbitmqPassword)
        {
            return new ConnectionFactory()
            {
                HostName = RabbitMQConst.RABBITMQ_HOST_URL,
                Port = RabbitMQConst.RABBITMQ_PORT,
                UserName = rabbitmqUsername,
                Password = rabbitmqPassword,
                VirtualHost = RabbitMQConst.RABBITMQ_VIRTUAL_HOST,
            };
        }
    }
}
