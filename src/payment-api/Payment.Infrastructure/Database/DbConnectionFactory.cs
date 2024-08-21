using System.Data.Common;
using Npgsql;
using Payment.Application.Abstractions.Data;

namespace Payment.Infrastructure.Database;

internal sealed class DbConnectionFactory(NpgsqlDataSource datasource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync()
    {
        return await datasource.OpenConnectionAsync();
    }
}
