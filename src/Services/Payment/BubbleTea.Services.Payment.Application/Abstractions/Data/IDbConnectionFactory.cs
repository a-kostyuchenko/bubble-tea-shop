using System.Data.Common;

namespace BubbleTea.Services.Payment.Application.Abstractions.Data;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}
