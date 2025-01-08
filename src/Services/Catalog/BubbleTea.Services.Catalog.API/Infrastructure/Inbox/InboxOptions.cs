namespace BubbleTea.Services.Catalog.API.Infrastructure.Inbox;

internal sealed class InboxOptions
{
    public const string ConfigurationSection = "Inbox";
    
    public string Schedule { get; init; }
    public int BatchSize { get; set; }
}
