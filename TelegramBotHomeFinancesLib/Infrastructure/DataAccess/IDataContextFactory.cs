using LinqToDB.Data;

namespace TelegramBotHomeFinancesLib.Infrastructure.DataAccess;

public interface IDataContextFactory<TDataContext>
        where TDataContext : DataConnection
{
    TDataContext CreateDataContext();
}
