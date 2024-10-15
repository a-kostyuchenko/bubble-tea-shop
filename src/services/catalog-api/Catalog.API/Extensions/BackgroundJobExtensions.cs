using Catalog.API.Infrastructure.Inbox;
using Catalog.API.Infrastructure.Outbox;
using Hangfire;

namespace Catalog.API.Extensions;

public static class BackgroundJobExtensions
{
    public static IApplicationBuilder UseBackgroundJobs(this WebApplication app)
    {
        IRecurringJobManager jobClient = app.Services.GetRequiredService<IRecurringJobManager>();
        
        jobClient.AddOrUpdate<IOutboxProcessor>(
            "catalog-outbox-processor", 
            processor => processor.ProcessAsync(),
            app.Configuration["Outbox:Schedule"]);
        
        jobClient.AddOrUpdate<IInboxProcessor>(
            "catalog-inbox-processor", 
            processor => processor.ProcessAsync(),
            app.Configuration["Inbox:Schedule"]);
        
        return app;
    }
}