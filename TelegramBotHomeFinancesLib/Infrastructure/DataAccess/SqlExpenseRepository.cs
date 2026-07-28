using LinqToDB;
using LinqToDB.Async;
using TelegramBotHomeFinancesLib.Core.DataAccess;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Infrastructure.DataAccess;

internal class SqlExpenseRepository : IExpenseRepository
{
    IDataContextFactory<HomeFinanceContext> _factory;

    public SqlExpenseRepository(IDataContextFactory<HomeFinanceContext> factory)
    {
        _factory = factory;
    }

    public async Task Add(Expense expense, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var expenseModel = ModelMapper.MapToModel(expense);
            await dbContext.InsertAsync(expenseModel, token: ct);
        }
    }

    public async Task Delete(Guid expenseId, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var expense = await Get(expenseId, ct);
            if (expense == null)
                return;

            var expenseModel = ModelMapper.MapToModel(expense);
            await dbContext.DeleteAsync(expenseModel);
        }
    }

    public async Task<IReadOnlyList<Expense>> Find(Guid userId, Func<Expense, bool> predicate, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            // Сначала загружаем все данные из БД
            var allItems = await dbContext.Expenses
                .LoadWith(i => i.User)
                .Where(i => i.FinanceUserId == userId)
                .ToListAsync(ct);

            // Применяем предикат в памяти
            var filteredItems = allItems.Select(i => ModelMapper.MapFromModel(i)).Where(predicate).ToList();

            return filteredItems.AsReadOnly();
        }
    }

    public async Task<Expense?> Get(Guid expenseId, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var expense = await dbContext.Expenses
                .LoadWith(i => i.User)
                .Where(i => i.ExpenseId == expenseId)
                .FirstOrDefaultAsync();

            return expense != null ? ModelMapper.MapFromModel(expense) : null;
        }
    }

    public async Task<IReadOnlyList<Expense>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var expenses = await dbContext.Expenses
                .LoadWith(i => i.User)
                .Where(i => i.FinanceUserId == userId)
                .ToListAsync();

            return expenses.Select(ModelMapper.MapFromModel).ToList();
        }
    }
}
