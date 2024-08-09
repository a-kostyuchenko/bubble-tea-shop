using System.Data.Common;
using Npgsql;

namespace Ordering.API.Infrastructure.Database;

internal sealed class DbConnectionFactory(NpgsqlDataSource datasource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync()
    {
        return await datasource.OpenConnectionAsync();
    }
}
