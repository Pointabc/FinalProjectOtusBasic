using LinqToDB;
using LinqToDB.Async;
using TelegramBotHomeFinancesLib.Core.DataAccess;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Infrastructure.DataAccess;

internal class SqlUserRepository : IUserRepository
{
    IDataContextFactory<HomeFinanceContext> _factory;

    public SqlUserRepository(IDataContextFactory<HomeFinanceContext> factory)
    {
        _factory = factory;
    }

    public async Task Add(FinanceUser user, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var toDoUserModel = ModelMapper.MapToModel(user);
            await dbContext.InsertAsync(toDoUserModel, token: ct);
        }
    }

    public async Task<FinanceUser?> GetUser(Guid userId, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var toDoUser = await dbContext.FinanceUsers
                .Where(i => i.FinanceUserId == userId)
                .FirstOrDefaultAsync();

            return toDoUser != null ? ModelMapper.MapFromModel(toDoUser) : null;
        }
    }

    public async Task<FinanceUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var toDoUser = await dbContext.FinanceUsers
                .Where(i => i.TelegramUserId == telegramUserId)
                .FirstOrDefaultAsync();

            return toDoUser != null ? ModelMapper.MapFromModel(toDoUser) : null;
        }
    }
}
