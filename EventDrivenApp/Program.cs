using System.Text;
using EventDrivenApp.Data;
using EventDrivenApp.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults (health checks, OpenTelemetry, service discovery)
builder.AddServiceDefaults();

// Database via Aspire integration (connection string injected by AppHost)
builder.AddNpgsqlDbContext<AppDbContext>("eventdriven");

// RabbitMQ via Aspire integration (connection string injected by AppHost)
builder.AddRabbitMQClient("messaging");

// Consumer background service
builder.Services.AddSingleton<RabbitMQConsumerService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RabbitMQConsumerService>());

// OpenAPI / Swagger
builder.Services.AddOpenApi();

var app = builder.Build();

// Apply EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.MapDefaultEndpoints();

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "EventDrivenApp"));

// ─── Endpoints ───────────────────────────────────────────────────────────────

var api = app.MapGroup("/api");

// POST /api/exchange/declare — Declare an exchange
api.MapPost("/exchange/declare", async (ExchangeDeclareRequest request, IConnection connection) =>
{
    await using var channel = await connection.CreateChannelAsync();
    await channel.ExchangeDeclareAsync(
        exchange: request.Name,
        type: request.Type,
        durable: true);

    return Results.Ok(new { message = $"Exchange '{request.Name}' declared as {request.Type}" });
})
.WithName("DeclareExchange")
.WithDescription("Declare a RabbitMQ exchange (fanout, direct, topic, headers)")
.Produces(200);

// POST /api/publish — Publish a message
api.MapPost("/publish", async (PublishRequest request, IConnection connection) =>
{
    await using var channel = await connection.CreateChannelAsync();

    var properties = new BasicProperties
    {
        Persistent = true,
    };

    if (request.Headers is { Count: > 0 })
    {
        properties.Headers = new Dictionary<string, object?>(
            request.Headers.Select(kv => new KeyValuePair<string, object?>(kv.Key, kv.Value)));
    }

    var body = Encoding.UTF8.GetBytes(request.Message);
    await channel.BasicPublishAsync(
        exchange: request.Exchange,
        routingKey: request.RoutingKey ?? string.Empty,
        mandatory: false,
        basicProperties: properties,
        body: body);

    return Results.Ok(new
    {
        message = "Message published",
        exchange = request.Exchange,
        routingKey = request.RoutingKey ?? string.Empty,
        body = request.Message
    });
})
.WithName("PublishMessage")
.WithDescription("Publish a message to a RabbitMQ exchange")
.Produces(200);

// POST /api/subscribe — Subscribe to a queue (starts consuming)
api.MapPost("/subscribe", (SubscribeRequest request, RabbitMQConsumerService consumerService) =>
{
    consumerService.AddSubscription(new SubscriptionRequest(
        Exchange: request.Exchange,
        ExchangeType: request.ExchangeType,
        Queue: request.Queue,
        RoutingKey: request.RoutingKey ?? string.Empty,
        AutoDelete: request.AutoDelete,
        Headers: request.Headers));

    return Results.Ok(new
    {
        message = $"Subscription started for queue '{request.Queue}' on exchange '{request.Exchange}'",
        exchangeType = request.ExchangeType,
        routingKey = request.RoutingKey ?? string.Empty,
        autoDelete = request.AutoDelete
    });
})
.WithName("Subscribe")
.WithDescription("Declare a queue, bind it to an exchange, and start consuming messages into the database")
.Produces(200);

// GET /api/subscriptions — List active subscriptions
api.MapGet("/subscriptions", (RabbitMQConsumerService consumerService) =>
{
    return Results.Ok(consumerService.ActiveSubscriptions);
})
.WithName("ListSubscriptions")
.WithDescription("List all active consumer subscriptions")
.Produces(200);

// GET /api/messages — Get consumed messages
api.MapGet("/messages", async (AppDbContext db, int? limit) =>
{
    var query = db.ConsumedMessages.OrderByDescending(m => m.ReceivedAt);
    var messages = limit.HasValue
        ? await query.Take(limit.Value).ToListAsync()
        : await query.ToListAsync();
    return Results.Ok(messages);
})
.WithName("GetMessages")
.WithDescription("Get consumed messages from the database")
.Produces(200);

// DELETE /api/messages — Clear all consumed messages
api.MapDelete("/messages", async (AppDbContext db) =>
{
    var count = await db.ConsumedMessages.ExecuteDeleteAsync();
    return Results.Ok(new { message = $"Deleted {count} messages" });
})
.WithName("ClearMessages")
.WithDescription("Delete all consumed messages from the database")
.Produces(200);

app.Run();

// ─── Request DTOs ────────────────────────────────────────────────────────────

public record ExchangeDeclareRequest(string Name, string Type);
public record PublishRequest(string Exchange, string Message, string? RoutingKey = null, Dictionary<string, string>? Headers = null);
public record SubscribeRequest(
    string Exchange,
    string ExchangeType,
    string Queue,
    string? RoutingKey = null,
    bool AutoDelete = false,
    Dictionary<string, object>? Headers = null);
