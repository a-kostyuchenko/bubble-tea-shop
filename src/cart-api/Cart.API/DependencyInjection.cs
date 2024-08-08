using Cart.API.Infrastructure.Database;
using Cart.API.Infrastructure.Database.Constants;
using Cart.API.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cart.API;

internal static class DependencyInjection
{
    public static void AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton<InsertOutboxMessagesInterceptor>();
        
        builder.AddNpgsqlDbContext<CartDbContext>(
            "cart-db",
            _ => {},
            optionsBuilder =>
            {
                optionsBuilder.UseSnakeCaseNamingConvention();
        
                optionsBuilder.UseNpgsql(contextOptionsBuilder =>
                    contextOptionsBuilder.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        Schemas.Cart));

                // TODO: find a better way to add interceptors
                optionsBuilder.AddInterceptors(
                    builder.Services
                        .BuildServiceProvider()
                        .GetRequiredService<InsertOutboxMessagesInterceptor>());
            });
        
        builder.EnrichNpgsqlDbContext<CartDbContext>();
        
        builder.AddNpgsqlDataSource("cart-db");
        
        builder.Services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();
    }
}
