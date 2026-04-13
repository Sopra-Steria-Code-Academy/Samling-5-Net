using System.Collections.Concurrent;
using System.Text;
using EventDrivenApp.Data;
using EventDrivenApp.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventDrivenApp.Services;

public record SubscriptionRequest(
    string Exchange,
    string ExchangeType,
    string Queue,
    string RoutingKey,
    bool AutoDelete = false,
    Dictionary<string, object>? Headers = null
);

public record SubscriptionInfo(
    string Exchange,
    string ExchangeType,
    string Queue,
    string RoutingKey,
    bool AutoDelete
);

public class RabbitMQConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly IConnection _connection;
    private readonly ConcurrentBag<IChannel> _channels = [];
    private readonly ConcurrentBag<SubscriptionInfo> _subscriptions = [];
    private readonly BlockingCollection<SubscriptionRequest> _pendingSubscriptions = [];

    public RabbitMQConsumerService(
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMQConsumerService> logger,
        IConnection connection)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _connection = connection;
    }

    public IReadOnlyCollection<SubscriptionInfo> ActiveSubscriptions => [.. _subscriptions];

    public void AddSubscription(SubscriptionRequest request)
    {
        _pendingSubscriptions.Add(request);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMQ consumer service started, waiting for subscriptions...");

        try
        {
            foreach (var request in _pendingSubscriptions.GetConsumingEnumerable(stoppingToken))
            {
                try
                {
                    await StartConsumerAsync(request, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to start consumer for queue {Queue}", request.Queue);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown requested
        }
    }

    private async Task StartConsumerAsync(SubscriptionRequest request, CancellationToken ct)
    {
        var channel = await _connection.CreateChannelAsync(cancellationToken: ct);
        _channels.Add(channel);

        await channel.ExchangeDeclareAsync(
            exchange: request.Exchange,
            type: request.ExchangeType,
            durable: true,
            cancellationToken: ct);

        var bindingArguments = request.Headers != null
            ? new Dictionary<string, object?>(request.Headers.Select(kv => new KeyValuePair<string, object?>(kv.Key, kv.Value)))
            : null;

        await channel.QueueDeclareAsync(
            queue: request.Queue,
            durable: true,
            exclusive: false,
            autoDelete: request.AutoDelete,
            arguments: null,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            queue: request.Queue,
            exchange: request.Exchange,
            routingKey: request.RoutingKey,
            arguments: bindingArguments,
            cancellationToken: ct);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogInformation("Received message on queue {Queue}: {Body}", request.Queue, body);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.ConsumedMessages.Add(new ConsumedMessage
                {
                    Queue = request.Queue,
                    Exchange = request.Exchange,
                    RoutingKey = ea.RoutingKey,
                    Body = body,
                    ReceivedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist message from queue {Queue}", request.Queue);
            }

            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(
            queue: request.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct);

        _subscriptions.Add(new SubscriptionInfo(
            request.Exchange,
            request.ExchangeType,
            request.Queue,
            request.RoutingKey,
            request.AutoDelete));

        _logger.LogInformation(
            "Started consuming from queue {Queue} bound to exchange {Exchange} ({Type}) with routing key '{RoutingKey}'",
            request.Queue, request.Exchange, request.ExchangeType, request.RoutingKey);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ consumer service shutting down...");
        _pendingSubscriptions.CompleteAdding();

        foreach (var channel in _channels)
        {
            try
            {
                await channel.CloseAsync(cancellationToken);
                channel.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing RabbitMQ channel");
            }
        }

        await base.StopAsync(cancellationToken);
    }
}
