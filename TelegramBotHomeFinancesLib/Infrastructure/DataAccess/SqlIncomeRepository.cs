using LinqToDB;
using LinqToDB.Async;
using TelegramBotHomeFinancesLib.Core.DataAccess;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Infrastructure.DataAccess;

internal class SqlIncomeRepository : IIncomeRepository
{
    IDataContextFactory<HomeFinanceContext> _factory;

    public SqlIncomeRepository(IDataContextFactory<HomeFinanceContext> factory)
    {
        _factory = factory;
    }

    public async Task Add(Income income, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var incomeModel = ModelMapper.MapToModel(income);
            await dbContext.InsertAsync(incomeModel, token: ct);
        }
    }

    public async Task Delete(Guid incomeId, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var income = await Get(incomeId, ct);
            if (income == null)
                return;

            var incomeModel = ModelMapper.MapToModel(income);
            await dbContext.DeleteAsync(incomeModel);
        }
    }

    public async Task<IReadOnlyList<Income>> Find(Guid userId, Func<Income, bool> predicate, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            // Сначала загружаем все данные из БД
            var allItems = await dbContext.Incomes
                .LoadWith(i => i.User)
                .Where(i => i.FinanceUserId == userId)
                .ToListAsync(ct);

            // Применяем предикат в памяти
            var filteredItems = allItems.Select(ModelMapper.MapFromModel).Where(predicate).ToList();

            return filteredItems.AsReadOnly();
        }
    }

    public async Task<Income?> Get(Guid incomeId, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var income = await dbContext.Incomes
                .LoadWith(i => i.User)
                .Where(i => i.IncomeId == incomeId)
                .FirstOrDefaultAsync();

            return income != null ? ModelMapper.MapFromModel(income) : null;
        }
    }

    public async Task<IReadOnlyList<Income>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var incomes = await dbContext.Incomes
                .LoadWith(i => i.User)
                .Where(i => i.FinanceUserId == userId)
                .ToListAsync();

            return [.. incomes.Select(ModelMapper.MapFromModel)];
        }
    }
}
