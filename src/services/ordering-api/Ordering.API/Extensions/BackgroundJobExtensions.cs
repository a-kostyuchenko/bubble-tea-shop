using Hangfire;
using Ordering.API.Infrastructure.Inbox;
using Ordering.API.Infrastructure.Outbox;

namespace Ordering.API.Extensions;

public static class BackgroundJobExtensions
{
    public static IApplicationBuilder UseBackgroundJobs(this WebApplication app)
    {
        IRecurringJobManager jobClient = app.Services.GetRequiredService<IRecurringJobManager>();
        
        jobClient.AddOrUpdate<IOutboxProcessor>(
            "ordering-outbox-processor", 
            processor => processor.ProcessAsync(),
            app.Configuration["Outbox:Schedule"]);
        
        jobClient.AddOrUpdate<IInboxProcessor>(
            "ordering-inbox-processor", 
            processor => processor.ProcessAsync(),
            app.Configuration["Inbox:Schedule"]);
        
        return app;
    }
}
