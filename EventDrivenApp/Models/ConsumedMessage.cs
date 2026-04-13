namespace EventDrivenApp.Models;

public class ConsumedMessage
{
    public int Id { get; set; }
    public string Queue { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
