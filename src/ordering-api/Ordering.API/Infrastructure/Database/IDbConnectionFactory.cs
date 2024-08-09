using System.Data.Common;

namespace Ordering.API.Infrastructure.Database;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}
