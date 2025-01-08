using System.Data.Common;

namespace BubbleTea.Services.Catalog.API.Infrastructure.Database;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}
