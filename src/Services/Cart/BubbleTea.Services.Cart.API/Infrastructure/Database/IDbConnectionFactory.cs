using System.Data.Common;

namespace BubbleTea.Services.Cart.API.Infrastructure.Database;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}
