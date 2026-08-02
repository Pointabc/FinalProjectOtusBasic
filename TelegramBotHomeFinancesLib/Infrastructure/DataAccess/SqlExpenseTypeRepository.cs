using LinqToDB;
using LinqToDB.Async;
using TelegramBotHomeFinancesLib.Core.DataAccess;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Infrastructure.DataAccess;

internal class SqlExpenseTypeRepository : IExpenseTypeRepository
{
    IDataContextFactory<HomeFinanceContext> _factory;

    public SqlExpenseTypeRepository(IDataContextFactory<HomeFinanceContext> factory)
    {
        _factory = factory;
    }

    public async Task Add(ExpenseType expenseType, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var expenseTypeModel = ModelMapper.MapToModel(expenseType);
            await dbContext.InsertAsync(expenseTypeModel, token: ct);
        }
    }

    public async Task Delete(Guid expenseTypeId, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            await dbContext.ExpenseTypes
                .Where(i => i.ExpenseTypeId == expenseTypeId)
                .DeleteAsync(ct);
        }
    }

    public async Task<IReadOnlyList<ExpenseType>> Find(Guid userId, Func<ExpenseType, bool> predicate, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var allItems = await dbContext.ExpenseTypes
                .ToListAsync(ct);

            var filteredItems = allItems.Select(ModelMapper.MapFromModel).Where(predicate).ToList();

            return filteredItems.AsReadOnly();
        }
    }

    public async Task<ExpenseType?> Get(Guid expenseTypeId, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var expenseType = await dbContext.ExpenseTypes
                .LoadWith(i => i.User)
                .Where(i => i.ExpenseTypeId == expenseTypeId)
                .FirstOrDefaultAsync();

            return expenseType != null ? ModelMapper.MapFromModel(expenseType) : null;
        }
    }

    public async Task<IReadOnlyList<ExpenseType>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var expenseTypes = await dbContext.ExpenseTypes
                .LoadWith(i => i.User)
                .Where(i => i.FinanceUserId == userId)
                .ToListAsync();

            return [.. expenseTypes.Select(ModelMapper.MapFromModel)];
        }
    }
}
