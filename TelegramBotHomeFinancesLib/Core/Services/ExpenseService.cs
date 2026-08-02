using TelegramBotHomeFinancesLib.Core.DataAccess;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.Services;

internal class ExpenseService : IExpenseService
{
    IExpenseRepository _expenseRepository;

    public ExpenseService(IExpenseRepository expenseRepository)
    {
        _expenseRepository = expenseRepository;
    }

    public async Task<Expense> Add(FinanceUser user, ExpenseType expenseType, decimal amount, string note, CancellationToken ct)
    {
        var expense = new Expense
        {
            ExpenseId = Guid.NewGuid(),
            User = user,
            expenseType = expenseType,
            Amount = amount,
            CreatedAt = DateTime.UtcNow,
            Note = note,
        };
        await _expenseRepository.Add(expense, ct);

        return expense;
    }

    public async Task Delete(Guid expenseId, CancellationToken ct)
    {
        await _expenseRepository.Delete(expenseId, ct);
    }

    public async Task<IReadOnlyList<Expense>> Find(Guid userId, Func<Expense, bool> predicate, CancellationToken ct)
    {
        return await _expenseRepository.Find(userId, predicate, ct);
    }

    public async Task<Expense?> Get(Guid expenseId, CancellationToken ct)
    {
        return await _expenseRepository.Get(expenseId, ct);
    }

    public async Task<IReadOnlyList<Expense>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        return await _expenseRepository.GetAllByUserId(userId, ct);
    }
}
