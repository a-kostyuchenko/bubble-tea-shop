namespace BubbleTea.Services.Payment.Infrastructure.Outbox;

internal sealed class OutboxOptions
{
    public const string ConfigurationSection = "Outbox";
    
    public string Schedule { get; init; }
    public int BatchSize { get; set; }
}
