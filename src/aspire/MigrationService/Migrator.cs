using System.Diagnostics;
using Cart.API.Infrastructure.Database;
using Catalog.API.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Ordering.API.Infrastructure.Database;
using Payment.Infrastructure.Database;

namespace MigrationService;

public sealed class Migrator(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName);
    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using Activity? activity = _activitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();

            DbContext[] contexts =
            [
                scope.ServiceProvider.GetRequiredService<CatalogDbContext>(),
                scope.ServiceProvider.GetRequiredService<CartDbContext>(),
                scope.ServiceProvider.GetRequiredService<OrderingDbContext>(),
                scope.ServiceProvider.GetRequiredService<PaymentDbContext>()
            ];
            
            foreach (DbContext dbContext in contexts)
            {
                await EnsureDatabaseAsync(dbContext, stoppingToken);
                await RunMigrationsAsync(dbContext, stoppingToken);
            }
            
        }
        catch (Exception e)
        {
            activity?.AddException(e);
            throw;
        }
        
        hostApplicationLifetime.StopApplication();
    }

    private static async Task EnsureDatabaseAsync<TDbContext>(TDbContext dbContext, CancellationToken cancellationToken) 
        where TDbContext : DbContext
    {
        IRelationalDatabaseCreator dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                await dbCreator.CreateAsync(cancellationToken);
            }
        });
    }
    
    private static async Task RunMigrationsAsync<TDbContext>(TDbContext dbContext, CancellationToken cancellationToken) 
        where TDbContext : DbContext
    {
        IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await dbContext.Database.MigrateAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
