using Cart.API.Infrastructure.Inbox;
using Cart.API.Infrastructure.Outbox;
using Hangfire;

namespace Cart.API.Extensions;

public static class BackgroundJobExtensions
{
    public static IApplicationBuilder UseBackgroundJobs(this WebApplication app)
    {
        IRecurringJobManager jobClient = app.Services.GetRequiredService<IRecurringJobManager>();
        
        jobClient.AddOrUpdate<IOutboxProcessor>(
            "cart-outbox-processor", 
            processor => processor.ProcessAsync(),
            app.Configuration["Outbox:Schedule"]);
        
        jobClient.AddOrUpdate<IInboxProcessor>(
            "cart-inbox-processor", 
            processor => processor.ProcessAsync(),
            app.Configuration["Inbox:Schedule"]);
        
        return app;
    }
}
