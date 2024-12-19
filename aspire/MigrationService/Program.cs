using Cart.API.Infrastructure.Database;
using Catalog.API.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using MigrationService;
using Ordering.API.Infrastructure.Database;
using Payment.Infrastructure.Database;
using ServiceDefaults;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Migrator.ActivitySourceName));

builder.Services.AddDbContextPool<CatalogDbContext>((_, options) => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("catalog-db"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddDbContextPool<CartDbContext>((_, options) => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("cart-db"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddDbContextPool<OrderingDbContext>((_, options) => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("order-db"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddDbContextPool<PaymentDbContext>((_, options) => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("payment-db"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddHostedService<Migrator>();

IHost app = builder.Build();

await app.RunAsync();
