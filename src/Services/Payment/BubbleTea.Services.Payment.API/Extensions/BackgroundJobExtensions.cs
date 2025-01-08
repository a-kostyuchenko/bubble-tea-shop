using Hangfire;
using BubbleTea.Services.Payment.Infrastructure.Inbox;
using BubbleTea.Services.Payment.Infrastructure.Outbox;

namespace BubbleTea.Services.Payment.API.Extensions;

public static class BackgroundJobExtensions
{
    public static IApplicationBuilder UseBackgroundJobs(this WebApplication app)
    {
        IRecurringJobManager jobClient = app.Services.GetRequiredService<IRecurringJobManager>();
        
        jobClient.AddOrUpdate<IOutboxProcessor>(
            "payment-outbox-processor", 
            processor => processor.ProcessAsync(),
            app.Configuration["Outbox:Schedule"]);
        
        jobClient.AddOrUpdate<IInboxProcessor>(
            "payment-inbox-processor", 
            processor => processor.ProcessAsync(),
            app.Configuration["Inbox:Schedule"]);
        
        return app;
    }
}
