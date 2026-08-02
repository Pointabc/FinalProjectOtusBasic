using TelegramBotHomeFinancesLib.Core.DataAccess;
using TelegramBotHomeFinancesLib.Core.Entities;

namespace TelegramBotHomeFinancesLib.Core.Services;

internal class ExpenseTypeService : IExpenseTypeService
{
    IExpenseTypeRepository _expenseTypeRepository;

    public ExpenseTypeService(IExpenseTypeRepository expenseTypeRepository)
    {
        _expenseTypeRepository = expenseTypeRepository;
    }

    public async Task<ExpenseType> Add(FinanceUser user, string name, CancellationToken ct)
    {
        var expenseType = new ExpenseType
        {
            ExpenseTypeId = Guid.NewGuid(),
            User = user,
            Name = name,
            CreatedAt = DateTime.UtcNow,
        };
        await _expenseTypeRepository.Add(expenseType, ct);

        return expenseType;
    }

    public async Task Delete(Guid expenseTypeId, CancellationToken ct)
    {
        await _expenseTypeRepository.Delete(expenseTypeId, ct);
    }

    public async Task<IReadOnlyList<ExpenseType>> Find(Guid userId, Func<ExpenseType, bool> predicate, CancellationToken ct)
    {
        return await _expenseTypeRepository.Find(userId, predicate, ct);
    }

    public async Task<ExpenseType?> Get(Guid expenseTypeId, CancellationToken ct)
    {
        return await _expenseTypeRepository.Get(expenseTypeId, ct);
    }

    public async Task<IReadOnlyList<ExpenseType>> GetAllByUserId(Guid userId, CancellationToken ct)
    {
        return await _expenseTypeRepository.GetAllByUserId(userId, ct);
    }
}
