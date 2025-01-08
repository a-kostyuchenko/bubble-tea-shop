using System.Data.Common;
using Npgsql;
using BubbleTea.Services.Payment.Application.Abstractions.Data;

namespace BubbleTea.Services.Payment.Infrastructure.Database;

internal sealed class DbConnectionFactory(NpgsqlDataSource datasource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync()
    {
        return await datasource.OpenConnectionAsync();
    }
}
