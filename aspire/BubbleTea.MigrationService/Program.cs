using BubbleTea.Services.Cart.API.Infrastructure.Database;
using BubbleTea.Services.Catalog.API.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using BubbleTea.MigrationService;
using BubbleTea.Services.Orders.API.Infrastructure.Database;
using BubbleTea.Services.Payment.Infrastructure.Database;
using BubbleTea.ServiceDefaults;

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
