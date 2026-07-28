namespace TelegramBotHomeFinancesLib.Infrastructure.DataAccess;

internal class DataContextFactory : IDataContextFactory<HomeFinanceContext>
{
    public HomeFinanceContext CreateDataContext()
    {
        var connectionString = Environment.GetEnvironmentVariable("HomeFinanceBotConnectionString", EnvironmentVariableTarget.User);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException("Отсутствует строка подключения к БД в переменных среды текущего пользователя");

        return new HomeFinanceContext(connectionString);
    }
}
