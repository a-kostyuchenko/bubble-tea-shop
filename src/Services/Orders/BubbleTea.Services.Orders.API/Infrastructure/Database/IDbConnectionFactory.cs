using System.Data.Common;

namespace BubbleTea.Services.Orders.API.Infrastructure.Database;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}
