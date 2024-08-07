using System.Data.Common;

namespace Cart.API.Infrastructure.Database;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}
